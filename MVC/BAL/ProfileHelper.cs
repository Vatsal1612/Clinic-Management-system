// using MVC.Models;
// using Npgsql;
// using System.Data;

// namespace MVC.BAL;

// public class ProfileHelper
// {
//     private const int MaxImageLength = 4000;
//     private readonly string _connectionString;

//     public ProfileHelper(string connectionString)
//     {
//         _connectionString = connectionString;
//     }

//     // ── Get Profile ──
//     public async Task<PatientModel?> GetProfileAsync(int patientId)
//     {
//         await using var connection = new NpgsqlConnection(_connectionString);
//         try
//         {
//             await connection.OpenAsync();

//             // Dynamic column name detection logic
//             var patientStateIdCol = await GetColumnNameAsync(connection, "t_patient", "c_stateid");
//             var patientCityIdCol = await GetColumnNameAsync(connection, "t_patient", "c_cityid");
//             var stateTableIdCol = await GetColumnNameAsync(connection, "t_state", "c_stateid");
//             var cityTableIdCol = await GetColumnNameAsync(connection, "t_city", "c_cityid");

//             var sql = $"""
//                 SELECT
//                     p.c_patientid, p.c_name, p.c_email, p.c_password, p.c_gender, p.c_mobile,
//                     p.{QuoteIdentifier(patientStateIdCol)}, 
//                     p.{QuoteIdentifier(patientCityIdCol)},
//                     COALESCE(s.c_statename, '') AS statename,
//                     COALESCE(c.c_cityname, '') AS cityname,
//                     p.c_image, p.c_role
//                 FROM t_patient p
//                 LEFT JOIN t_state s ON s.{QuoteIdentifier(stateTableIdCol)} = p.{QuoteIdentifier(patientStateIdCol)}
//                 LEFT JOIN t_city c ON c.{QuoteIdentifier(cityTableIdCol)} = p.{QuoteIdentifier(patientCityIdCol)}
//                 WHERE p.c_patientid = @patientId
//                 LIMIT 1;
//                 """;

//             await using var command = new NpgsqlCommand(sql, connection);
//             command.Parameters.AddWithValue("@patientId", patientId);

//             await using var reader = await command.ExecuteReaderAsync();
//             if (await reader.ReadAsync())
//             {
//                 return new PatientModel
//                 {
//                     c_PatientId = reader.GetInt32(0),
//                     c_Name = reader.IsDBNull(1) ? null : reader.GetString(1),
//                     c_Email = reader.IsDBNull(2) ? null : reader.GetString(2),
//                     c_Password = reader.IsDBNull(3) ? null : reader.GetString(3),
//                     c_Gender = reader.IsDBNull(4) ? null : reader.GetString(4),
//                     c_Mobile = reader.IsDBNull(5) ? null : reader.GetString(5),
//                     c_StateId = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
//                     c_CityId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
//                     c_StateName = reader.GetString(8),
//                     c_CityName = reader.GetString(9),
//                     c_Image = reader.IsDBNull(10) ? null : reader.GetString(10),
//                     c_Role = reader.IsDBNull(11) ? null : reader.GetString(11)
//                 };
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("❌ GetProfileAsync Error: " + ex.Message);
//         }
//         return null;
//     }

//     // ── Save Profile (Update / Insert) ──
//     public async Task<PatientModel?> SaveProfileAsync(PatientModel model)
//     {
//         await using var connection = new NpgsqlConnection(_connectionString);
//         try
//         {
//             await connection.OpenAsync();
//             var normalizedImage = NormalizeImageForDb(model.c_Image);

//             var patientStateIdCol = await GetColumnNameAsync(connection, "t_patient", "c_stateid");
//             var patientCityIdCol = await GetColumnNameAsync(connection, "t_patient", "c_cityid");

//             if (model.c_PatientId > 0)
//             {
//                 // UPDATE Logic
//                 var updateSql = $"""
//                     UPDATE t_patient
//                     SET c_name = @name,
//                         c_gender = @gender,
//                         c_mobile = @mobile,
//                         {QuoteIdentifier(patientStateIdCol)} = @stateId,
//                         {QuoteIdentifier(patientCityIdCol)} = @cityId,
//                         c_image = COALESCE(@image, c_image)
//                     WHERE c_patientid = @patientId;
//                     """;

//                 await using var cmd = new NpgsqlCommand(updateSql, connection);
//                 AddCommonParameters(cmd, model, normalizedImage);
//                 cmd.Parameters.AddWithValue("@patientId", model.c_PatientId);

//                 await cmd.ExecuteNonQueryAsync();
//                 return await GetProfileAsync(model.c_PatientId);
//             }
//             else
//             {
//                 // INSERT Logic
//                 var insertSql = $"""
//                     INSERT INTO t_patient
//                     (c_name, c_email, c_password, c_gender, c_mobile, {QuoteIdentifier(patientStateIdCol)}, {QuoteIdentifier(patientCityIdCol)}, c_image, c_role)
//                     VALUES
//                     (@name, @email, @password, @gender, @mobile, @stateId, @cityId, @image, @role)
//                     RETURNING c_patientid;
//                     """;

//                 await using var cmd = new NpgsqlCommand(insertSql, connection);
//                 AddCommonParameters(cmd, model, normalizedImage);
//                 cmd.Parameters.AddWithValue("@email", model.c_Email ?? (object)DBNull.Value);
//                 cmd.Parameters.AddWithValue("@password", model.c_Password ?? (object)DBNull.Value);
//                 cmd.Parameters.AddWithValue("@role", model.c_Role ?? "Patient");

//                 var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
//                 return await GetProfileAsync(newId);
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("❌ SaveProfileAsync Error: " + ex.Message);
//             return null;
//         }
//     }

//     // ── Helpers ──

//     private void AddCommonParameters(NpgsqlCommand cmd, PatientModel model, string? normalizedImage)
//     {
//         cmd.Parameters.AddWithValue("@name", model.c_Name ?? (object)DBNull.Value);
//         cmd.Parameters.AddWithValue("@gender", model.c_Gender ?? (object)DBNull.Value);
//         cmd.Parameters.AddWithValue("@mobile", model.c_Mobile ?? (object)DBNull.Value);
//         cmd.Parameters.AddWithValue("@stateId", model.c_StateId);
//         cmd.Parameters.AddWithValue("@cityId", model.c_CityId);
//         cmd.Parameters.AddWithValue("@image", (object?)normalizedImage ?? DBNull.Value);
//     }

//     private static async Task<string> GetColumnNameAsync(NpgsqlConnection conn, string tableName, string logicalCol)
//     {
//         const string sql = "SELECT column_name FROM information_schema.columns WHERE table_name = @t AND lower(column_name) = lower(@c) LIMIT 1;";
//         await using var cmd = new NpgsqlCommand(sql, conn);
//         cmd.Parameters.AddWithValue("@t", tableName);
//         cmd.Parameters.AddWithValue("@c", logicalCol);
//         var res = await cmd.ExecuteScalarAsync();
//         return res?.ToString() ?? logicalCol;
//     }

//     private static string QuoteIdentifier(string id) => $"\"{id.Replace("\"", "\"\"")}\"";

//     private static string? NormalizeImageForDb(string? img) => 
//         (!string.IsNullOrWhiteSpace(img) && img.Length <= MaxImageLength) ? img : null;
// }



using MVC.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// using MVC.Models;

namespace MVC.BAL
{
    public class ProfileHelper
    {
        private readonly NpgsqlConnection _conn;
        private const int MaxImageLength = 4000;

        public ProfileHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }

        // ── Get Profile ──
        public async Task<PatientModel?> GetProfileAsync(int patientId)
        {
            PatientModel? patient = null;
            var query = @"SELECT 
                            p.c_patientid, p.c_name, p.c_email, p.c_password, 
                            p.c_gender, p.c_mobile, p.c_stateid, p.c_cityid, 
                            p.c_image, p.c_role,
                            s.c_statename, c.c_cityname
                          FROM t_patient p
                          LEFT JOIN t_state s ON s.c_stateid = p.c_stateid
                          LEFT JOIN t_city c ON c.c_cityid = p.c_cityid
                          WHERE p.c_patientid = @id";

            try
            {
                await _conn.OpenAsync();
                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", patientId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    patient = new PatientModel
                    {
                        c_PatientId = Convert.ToInt32(reader["c_patientid"]),
                        c_Name = reader["c_name"]?.ToString(),
                        c_Email = reader["c_email"]?.ToString(),
                        c_Password = reader["c_password"]?.ToString(),
                        c_Gender = reader["c_gender"]?.ToString(),
                        c_Mobile = reader["c_mobile"]?.ToString(),
                        c_StateId = reader["c_stateid"] != DBNull.Value ? Convert.ToInt32(reader["c_stateid"]) : 0,
                        c_CityId = reader["c_cityid"] != DBNull.Value ? Convert.ToInt32(reader["c_cityid"]) : 0,
                        c_Image = reader["c_image"]?.ToString(),
                        c_Role = reader["c_role"]?.ToString(),
                        c_StateName = reader["c_statename"]?.ToString(),
                        c_CityName = reader["c_cityname"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetProfileAsync error: " + ex.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return patient;
        }

        // ── Save Profile (Insert/Update) ──
        public async Task<PatientModel?> SaveProfileAsync(PatientModel model)
        {
            try
            {
                await _conn.OpenAsync();
                // string? normalizedImage = NormalizeImageForDb(model.c_Image);

                if (model.c_PatientId > 0)
                {
                    // UPDATE Logic
                    var updateQuery = @"UPDATE t_patient 
                                        SET c_name=@name, c_gender=@gender, c_mobile=@mobile, 
                                            c_stateid=@stateId, c_cityid=@cityId, 
                                            c_image=COALESCE(@image, c_image)
                                        WHERE c_patientid=@id";

                    using var cmd = new NpgsqlCommand(updateQuery, _conn);
                    cmd.Parameters.AddWithValue("@name", model.c_Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gender", model.c_Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@mobile", model.c_Mobile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@stateId", model.c_StateId);
                    cmd.Parameters.AddWithValue("@cityId", model.c_CityId);
                    cmd.Parameters.AddWithValue("@image", model.c_Image ?? (object)DBNull.Value );
                    cmd.Parameters.AddWithValue("@id", model.c_PatientId);

                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    // INSERT Logic
                    var insertQuery = @"INSERT INTO t_patient (c_name, c_email, c_password, c_gender, c_mobile, c_stateid, c_cityid, c_image, c_role) 
                                        VALUES (@name, @email, @pass, @gender, @mobile, @stateId, @cityId, @image, @role)
                                        RETURNING c_patientid";

                    using var cmd = new NpgsqlCommand(insertQuery, _conn);
                    cmd.Parameters.AddWithValue("@name", model.c_Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", model.c_Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@pass", model.c_Password ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gender", model.c_Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@mobile", model.c_Mobile ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@stateId", model.c_StateId);
                    cmd.Parameters.AddWithValue("@cityId", model.c_CityId);
                    cmd.Parameters.AddWithValue("@image", model.c_Image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@role", model.c_Role ?? "Patient");

                    model.c_PatientId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                await _conn.CloseAsync(); // Close before calling GetProfile to avoid connection busy error
                return await GetProfileAsync(model.c_PatientId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveProfileAsync error: " + ex.Message);
                return null;
            }
            finally
            {
                if (_conn.State == System.Data.ConnectionState.Open) await _conn.CloseAsync();
            }
        }

        // ── Get States ──
        public async Task<List<StateModel>> GetAllStates()
        {
            var list = new List<StateModel>();
            try
            {
                await _conn.OpenAsync();
                var query = "SELECT c_stateid, c_statename FROM t_state ORDER BY c_statename";
                using var cmd = new NpgsqlCommand(query, _conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new StateModel
                    {
                        c_StateId = Convert.ToInt32(reader["c_stateid"]),
                        c_StateName = reader["c_statename"].ToString()
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetAllStates error: " + e.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return list;
        }

        // ── Get Cities ──
        public async Task<List<CityModel>> GetCitiesByState(int stateId)
        {
            var list = new List<CityModel>();
            try
            {
                await _conn.OpenAsync();
                var query = "SELECT c_cityid, c_cityname, c_stateid FROM t_city WHERE c_stateid=@id ORDER BY c_cityname";
                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", stateId);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new CityModel
                    {
                        c_CityId = Convert.ToInt32(reader["c_cityid"]),
                        c_CityName = reader["c_cityname"].ToString(),
                        c_StateId = Convert.ToInt32(reader["c_stateid"])
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetCities error: " + e.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return list;
        }

//         private static string? NormalizeImageForDb(string? imageData)
// {
//     if (string.IsNullOrWhiteSpace(imageData)) return null;
    
//     // Completely remove the length check, or set it to something huge like 5000000
//     return imageData; 
// }
    }
}
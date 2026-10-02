using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVC.Models;
using Npgsql;

namespace LoginReg.BAL
{
    public class AuthHelper
    {
        private readonly NpgsqlConnection _conn;

        public AuthHelper(NpgsqlConnection conn)
        {
            _conn = conn;
        }
        public async Task<PatientModel> Login(LoginModel login)
        {
            PatientModel patient = null;
            try
            {
                await _conn.OpenAsync();

                var query = @"select c_patientid,c_name,c_email,c_password,
                      c_gender,c_mobile,c_image,c_role
                      from t_patient 
                      where c_email=@email";

                using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@email", login.c_Email);
                // cmd.Parameters.AddWithValue("@pass", login.Password);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string hashPassword = reader["c_password"].ToString();

                    if (BCrypt.Net.BCrypt.Verify(login.c_Password, hashPassword))
                    {
                        patient = new PatientModel
                        {
                            c_PatientId = Convert.ToInt32(reader["c_patientid"]),
                            c_Name = reader["c_name"]?.ToString(),
                            c_Email = reader["c_email"]?.ToString(),
                            c_Gender = reader["c_gender"]?.ToString(),
                            c_Mobile = reader["c_mobile"]?.ToString(),
                            c_Image = reader["c_image"]?.ToString(),
                            c_Role = reader["c_role"]?.ToString()
                        };
                    }
                }

                await _conn.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Login error : " + e.Message);
            }

            return patient;
        }

        public async Task<int> Register(PatientModel patient)
        {
            try
            {
                await _conn.OpenAsync();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(patient.c_Password);
                var qry = "SELECT c_email FROM t_patient WHERE c_email = @c_email";
                NpgsqlCommand cmd = new NpgsqlCommand(qry, _conn);
                cmd.Parameters.AddWithValue("@c_email", patient.c_Email);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        return 0;
                    }
                }

                var query = "INSERT INTO t_patient (c_name , c_email , c_password , c_gender , c_mobile  ,c_stateId , c_cityId , c_image , c_role) VALUES (@c_name , @c_email , @c_password , @c_gender , @c_mobile  ,@c_stateId ,@c_cityId ,  @c_image , @c_role)";

                NpgsqlCommand command = new NpgsqlCommand(query, _conn);
                command.Parameters.AddWithValue("@c_name", patient.c_Name);
                command.Parameters.AddWithValue("@c_email", patient.c_Email);
                command.Parameters.AddWithValue("@c_password", hashedPassword);
                command.Parameters.AddWithValue("@c_gender", patient.c_Gender);
                command.Parameters.AddWithValue("@c_mobile", patient.c_Mobile);
                command.Parameters.AddWithValue("@c_stateId",patient.c_StateId);
                command.Parameters.AddWithValue("@c_cityId" , patient.c_CityId);
                command.Parameters.AddWithValue("@c_image", patient.c_Image);
                command.Parameters.AddWithValue("@c_role","Patient");

                await command.ExecuteNonQueryAsync();
                await _conn.CloseAsync();
                return 1;
            }
            catch(Exception ex)
            {
                System.Console.WriteLine("Register error-------->" + ex.Message);
                return 0;
            }
            finally
            {
                await _conn.CloseAsync();
            }
        }

        public async Task<List<StateModel>> GetAllState()
        {
             var list = new List<StateModel>();
            try
            {
                await _conn.OpenAsync();
                var query = "SELECT c_stateid , c_statename FROM t_state";  
                NpgsqlCommand cmd = new NpgsqlCommand(query,_conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    list.Add(new StateModel
                    {
                        c_StateId = Convert.ToInt32(reader["c_stateid"]),
                        c_StateName = reader["c_statename"].ToString()
                    });
                }
            }catch(Exception e)
            {
                System.Console.WriteLine("GetAllState error ------->"+e.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return list;
        }
        public async Task<List<CityModel>> GetCities(int StateId)
        {
            var city = new List<CityModel>();
            try
            {
                await _conn.OpenAsync();
                var query = "select c_cityid,c_cityname,c_stateid from t_city where c_stateid=@id";
                var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", StateId);

                var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    city.Add(new CityModel
                    {
                        c_CityId = (int)reader["c_cityid"],
                        c_CityName = (string)reader["c_cityname"],
                        c_StateId = (int)reader["c_stateid"],
                    });
                }

                await _conn.CloseAsync();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("City fetch error : " + e.Message);
            }
            finally
            {
                await _conn.CloseAsync();
            }
            return city;
        }

    }
}
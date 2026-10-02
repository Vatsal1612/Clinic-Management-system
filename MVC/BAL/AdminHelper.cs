using Npgsql;
using MVC.Models;

namespace MVC.BAL;

public class AdminHelper
{
  private readonly NpgsqlConnection _conn;

  public AdminHelper(NpgsqlConnection conn)
  {
    _conn = conn;
  }

  // ── Get Dashboard Data ──
  public DashboardData GetDashboardData()
  {
    var data = new DashboardData();

    var statsQuery = @"SELECT 
                            COUNT(*) AS total,
                            SUM(CASE WHEN LOWER(COALESCE(c_status, 'Pending')) = 'confirmed' THEN 1 ELSE 0 END) AS confirmed,
                            SUM(CASE WHEN LOWER(COALESCE(c_status, 'Pending')) = 'pending' THEN 1 ELSE 0 END) AS pending,
                            SUM(CASE WHEN LOWER(COALESCE(c_status, 'Pending')) IN ('cancelled', 'canceled') THEN 1 ELSE 0 END) AS cancelled
                          FROM t_appointment";

    var pendingQuery = @"SELECT a.c_appointmentid, p.c_name, d.c_departname, a.c_date, a.c_time, COALESCE(a.c_status, 'Pending') AS c_status
                            FROM t_appointment a
                            INNER JOIN t_patient p ON p.c_patientid = a.c_patientid
                            INNER JOIN t_department d ON d.c_departmentid = a.c_departmentid
                            WHERE LOWER(COALESCE(a.c_status, 'Pending')) = 'pending'
                            ORDER BY a.c_date, a.c_time
                            LIMIT 10";

    var todayQuery = @"SELECT a.c_appointmentid, p.c_name, d.c_departname, a.c_date, a.c_time, COALESCE(a.c_status, 'Pending') AS c_status
                          FROM t_appointment a
                          INNER JOIN t_patient p ON p.c_patientid = a.c_patientid
                          INNER JOIN t_department d ON d.c_departmentid = a.c_departmentid
                          WHERE a.c_date = @today
                          ORDER BY a.c_time";

    try
    {
      if (_conn.State != System.Data.ConnectionState.Open)
        _conn.Open();

      // Get stats
      using (var cmd = new NpgsqlCommand(statsQuery, _conn))
      using (var reader = cmd.ExecuteReader())
      {
        if (reader.Read())
        {
          data.TotalAppointments = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
          data.ConfirmedAppointments = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
          data.PendingAppointments = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
          data.CancelledAppointments = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
        }
      }

      // Get pending list
      data.PendingList = new List<object>();
      using (var cmd = new NpgsqlCommand(pendingQuery, _conn))
      using (var reader = cmd.ExecuteReader())
      {
        while (reader.Read())
        {
          data.PendingList.Add(new
          {
            appointmentId = reader.GetInt32(0),
            patientName = reader.GetString(1),
            departmentName = reader.GetString(2),
            appointmentDate = reader.GetDateTime(3).ToString("dd MMM yyyy"),
            appointmentTime = reader.GetTimeSpan(4).ToString(@"hh\:mm"),
            status = reader.GetString(5)
          });
        }
      }

      // Get today's schedule
      var today = DateOnly.FromDateTime(DateTime.Today);
      data.TodaySchedule = new List<object>();
      using (var cmd = new NpgsqlCommand(todayQuery, _conn))
      {
        cmd.Parameters.AddWithValue("today", today);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
          data.TodaySchedule.Add(new
          {
            appointmentId = reader.GetInt32(0),
            patientName = reader.GetString(1),
            departmentName = reader.GetString(2),
            appointmentDate = reader.GetDateTime(3).ToString("dd MMM yyyy"),
            appointmentTime = reader.GetTimeSpan(4).ToString(@"hh\:mm"),
            status = reader.GetString(5)
          });
        }
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ GetDashboardData: " + ex.Message);
    }
    finally
    {
      if (_conn.State == System.Data.ConnectionState.Open)
        _conn.Close();
    }

    return data;
  }

  // ── Get All Appointments ──
  public List<object> GetAppointments(string? search, int? departmentId, string? status)
  {
    var list = new List<object>();

    var whereClauses = new List<string>();
    var parameters = new List<NpgsqlParameter>();

    if (!string.IsNullOrWhiteSpace(search))
    {
      whereClauses.Add("(LOWER(p.c_name) LIKE @search OR LOWER(d.c_departname) LIKE @search OR CAST(a.c_appointmentid AS TEXT) LIKE @searchRaw)");
      parameters.Add(new NpgsqlParameter("search", $"%{search.Trim().ToLower()}%"));
      parameters.Add(new NpgsqlParameter("searchRaw", $"%{search.Trim()}%"));
    }

    if (departmentId.HasValue)
    {
      whereClauses.Add("a.c_departmentid = @departmentId");
      parameters.Add(new NpgsqlParameter("departmentId", departmentId.Value));
    }

    if (!string.IsNullOrWhiteSpace(status))
    {
      whereClauses.Add("LOWER(COALESCE(a.c_status, 'Pending')) = @status");
      parameters.Add(new NpgsqlParameter("status", status.Trim().ToLower()));
    }

    var whereSql = whereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", whereClauses)}" : string.Empty;

    var query = $@"SELECT a.c_appointmentid, p.c_name, d.c_departname, a.c_date, a.c_time, COALESCE(a.c_status, 'Pending') AS c_status
                     FROM t_appointment a
                     INNER JOIN t_patient p ON p.c_patientid = a.c_patientid
                     INNER JOIN t_department d ON d.c_departmentid = a.c_departmentid
                     {whereSql}
                     ORDER BY a.c_date DESC, a.c_time ASC";

    try
    {
      if (_conn.State != System.Data.ConnectionState.Open)
        _conn.Open();

      using var cmd = new NpgsqlCommand(query, _conn);
      cmd.Parameters.AddRange(parameters.ToArray());

      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        list.Add(new
        {
          appointmentId = reader.GetInt32(0),
          patientName = reader.GetString(1),
          departmentName = reader.GetString(2),
          appointmentDate = reader.GetDateTime(3).ToString("dd MMM yyyy"),
          appointmentTime = reader.GetTimeSpan(4).ToString(@"hh\:mm"),
          status = reader.GetString(5)
        });
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ GetAppointments: " + ex.Message);
    }
    finally
    {
      if (_conn.State == System.Data.ConnectionState.Open)
        _conn.Close();
    }

    return list;
  }

  // ── Get All Patients ──
  public List<object> GetPatients(string? search)
  {
    var list = new List<object>();

    var whereSql = string.Empty;
    var parameters = new List<NpgsqlParameter>();

    if (!string.IsNullOrWhiteSpace(search))
    {
      whereSql = "WHERE LOWER(p.c_name) LIKE @search OR LOWER(COALESCE(p.c_email, '')) LIKE @search OR COALESCE(p.c_mobile, '') LIKE @mobileSearch";
      parameters.Add(new NpgsqlParameter("search", $"%{search.Trim().ToLower()}%"));
      parameters.Add(new NpgsqlParameter("mobileSearch", $"%{search.Trim()}%"));
    }

    var query = $@"SELECT p.c_patientid, p.c_name, p.c_email, p.c_mobile, p.c_gender,
                      COALESCE(c.c_cityname, '') AS city_name,
                      COALESCE(s.c_statename, '') AS state_name,
                      COUNT(a.c_appointmentid) AS total_appointments
                      FROM t_patient p
                      LEFT JOIN t_city c ON c.c_cityid = p.c_cityid
                      LEFT JOIN t_state s ON s.c_stateid = p.c_stateid
                      LEFT JOIN t_appointment a ON a.c_patientid = p.c_patientid
                      {whereSql}
                      GROUP BY p.c_patientid, p.c_name, p.c_email, p.c_mobile, p.c_gender, c.c_cityname, s.c_statename
                      ORDER BY p.c_name";

    try
    {
      if (_conn.State != System.Data.ConnectionState.Open)
        _conn.Open();

      using var cmd = new NpgsqlCommand(query, _conn);
      cmd.Parameters.AddRange(parameters.ToArray());

      using var reader = cmd.ExecuteReader();
      while (reader.Read())
      {
        var city = reader.GetString(5);
        var state = reader.GetString(6);
        var location = string.IsNullOrWhiteSpace(city) ? (string.IsNullOrWhiteSpace(state) ? "" : state)
                             : (string.IsNullOrWhiteSpace(state) ? city : $"{city}, {state}");

        list.Add(new
        {
          patientId = reader.GetInt32(0),
          name = reader.GetString(1),
          email = reader.IsDBNull(2) ? null : reader.GetString(2),
          mobile = reader.IsDBNull(3) ? null : reader.GetString(3),
          gender = reader.IsDBNull(4) ? null : reader.GetString(4),
          location = location,
          totalAppointments = reader.GetInt32(7)
        });
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ GetPatients: " + ex.Message);
    }
    finally
    {
      if (_conn.State == System.Data.ConnectionState.Open)
        _conn.Close();
    }

    return list;
  }

  // ── Get Departments ──
  public List<DepartmentModel> GetDepartments()
  {
    var list = new List<DepartmentModel>();

    var query = @"SELECT c_departmentid, c_departname 
                      FROM t_department 
                      ORDER BY c_departname";

    try
    {
      if (_conn.State != System.Data.ConnectionState.Open)
        _conn.Open();

      using var cmd = new NpgsqlCommand(query, _conn);
      using var reader = cmd.ExecuteReader();

      while (reader.Read())
      {
        list.Add(new DepartmentModel
        {
          c_DepartmentId = reader.GetInt32(0),
          c_DepartmentName = reader.GetString(1)
        });
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ GetDepartments: " + ex.Message);
    }
    finally
    {
      if (_conn.State == System.Data.ConnectionState.Open)
        _conn.Close();
    }

    return list;
  }

  // ── Update Appointment Status ──
  public bool UpdateAppointmentStatus(int appointmentId, string status)
  {
    var normalized = status.Trim();
    if (string.IsNullOrWhiteSpace(normalized))
      return false;

    var query = @"UPDATE t_appointment
                      SET c_status = @status
                      WHERE c_appointmentid = @appointmentId";

    try
    {
      if (_conn.State != System.Data.ConnectionState.Open)
        _conn.Open();

      using var cmd = new NpgsqlCommand(query, _conn);
      cmd.Parameters.AddWithValue("status", normalized);
      cmd.Parameters.AddWithValue("appointmentId", appointmentId);

      int affected = cmd.ExecuteNonQuery();
      return affected > 0;
    }
    catch (Exception ex)
    {
      Console.WriteLine("❌ UpdateAppointmentStatus: " + ex.Message);
      return false;
    }
    finally
    {
      if (_conn.State == System.Data.ConnectionState.Open)
        _conn.Close();
    }
  }
}

// ── Dashboard Data Model ──
public class DashboardData
{
  public int TotalAppointments { get; set; }
  public int ConfirmedAppointments { get; set; }
  public int PendingAppointments { get; set; }
  public int CancelledAppointments { get; set; }
  public List<object> PendingList { get; set; } = new();
  public List<object> TodaySchedule { get; set; } = new();
}


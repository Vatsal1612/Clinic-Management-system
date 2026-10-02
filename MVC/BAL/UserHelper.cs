using Npgsql;
using MVC.Models;

namespace MVC.BAL;

public class UserHelper
{
    private readonly NpgsqlConnection _conn;

    public UserHelper(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    // ── Get all departments ──
    public List<DepartmentModel> GetDepartments()
    {
        var list  = new List<DepartmentModel>();
        var query = @"SELECT c_departmentid, c_departname 
                      FROM t_department 
                      ORDER BY c_departmentid";
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            using var cmd    = new NpgsqlCommand(query, _conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new DepartmentModel
                {
                    c_DepartmentId   = reader.GetInt32(0),
                    c_DepartmentName = reader.GetString(1)
                });
            }

            // Console.WriteLine("✅ Departments loaded: " + list.Count);
        }
        catch (Exception ex) { Console.WriteLine("❌ GetDepartments: " + ex.Message); }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }
        return list;
    }

    // ── Get booked slots for a dept + date ──
    public List<string> GetBookedSlots(int departmentId, int patientId,string date)
    {
        var booked = new List<string>();
        var query = @"SELECT c_time FROM t_appointment
            WHERE c_date = @date
            AND c_status != 'Cancelled'
            AND (c_departmentid = @deptId OR c_patientid = @patientId)";
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            using var cmd = new NpgsqlCommand(query, _conn);
            cmd.Parameters.AddWithValue("deptId", departmentId);
            cmd.Parameters.AddWithValue("patientId", patientId);
            cmd.Parameters.AddWithValue("date",   DateOnly.Parse(date));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // c_time is PostgreSQL TIME → read as TimeOnly → format to match slot buttons
                var time = reader.GetFieldValue<TimeOnly>(0);
                booked.Add(time.ToString("hh:mm tt"));
            }
        }
        catch (Exception ex) { Console.WriteLine("❌ GetBookedSlots: " + ex.Message); }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }
        return booked;
    }

    // ── Book appointment ──
    public (bool success, string message) BookAppointment(AppointmentModel model)
    {
        var pendingCheck = @"SELECT COUNT(*) 
                            FROM t_appointment
                            WHERE c_patientid = @patientId
                            AND c_status = 'Pending'";

        var slotCheck = @"SELECT c_patientid, c_departmentid 
                FROM t_appointment
                WHERE c_date = @date
                AND c_time = @time
                AND c_status != 'Cancelled'
                AND (c_departmentid = @deptId OR c_patientid = @patientId)";
        var insertQry = @"INSERT INTO t_appointment
                        (c_patientid, c_departmentid, c_date, c_time, c_status)
                        VALUES (@patientId, @deptId, @date, @time, 'Pending')";

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            // Check pending appointments
            using(var cmd = new NpgsqlCommand(pendingCheck, _conn))
            {
                cmd.Parameters.AddWithValue("patientId", model.c_PatientId);

                int pendingCount = Convert.ToInt32(cmd.ExecuteScalar());

                if(pendingCount >= 2)
                    return (false, "You already have 2 pending appointments.");
            }

            var timeValue = TimeOnly.Parse(model.c_Time!);

            // Check slot availability
            using(var cmd = new NpgsqlCommand(slotCheck, _conn))
            {
                cmd.Parameters.AddWithValue("deptId", model.c_DepartmentId);
                cmd.Parameters.AddWithValue("patientId", model.c_PatientId);
                cmd.Parameters.AddWithValue("date", model.c_Date);
                cmd.Parameters.AddWithValue("time", timeValue);

                using var reader = cmd.ExecuteReader();

                while(reader.Read())
                {
                    int patientId = reader.GetInt32(0);
                    int deptId = reader.GetInt32(1);

                    if(patientId == model.c_PatientId)
                        return (false, "You already have an appointment at this time.");

                    if(deptId == model.c_DepartmentId)
                        return (false, "This slot is already booked.");
                }
            }

            // Insert appointment
            using(var cmd = new NpgsqlCommand(insertQry, _conn))
            {
                cmd.Parameters.AddWithValue("patientId", model.c_PatientId);
                cmd.Parameters.AddWithValue("deptId", model.c_DepartmentId);
                cmd.Parameters.AddWithValue("date", model.c_Date);
                cmd.Parameters.AddWithValue("time", timeValue);

                cmd.ExecuteNonQuery();
            }

            return (true, "Appointment booked successfully.");
        }
        catch(Exception ex)
        {
            return (false, ex.Message);
        }
        finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }
    }
    public List<object> GetRecentAppointments(int patientId)
    {
        var list = new List<object>();

        var query = @"SELECT 
                        a.c_appointmentid,
                        d.c_departname,
                        a.c_date,
                        a.c_time,
                        a.c_status,
                        a.c_departmentid
                    FROM t_appointment a
                    JOIN t_department d
                    ON a.c_departmentid = d.c_departmentid
                    WHERE a.c_patientid = @patientId
                    ORDER BY a.c_date DESC";

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            using var cmd = new NpgsqlCommand(query, _conn);
            cmd.Parameters.AddWithValue("@patientId", patientId);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new
                {
                    id = reader.GetInt32(0),
                    department = reader.GetString(1),
                    date = reader.GetDateTime(2).ToString("dd MMM yyyy"),
                    time = reader.GetTimeSpan(3).ToString(@"hh\:mm"),
                    status = reader.GetString(4),
                    departmentId = reader.GetInt32(5)
                });
            }
        }
        finally
        {
            _conn.Close();
        }

        return list;
    }
    // Reschedule Appointment
    public void RescheduleAppointment(int id, string date, string time)
    {
        var query = @"UPDATE t_appointment
                        SET c_date=@date,
                            c_time=@time,
                            c_status='Pending'
                        WHERE c_appointmentid=@id";
        try
        {
            _conn.Open();
            using var cmd = new NpgsqlCommand(query, _conn);
            cmd.Parameters.AddWithValue("@date", DateOnly.Parse(date));
            cmd.Parameters.AddWithValue("@time", TimeOnly.Parse(time));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }catch (Exception ex)
        {
            Console.WriteLine("❌ RescheduleAppointment: " + ex.Message);
        }finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }
    }

    // Cancel Appointment
    public void CancelAppointment(int id)
    {
        string query = @"UPDATE t_appointment
                     SET c_status='Cancelled'
                     WHERE c_appointmentid=@id";

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();
            using var cmd = new NpgsqlCommand(query, _conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ CancelAppointment: " + ex.Message);
        }finally
        {
            if (_conn.State == System.Data.ConnectionState.Open)
                _conn.Close();
        }
    }
    
}
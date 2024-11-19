// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace EmployeePunchSystem
{
    public class PunchSystem
    {
        private const string ConnectionString = "Host=localhost;Port=5432;Database=EmployeeDB;Username=postgres;Password=Troyeecg1223";

        public async Task PunchAsync(int employeeId, PunchType type)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            // 查詢員工姓名
            var employeeQuery = "SELECT Name FROM Employees WHERE Id = @EmployeeId";
            using var employeeCommand = new NpgsqlCommand(employeeQuery, connection);
            employeeCommand.Parameters.AddWithValue("@EmployeeId", employeeId);

            var employeeName = await employeeCommand.ExecuteScalarAsync() as string;
            if (string.IsNullOrEmpty(employeeName))
            {
                Console.WriteLine("找不到該員工，請確認員工ID是否正確！");
                return;
            }

            // 查詢最近的打卡記錄
            var recentPunchQuery = @"SELECT PunchTime, PunchType 
                                     FROM PunchRecords 
                                     WHERE EmployeeId = @EmployeeId 
                                     ORDER BY PunchTime DESC LIMIT 1";

            using var recentCommand = new NpgsqlCommand(recentPunchQuery, connection);
            recentCommand.Parameters.AddWithValue("@EmployeeId", employeeId);

            using var reader = await recentCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var lastPunchTime = reader.GetDateTime(0);
                var lastPunchType = reader.GetString(1);

                if ((DateTime.Now - lastPunchTime).TotalMinutes < 1 && lastPunchType == type.ToString())
                {
                    Console.WriteLine("重複打卡，請稍後再試！");
                    return;
                }
            }
            reader.Close();

            // 插入新的打卡記錄
            var insertQuery = @"INSERT INTO PunchRecords (EmployeeId, PunchTime, PunchType) 
                                VALUES (@EmployeeId, @PunchTime, @PunchType)";
            using var insertCommand = new NpgsqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@EmployeeId", employeeId);
            insertCommand.Parameters.AddWithValue("@PunchTime", DateTime.Now);
            insertCommand.Parameters.AddWithValue("@PunchType", type.ToString());

            await insertCommand.ExecuteNonQueryAsync();

            Console.WriteLine($"打卡成功！\n員工姓名：{employeeName}\n時間：{DateTime.Now}\n類型：{(type == PunchType.PunchIn ? "上班" : "下班")}");
        }

        public async Task<List<PunchRecord>> GetEmployeePunchRecordsAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var punchRecords = new List<PunchRecord>();
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var query = @"SELECT PunchTime, PunchType 
                          FROM PunchRecords 
                          WHERE EmployeeId = @EmployeeId 
                          AND PunchTime BETWEEN @StartDate AND @EndDate 
                          ORDER BY PunchTime";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeId", employeeId);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                punchRecords.Add(new PunchRecord
                {
                    EmployeeId = employeeId,
                    PunchTime = reader.GetDateTime(0),
                    Type = Enum.Parse<PunchType>(reader.GetString(1))
                });
            }

            return punchRecords;
        }
    }

    public enum PunchType
    {
        PunchIn,
        PunchOut
    }

    public class PunchRecord
    {
        public int EmployeeId { get; set; }
        public DateTime PunchTime { get; set; }
        public PunchType Type { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var punchSystem = new PunchSystem();

            while (true)
            {
                Console.WriteLine("\n員工打卡系統");
                Console.WriteLine("1. 上班打卡");
                Console.WriteLine("2. 下班打卡");
                Console.WriteLine("3. 查詢打卡記錄");
                Console.WriteLine("4. 退出");
                Console.Write("請選擇功能 (1-4): ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                    case "2":
                        Console.Write("請輸入員工ID: ");
                        if (int.TryParse(Console.ReadLine(), out int empId))
                        {
                            var type = choice == "1" ? PunchType.PunchIn : PunchType.PunchOut;
                            await punchSystem.PunchAsync(empId, type);
                        }
                        break;

                    case "3":
                        Console.Write("請輸入員工ID: ");
                        if (int.TryParse(Console.ReadLine(), out int queryId))
                        {
                            Console.Write("請輸入起始日期 (yyyy-MM-dd): ");
                            DateTime.TryParse(Console.ReadLine(), out DateTime startDate);
                            Console.Write("請輸入結束日期 (yyyy-MM-dd): ");
                            DateTime.TryParse(Console.ReadLine(), out DateTime endDate);

                            var records = await punchSystem.GetEmployeePunchRecordsAsync(queryId, startDate, endDate);
                            Console.WriteLine("\n打卡記錄：");
                            foreach (var record in records)
                            {
                                Console.WriteLine($"時間：{record.PunchTime}, 類型：{(record.Type == PunchType.PunchIn ? "上班" : "下班")}");
                            }
                        }
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("無效的選擇，請重試。");
                        break;
                }
            }
        }
    }
}

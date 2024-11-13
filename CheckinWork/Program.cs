// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EmployeePunchSystem
{
    // 打卡記錄類別
    public class PunchRecord
    {
        public int EmployeeId { get; set; }
        public DateTime PunchTime { get; set; }
        public PunchType Type { get; set; }
    }

    // 打卡類型列舉
    public enum PunchType
    {
        PunchIn,
        PunchOut
    }

    // 員工類別
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
    }

    // 打卡系統主類別
    public class PunchSystem
    {
        private readonly string _logFilePath = "punch_records.txt";
        private List<PunchRecord> _punchRecords;
        private List<Employee> _employees;

        public PunchSystem()
        {
            _punchRecords = new List<PunchRecord>();
            _employees = new List<Employee>();
            LoadEmployees();
        }

        // 載入員工資料
        private void LoadEmployees()
        {
            // 示範資料，實際應用中可以從資料庫載入
            _employees.Add(new Employee { Id = 1, Name = "張三", Department = "IT部門" });
            _employees.Add(new Employee { Id = 2, Name = "李四", Department = "人資部門" });
        }

        // 打卡功能
        public void Punch(int employeeId, PunchType type)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                throw new Exception("找不到該員工");
            }

            var record = new PunchRecord
            {
                EmployeeId = employeeId,
                PunchTime = DateTime.Now,
                Type = type
            };

            _punchRecords.Add(record);
            SavePunchRecord(record);

            Console.WriteLine($"打卡成功！\n員工：{employee.Name}\n時間：{record.PunchTime}\n類型：{(type == PunchType.PunchIn ? "上班" : "下班")}");
        }

        // 儲存打卡記錄
        private void SavePunchRecord(PunchRecord record)
        {
            var logLine = $"{record.EmployeeId},{record.PunchTime},{record.Type}";
            File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
        }

        // 查詢某員工的打卡記錄
        public List<PunchRecord> GetEmployeePunchRecords(int employeeId, DateTime startDate, DateTime endDate)
        {
            return _punchRecords
                .Where(r => r.EmployeeId == employeeId &&
                           r.PunchTime.Date >= startDate.Date &&
                           r.PunchTime.Date <= endDate.Date)
                .OrderBy(r => r.PunchTime)
                .ToList();
        }
    }

    // 主程式範例
    class Program
    {
        static void Main(string[] args)
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
                        Console.Write("請輸入員工ID: ");
                        if (int.TryParse(Console.ReadLine(), out int empIdIn))
                        {
                            try
                            {
                                punchSystem.Punch(empIdIn, PunchType.PunchIn);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"錯誤：{ex.Message}");
                            }
                        }
                        break;

                    case "2":
                        Console.Write("請輸入員工ID: ");
                        if (int.TryParse(Console.ReadLine(), out int empIdOut))
                        {
                            try
                            {
                                punchSystem.Punch(empIdOut, PunchType.PunchOut);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"錯誤：{ex.Message}");
                            }
                        }
                        break;

                    case "3":
                        Console.Write("請輸入員工ID: ");
                        if (int.TryParse(Console.ReadLine(), out int queryId))
                        {
                            var records = punchSystem.GetEmployeePunchRecords(queryId, DateTime.Today.AddDays(-7), DateTime.Today);
                            Console.WriteLine("\n最近一週打卡記錄：");
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

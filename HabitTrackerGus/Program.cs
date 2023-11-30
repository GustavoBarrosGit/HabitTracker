using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace HabitTrackerGus
{
    class Program
    {
        static string connectionString = @"Data Source=HabitTracker.db";

        static void Main(string[] args)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS drinking_water (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Date TEXT,
            Quantity INTEGER
            )";

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }

            GetUserInput();
        }

        static void GetUserInput()
        {
            Console.Clear();
            bool closeApp = false;

            while (closeApp == false)
            {
                Console.WriteLine("\n\nMain Menu");
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("Type 0 to close application");
                Console.WriteLine("Type 1 to View all records");
                Console.WriteLine("Type 2 to Insert Record");
                Console.WriteLine("Type 3 to Delete Record");
                Console.WriteLine("Type 4 to Update record");
                Console.WriteLine("-----------------------\n");

                string userInput = Console.ReadLine();

                switch (userInput)
                {
                    case "0":
                        Console.WriteLine("Goodbye");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("\nInvalid Input. Plase type a number between 0 and 4");
                        break;
                }
            }
        }

        private static void Update()
        {
            Console.Clear();
            GetAllRecords();

            var recordId = GetNumberInput("\nType 0 to go back to Main Menu\nPlease type the ID of the record you want to update:\n");
            
            using(var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = @id)";
                checkCmd.Parameters.AddWithValue("@id", recordId);
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (checkQuery == 0) 
                {
                    Console.WriteLine($"\nRecord with Id {recordId} does not exist\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n");

                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText = $"UPDATE drinking_water SET date = @date, quantity = @quantity WHERE Id = @id";
                tableCmd.Parameters.AddWithValue("@date", date);
                tableCmd.Parameters.AddWithValue("@quantity", quantity);
                tableCmd.Parameters.AddWithValue("@id", recordId);

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static void Delete()
        {
            //GetAllRecords();

            var recordId = GetNumberInput("\nType 0 to go back to Main Menu\nPlease type the ID of the record you want to delete:\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = {recordId}";

                int rowCount = tableCmd.ExecuteNonQuery();

                if (rowCount == 0) 
                {
                    Console.WriteLine($"\nRecord with ID {recordId} does not exist.\n");
                    Delete();
                }
            }
            
            Console.WriteLine($"\nRecord with ID {recordId} was deleted successfully!");

            GetUserInput();
        }

        private static void GetAllRecords()
        {
            Console.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    $"SELECT * FROM drinking_water";

                List<DrinkingWater> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if(reader.HasRows)
                {
                    while(reader.Read())
                    {
                        tableData.Add(
                            new DrinkingWater
                            {
                                Id = reader.GetInt32(0),
                                Date = DateTime.ParseExact(reader.GetString(1), "dd-mm-yy", new CultureInfo("en-US")),
                                Quantity = reader.GetInt32(2)
                            }); 
                    }
                }else
                {
                    Console.WriteLine("No rows found.");
                }
                connection.Close();

                Console.WriteLine("--------------------------------------");
                foreach(var dw in tableData)
                {
                    Console.WriteLine($"{dw.Id} - {dw.Date.ToString("dd-mm-yyyy")} - Quantity: {dw.Quantity}");
                }
                Console.WriteLine("--------------------------------------");
            }
        }

        static void Insert()
        {
            string date = GetDateInput();

            int quantity = GetNumberInput("\nPlease insert number of glasses or other measure of your choice (no decimals allowed)\n\n");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();

                tableCmd.CommandText =
                    $"INSERT INTO drinking_water(date, quantity) VALUES(@date, @quantity)";
                tableCmd.Parameters.AddWithValue("@date", date);
                tableCmd.Parameters.AddWithValue("@quantity", quantity);

                tableCmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        static string GetDateInput()
        {
            Console.WriteLine("\n\nPlease input the date:\n(Format: dd-mm-yy).\t Type 0 to return to main menu");
            string dateInput = Console.ReadLine();

            if (dateInput == "0")
            {
                GetUserInput();
            }

            while(!DateTime.TryParseExact(dateInput, "dd-mm-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\nInvalid date (Format: dd-mm-yy).");
                dateInput = Console.ReadLine();
            }

            return dateInput;
        }

        static int GetNumberInput(string message)
        {
            Console.WriteLine(message);
            string numberInput = Console.ReadLine();

            if (numberInput == "0")
            {
                GetUserInput();
            }

            while(!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0)
            {
                Console.WriteLine("Invalid number. Try Again\n");
                numberInput = Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }
    }
    
    public class DrinkingWater
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Quantity { get; set; }

    }
}
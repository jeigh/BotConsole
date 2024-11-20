using System;
using System.Data.SQLite;


namespace ZwiftDataCollectionAgent.Console.DataAccess
{
    public class SQLiteGateway
    {
        private string _connectionString = "Data Source=c:\\sqlite_databases\\zwift_info.sqlite;Version=3;";

        public SQLiteGateway()
        {
            CreateTablesIfTheyDontYetExist();
        }

        public void CreateTablesIfTheyDontYetExist()
        {
            using SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Rider (
                        RiderId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AdditionalWatts INTEGER
                    );";

            using SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();

            System.Console.WriteLine($"Created Rider table");
        }




        public int GetRiderValues(int riderId)
        {
            using SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery =
                "SELECT AdditionalWatts " +
                "FROM Rider " +
                "WHERE RiderId = @RiderId";

            using SQLiteCommand command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@RiderId", riderId);

            using SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                throw new Exception("Rider not found.");

            int additionalWatts = reader.GetInt32(0);
            
            return additionalWatts;
        }

        public void UpsertRiderValues(int riderId, int additionalWatts)
        {
            using SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string upsertQuery = 
                "INSERT INTO Rider (RiderId, AdditionalWatts) " +
                "VALUES (@RiderId, @AdditionalWatts) " +
                "ON CONFLICT(RiderId) DO " +
                "UPDATE SET AdditionalWatts = excluded.AdditionalWatts;";

            using SQLiteCommand command = new SQLiteCommand(upsertQuery, connection);

            command.Parameters.AddWithValue("@RiderId", riderId);
            command.Parameters.AddWithValue("@AdditionalWatts", additionalWatts);

            command.ExecuteNonQuery();
            System.Console.WriteLine($"Upserted Watts to {additionalWatts} for rider {riderId}");
            
        }


        //public void UpdateRiderValues(int riderId, int additionalWatts)
        //{
        //    using SQLiteConnection connection = new SQLiteConnection(_connectionString);
        //    connection.Open();

        //    string updateQuery =
        //        "UPDATE Rider SET " +
        //        "   LastDraftValue = @AdditionalWatts, " +
        //        "WHERE RiderId = @RiderId";

        //    using SQLiteCommand command = new SQLiteCommand(updateQuery, connection);

        //    command.Parameters.AddWithValue("@LastDraftValue", additionalWatts);
        //    command.Parameters.AddWithValue("@RiderId", riderId);

        //    command.ExecuteNonQuery();
        //    System.Console.WriteLine($"Updated Watts to {additionalWatts} for rider {riderId}");
        //}
    }
}

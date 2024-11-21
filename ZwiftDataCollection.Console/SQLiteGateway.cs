using System;
using System.Data.SQLite;
using ZwiftClassLibrary;


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
                        CurrentWatts INTEGER,
                        CurrentCadence INTEGER,
                        MaxIdealOneMinuteWatts INTEGER,
                        MaxIdealFiveMinuteWatts INTEGER,                        
                        MaxIdealTenMinuteWatts INTEGER,
                        MaxIdealTwentyMinuteWatts INTEGER,
                        MaxIdealOneHourWatts INTEGER,
                        MaxWattsAboveThreshold INTEGER
                    );";

            using SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();

            System.Console.WriteLine($"Created Rider table");
        }




        public Rider GetRiderValues(int riderId)
        {
            using SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery =
                "SELECT " +
                "   RiderId, " +
                "   CurrentWatts, " +
                "   CurrentCadence, " +
                "   MaxIdealOneMinuteWatts, " +
                "   MaxIdealFiveMinuteWatts, " +
                "   MaxIdealTenMinuteWatts, " +
                "   MaxIdealTwentyMinuteWatts , " +
                "   MaxIdealOneHourWatts,  " +
                "   MaxWattsAboveThreshold " +                
                "FROM Rider " +
                "WHERE RiderId = @RiderId";

            using SQLiteCommand command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@RiderId", riderId);

            using SQLiteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                throw new Exception("Rider not found.");

            return new Rider
            {
                RiderId = reader.GetInt32(0),
                CurrentWatts = reader.GetInt32(1),
                CurrentCadence = reader.GetInt32(2),
                MaxIdealOneMinuteWatts = reader.GetInt32(3),
                MaxIdealFiveMinuteWatts = reader.GetInt32(4),
                MaxIdealTenMinuteWatts = reader.GetInt32(5),
                MaxIdealTwentyMinuteWatts = reader.GetInt32(6),
                MaxIdealOneHourWatts = reader.GetInt32(7),
                MaxWattsAboveThreshold = reader.GetInt32(8)
            };
        }

        public void UpsertRiderValues(Rider rider)
        {
            using SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string upsertQuery =
                "INSERT INTO Rider (RiderId, CurrentWatts, CurrentCadence, MaxIdealOneMinuteWatts, MaxIdealFiveMinuteWatts, MaxIdealTenMinuteWatts, MaxIdealTwentyMinuteWatts, MaxIdealOneHourWatts, MaxWattsAboveThreshold) " +
                "VALUES (@RiderId, @CurrentWatts, @CurrentCadence, @MaxIdealOneMinuteWatts, @MaxIdealFiveMinuteWatts, @MaxIdealTenMinuteWatts, @MaxIdealTwentyMinuteWatts, @MaxIdealOneHourWatts, @MaxWattsAboveThreshold) " +
                "ON CONFLICT(RiderId) DO " +
                "UPDATE SET " +
                "   CurrentWatts = excluded.CurrentWatts, " +
                "   CurrentCadence = excluded.CurrentCadence, " +
                "   MaxIdealOneMinuteWatts = excluded.MaxIdealOneMinuteWatts, " +
                "   MaxIdealFiveMinuteWatts = excluded.MaxIdealFiveMinuteWatts, " +
                "   MaxIdealTenMinuteWatts = excluded.MaxIdealTenMinuteWatts, " +
                "   MaxIdealTwentyMinuteWatts = excluded.MaxIdealTwentyMinuteWatts, " +
                "   MaxIdealOneHourWatts = excluded.MaxIdealOneHourWatts, " +
                "   MaxWattsAboveThreshold = excluded.MaxWattsAboveThreshold;";

            using SQLiteCommand command = new SQLiteCommand(upsertQuery, connection);

            command.Parameters.AddWithValue("@RiderId", rider.RiderId);
            command.Parameters.AddWithValue("@CurrentWatts", rider.CurrentWatts);
            command.Parameters.AddWithValue("@CurrentCadence", rider.CurrentCadence);
            command.Parameters.AddWithValue("@MaxIdealOneMinuteWatts", rider.MaxIdealOneMinuteWatts);
            command.Parameters.AddWithValue("@MaxIdealFiveMinuteWatts", rider.MaxIdealFiveMinuteWatts);
            command.Parameters.AddWithValue("@MaxIdealTenMinuteWatts", rider.MaxIdealTenMinuteWatts);
            command.Parameters.AddWithValue("@MaxIdealTwentyMinuteWatts", rider.MaxIdealTwentyMinuteWatts);
            command.Parameters.AddWithValue("@MaxIdealOneHourWatts", rider.MaxIdealOneHourWatts);
            command.Parameters.AddWithValue("@MaxWattsAboveThreshold", rider.MaxWattsAboveThreshold);

            command.ExecuteNonQuery();
        }
    }
}

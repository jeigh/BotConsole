using System;
using System.Data.SQLite;


namespace DataAccess
{
    public class SQLiteGateway
    {
        private string _connectionString = "Data Source=c:\\sqlite_databases\\zwift_info.sqlite;Version=3;";

        public Rider GetRiderValues(int riderId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
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

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@RiderId", riderId);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
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
                }
            }
        }
    }
}

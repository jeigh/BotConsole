using System;
using System.Data.SQLite;


namespace DataAccess
{
    public class SQLiteGateway
    {
        private string _connectionString = "Data Source=c:\\sqlite_databases\\zwift_info.sqlite;Version=3;";

        public SQLiteGateway()
        {
        }

        public int GetRiderValues(int riderId)
        {
            int retval = 0;
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string selectQuery =
                    "SELECT AdditionalWatts " +
                    "FROM Rider " +
                    "WHERE RiderId = @RiderId";

                using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@RiderId", riderId);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("Rider not found.");

                        retval = reader.GetInt32(0);
                    }
                }
            }
            return retval;
        }
    }
}

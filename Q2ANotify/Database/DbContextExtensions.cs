using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Database
{
    public static class DbContextExtensions
    {
        public static int ExecuteNonQuery(this IDbContext self, string commandText, params (string Name, object Value)[] values)
        {
            using (var command = self.CreateCommand(commandText, values))
            {
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(this IDbContext self, string commandText, params (string Name, object Value)[] values)
        {
            using (var reader = self.ExecuteReader(commandText, values))
            {
                if (reader.Read())
                    return reader[0];

                return null;
            }
        }

        public static SQLiteDataReader ExecuteReader(this IDbContext self, string commandText, params (string Name, object Value)[] values)
        {
            using (var command = self.CreateCommand(commandText, values))
            {
                return command.ExecuteReader();
            }
        }
    }
}

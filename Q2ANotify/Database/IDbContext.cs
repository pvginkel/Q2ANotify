using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Database
{
    public interface IDbContext : IDisposable
    {
        long LastInsertRowId { get; }

        void Commit();

        SQLiteCommand CreateCommand(string commandText = null, params (string Name, object Value)[] values);
    }
}
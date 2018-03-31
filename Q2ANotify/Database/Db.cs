using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Q2ANotify.Database
{
    public class Db : IDisposable
    {
        private SQLiteConnection _connection;
        private readonly object _syncRoot = new object();
        private bool _disposed;

        public Db(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            _connection = new SQLiteConnection("data source=" + path);
            _connection.Open();

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = "pragma foreign_keys=ON";
                command.ExecuteNonQuery();

                command.CommandText = "pragma journal_mode=TRUNCATE";
                command.ExecuteNonQuery();

                command.CommandText = "pragma locking_mode=EXCLUSIVE";
                command.ExecuteNonQuery();

                command.CommandText = "vacuum";
                command.ExecuteNonQuery();
            }


            RunMigrations();
        }

        private void RunMigrations()
        {
            using (var ctx = OpenContext())
            {
                bool hasMigration = (long)ctx.ExecuteScalar(
                    "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = @table",
                    ("@table", "migration")
                ) != 0;

                int currentVersion = 0;

                if (hasMigration)
                    currentVersion = (int)ctx.ExecuteScalar("SELECT version FROM migration");

                var scripts = FindScriptNames();

                foreach (var script in scripts)
                {
                    if (script.Version <= currentVersion)
                        continue;

                    string sql;

                    using (var stream = GetType().Assembly.GetManifestResourceStream(script.Name))
                    using (var reader = new StreamReader(stream))
                    {
                        sql = reader.ReadToEnd();
                    }

                    ctx.ExecuteNonQuery(sql);

                    if (hasMigration)
                    {
                        ctx.ExecuteNonQuery(
                            "UPDATE migration SET version = @version",
                            ("@version", script.Version)
                        );
                    }
                    else
                    {
                        ctx.ExecuteNonQuery(
                            "INSERT INTO migration (version) VALUES (@version)",
                            ("@version", script.Version)
                        );
                    }

                    currentVersion = script.Version;
                    hasMigration = true;
                }

                ctx.Commit();
            }
        }

        private List<(int Version, string Name)> FindScriptNames()
        {
            var result = new List<(int Version, string Name)>();
            string prefix = GetType().Namespace + ".Migration.";

            foreach (string resourceName in GetType().Assembly.GetManifestResourceNames())
            {
                if (resourceName.StartsWith(prefix))
                {
                    string shortName = resourceName.Substring(prefix.Length);
                    int pos = shortName.IndexOf('-');
                    if (pos != -1)
                    {
                        int version = int.Parse(shortName.Substring(0, pos));
                        result.Add((version, resourceName));
                    }
                }
            }

            result.Sort((a, b) => a.Version.CompareTo(b.Version));

            return result;
        }

        public IDbContext OpenContext()
        {
            return new DbContext(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }

                _disposed = true;
            }
        }

        private class DbContext : IDbContext
        {
            private readonly Db _db;
            private SQLiteTransaction _transaction;
            private bool _commit;
            private bool _disposed;

            public DbContext(Db db)
            {
                _db = db;

                // Enter the lock to ensure we serialize data access.
                Monitor.Enter(db._syncRoot);

                _transaction = db._connection.BeginTransaction();
            }

            public long LastInsertRowId => _db._connection.LastInsertRowId;

            public void Commit()
            {
                _commit = true;
            }

            public SQLiteCommand CreateCommand(string commandText, params (string Name, object Value)[] values)
            {
                var command = _db._connection.CreateCommand();
                command.Transaction = _transaction;
                command.CommandText = commandText;

                if (values != null)
                {
                    foreach (var value in values)
                    {
                        command.Parameters.AddWithValue(value.Name, value.Value ?? DBNull.Value);
                    }
                }

                return command;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    try
                    {
                        // Commit or rollback the transaction.
                        using (_transaction)
                        {
                            if (_commit)
                                _transaction.Commit();
                            else
                                _transaction.Rollback();
                        }
                    }
                    finally
                    {
                        _transaction = null;

                        // Exit the lock.
                        Monitor.Exit(_db._syncRoot);
                    }

                    _disposed = true;
                }
            }
        }
    }
}

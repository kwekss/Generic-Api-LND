using helpers.Database.Executors;
using helpers.Database.Extensions;
using helpers.Database.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Database
{
    public class DBHelper : IDBHelper
    {
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;
        private readonly DatabaseConnections _connections;

        public DBHelper(IStoredProcedureExecutor storedProcedureExecutor, DatabaseConnections connections)
        {
            _storedProcedureExecutor = storedProcedureExecutor;
            _connections = connections;
        }

        public DatabaseConnections GetConnections() => _connections;
        public async Task Subscribe(string tag, NotificationEventHandler handler)
        {
            _storedProcedureExecutor.Subscribe(_connections.Default, tag, handler);
        }
        public async Task<List<T>> Fetch<T>(string procedureName, List<StoreProcedureParameter> parameters)
        {
            return await _storedProcedureExecutor.ExecuteStoredProcedure<T>(_connections.Default, $"\"{procedureName}\"", parameters);
        }
        public async Task<List<T>> Fetch<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            return await _storedProcedureExecutor.ExecuteStoredProcedure<T>(connection, $"\"{procedureName}\"", parameters);
        }
        public async Task<T> ExecuteRaw<T>(string procedureName, List<StoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _storedProcedureExecutor.ExecuteStoredProcedure(_connections.Default, $"\"{procedureName}\"", parameters, (reader) =>
            {
                if (reader.Read()) t = reader.Get<T>($"{procedureName}");
            });

            return t;
        }
        public async Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _storedProcedureExecutor.ExecuteStoredProcedure(connection, $"\"{procedureName}\"", parameters, (reader) =>
            {
                if (reader.Read()) t = reader.Get<T>($"{procedureName}");
            });

            return t;
        }
        public async Task ExecuteRaw(string procedureName, List<StoreProcedureParameter> parameters)
        {
            await _storedProcedureExecutor.ExecuteStoredProcedure(_connections.Default, $"\"{procedureName}\"", parameters);
        }
        public async Task ExecuteRaw(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            await _storedProcedureExecutor.ExecuteStoredProcedure(connection, $"\"{procedureName}\"", parameters);
        }
    }
}

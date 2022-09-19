using helpers.Database.Executors;
using helpers.Database.Extensions;
using helpers.Database.Models;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace helpers.Database
{
    public class DBHelper : IDBHelper
    {
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;
        private readonly List<Connection> _connections;
        private readonly Connection _defaultConnection;

        public DBHelper(IStoredProcedureExecutor storedProcedureExecutor, List<Connection> connections)
        {
            _storedProcedureExecutor = storedProcedureExecutor;
            _connections = connections;
            _defaultConnection = GetConnection("Default");
        }

        public List<Connection> GetConnections() => _connections;
        public Connection GetConnection(string connectionName)
        {
            return _connections.FirstOrDefault(_ => _.Name.ToLower() == connectionName.ToLower());
        }
        public async Task Subscribe(string tag, NotificationEventHandler handler)
        {
            _storedProcedureExecutor.Subscribe(_defaultConnection, tag, handler);
        }
        public async Task<List<T>> Fetch<T>(string procedureName, List<StoreProcedureParameter> parameters)
        {
            return await _storedProcedureExecutor.ExecuteStoredProcedure<T>(_defaultConnection, $"\"{procedureName}\"", parameters);
        }
        public async Task<List<T>> Fetch<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            return await _storedProcedureExecutor.ExecuteStoredProcedure<T>(connection, $"\"{procedureName}\"", parameters);
        }
        public async Task<T> ExecuteRaw<T>(string procedureName, List<StoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _storedProcedureExecutor.ExecuteStoredProcedure(_defaultConnection, $"\"{procedureName}\"", parameters, (reader) =>
            {
                if (reader.Read())
                    t = reader.Get<T>(procedureName);
            });

            return t;
        }
        public async Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _storedProcedureExecutor.ExecuteStoredProcedure(connection, $"\"{procedureName}\"", parameters, (reader) =>
            {
                if (reader.Read()) t = reader.Get<T>($"\"{procedureName}\"");
            });

            return t;
        }
        public async Task ExecuteRaw(string procedureName, List<StoreProcedureParameter> parameters)
        {
            await _storedProcedureExecutor.ExecuteStoredProcedure(_defaultConnection, $"\"{procedureName}\"", parameters);
        }
        public async Task ExecuteRaw(Connection connection, string procedureName, List<StoreProcedureParameter> parameters)
        {
            await _storedProcedureExecutor.ExecuteStoredProcedure(connection, $"\"{procedureName}\"", parameters);
        }
    }
}

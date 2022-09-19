using helpers.Database.Executors;
using helpers.Database.Extensions;
using helpers.Database.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace helpers.Database
{
    public class OracleDBHelper : IOracleDBHelper
    {
        private readonly IOracleExecutor _oracleExecutor;
        private readonly List<Connection> _connections;
        private readonly Connection _defaultConnection;

        public OracleDBHelper(IOracleExecutor oracleExecutor, List<Connection> connections)
        {
            _oracleExecutor = oracleExecutor;
            _connections = connections;
            _defaultConnection = GetConnection("Oracle");
        }

        public List<Connection> GetConnections() => _connections;
        public Connection GetConnection(string connectionName)
        {
            return _connections.FirstOrDefault(_ => _.Name.ToLower() == connectionName.ToLower());
        }
        public async Task<List<T>> Fetch<T>(string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            var response = new List<T>();
            await _oracleExecutor.ExecuteStoredProcedure(_defaultConnection, $"{procedureName}", parameters, (reader) =>
            {
                response = reader.toModel<T>();
            });

            return response;
        }
        public async Task<List<T>> QueryFetch<T>(string query, List<OracleStoreProcedureParameter> parameters = null)
        {
            var response = new List<T>();
            await _oracleExecutor.ExecuteQuery(_defaultConnection, $"{query}", (reader) =>
            {
                response = reader.toModel<T>();
            });

            return response;
        }
        public async Task ExecuteNonQuery(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            await _oracleExecutor.ExecuteStoredProcedure(connection, $"{procedureName}", parameters);
        }

        public async Task<List<T>> QueryFetch<T>(Connection connection, string query, List<OracleStoreProcedureParameter> parameters = null)
        {
            var response = new List<T>();
            await _oracleExecutor.ExecuteQuery(connection, $"{query}", (reader) =>
            {
                response = reader.toModel<T>();
            });

            return response;
        }
        public async Task ExecuteNonQuery(string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            await _oracleExecutor.ExecuteStoredProcedure(_defaultConnection, $"{procedureName}", parameters);
        }

        public async Task<T> ExecuteRaw<T>(string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _oracleExecutor.ExecuteStoredProcedure(_defaultConnection, $"{procedureName}", parameters, (reader) =>
            {
                if (reader.Read())
                    t = reader.Get<T>(procedureName);
            });

            return t;
        }
        public async Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            var t = default(T);
            await _oracleExecutor.ExecuteStoredProcedure(connection, $"{procedureName}", parameters, (reader) =>
            {
                if (reader.Read()) t = reader.Get<T>($"{procedureName}");
            });

            return t;
        }
        public async Task ExecuteRaw(string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            await _oracleExecutor.ExecuteStoredProcedure(_defaultConnection, $"\"{procedureName}\"", parameters);
        }
        public async Task ExecuteRaw(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters)
        {
            await _oracleExecutor.ExecuteStoredProcedure(connection, $"\"{procedureName}\"", parameters);
        }
    }
}

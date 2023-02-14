using helpers.Database.Models;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Database
{
    public interface IOracleDBHelper
    {
        Task ExecuteNonQuery(string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task ExecuteNonQuery(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task<OracleParameterCollection> ExecuteNonQueryOut(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task ExecuteRaw(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task ExecuteRaw(string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task<T> ExecuteRaw<T>(string procedureName, List<OracleStoreProcedureParameter> parameters);
        Task<List<T>> Fetch<T>(string procedureName, List<OracleStoreProcedureParameter> parameters);
        Connection GetConnection(string connectionName);
        List<Connection> GetConnections();
        Task<List<T>> QueryFetch<T>(Connection connection, string query, List<OracleStoreProcedureParameter> parameters = null);
        Task<List<T>> QueryFetch<T>(string query, List<OracleStoreProcedureParameter> parameters = null);
    }
}
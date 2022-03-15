using helpers.Database.Models;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Database
{
    public interface IDBHelper
    {
        Task<T> ExecuteRaw<T>(string procedureName, List<StoreProcedureParameter> parameters);
        Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        Task ExecuteRaw(string procedureName, List<StoreProcedureParameter> parameters);
        Task ExecuteRaw(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        Task<List<T>> Fetch<T>(string procedureName, List<StoreProcedureParameter> parameters);
        Task<List<T>> Fetch<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        DatabaseConnections GetConnections();
        Task Subscribe(string tag, NotificationEventHandler handler);
    }
}
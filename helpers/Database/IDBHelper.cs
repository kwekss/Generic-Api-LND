using helpers.Database.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace helpers.Database
{
    public interface IDBHelper
    {
        Task Execute(string procedureName, List<StoreProcedureParameter> parameters, Action<IDataReader> callback = null);
        Task<T> ExecuteRaw<T>(string procedureName, List<StoreProcedureParameter> parameters);
        Task<T> ExecuteRaw<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        Task ExecuteRaw(string procedureName, List<StoreProcedureParameter> parameters);
        Task ExecuteRaw(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        Task<List<T>> Fetch<T>(string procedureName, List<StoreProcedureParameter> parameters);
        Task<List<T>> Fetch<T>(Connection connection, string procedureName, List<StoreProcedureParameter> parameters);
        Connection GetConnection(string connectionName);
        List<Connection> GetConnections();
        Task Subscribe(string tag, NotificationEventHandler handler);
    }
}
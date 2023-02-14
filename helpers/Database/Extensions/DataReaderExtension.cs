using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace helpers.Database.Extensions
{
    public static class DataReaderExtension
    {
        public static bool IsDBNull(this IDataReader dataReader, string columnName)
        {
            return dataReader[columnName] == DBNull.Value;
        }
        public static T Get<T>(this IDataReader dataReader, string columnName)
        {
            try
            {
                return IsDBNull(dataReader, columnName) ? default(T) : (T)dataReader[columnName];
            }
            catch (Exception)
            {
                return default(T);
            }

        }

        private static T ObjectFromDictionary<T>(Dictionary<string, dynamic> dict)
        {
            Type type = typeof(T);
            T result = type is string ? default(T) : (T)Activator.CreateInstance(type);
            var json_obj_start = new string[] { "{", "[" };
            foreach (var item in dict)
            {
                var prop = type?.GetProperty(item.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) continue;
                Type t = Nullable.GetUnderlyingType(prop?.PropertyType) ?? prop.PropertyType;
                object? safeValue = item.Value;
                try
                {
                    if (safeValue is DBNull && t != typeof(string) && t != typeof(DateTime)) safeValue = Activator.CreateInstance(t);
                    if (t == typeof(string))
                    {
                        safeValue = $"{safeValue}";
                    }
                    else if (safeValue is DBNull && t == typeof(DateTime))
                    {
                        safeValue = null;
                    }
                    else if (t != typeof(string) && json_obj_start.Any(x => $"{safeValue}".StartsWith(x)))
                    {
                        if (safeValue != null) safeValue = JsonConvert.DeserializeObject(item.Value, t);
                    }
                    else
                    {
                        if (safeValue != null) safeValue = Convert.ChangeType(safeValue, t);
                    }
                    prop?.SetValue(result, safeValue, null);

                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }

        public static List<T>? toModel<T>(this IDataReader reader)
        {
            var r = reader.Serialize();
            var data = r?.Select(row => ObjectFromDictionary<T>(row))?.ToList();
            return data;
        }

        public static IEnumerable<Dictionary<string, dynamic>> Serialize(this IDataReader reader)
        {
            var results = new List<Dictionary<string, dynamic>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        private static Dictionary<string, dynamic> SerializeRow(IEnumerable<string> cols, IDataReader reader)
        {
            var result = new Dictionary<string, dynamic>();
            foreach (var col in cols)
                result.Add(col, reader[col]);

            return result;
        }

    }
}

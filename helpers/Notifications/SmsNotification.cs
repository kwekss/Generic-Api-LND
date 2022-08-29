using helpers.Database;
using helpers.Database.Models;
using Microsoft.Extensions.Configuration;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace helpers.Notifications
{
    public class SmsNotification : ISmsNotification
    {
        private readonly IDBHelper _dbHelper;
        private readonly string _senderId;

        public SmsNotification(IConfiguration config, IDBHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _senderId = config["SMS:SenderId"] ?? "MTN";
        }

        public async Task<string> Dispatch(string recipient, string content, params dynamic[] extra)
        {
            string senderId;
            if (extra != null && extra.Any())
                senderId = extra[0].ToString();
            else
                senderId = _senderId;

            var parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter { Name="reqSender", Type = NpgsqlDbType.Varchar, Value = senderId },
                new StoreProcedureParameter { Name="reqMessageAndMsisdns", Type = NpgsqlDbType.Varchar, Value = $"{recipient}||{content}" }
            };

            await _dbHelper.ExecuteRaw(_dbHelper.GetConnections().SMS, "ScheduleBulkSmsBulkRecipients_PipeSeparated", parameters);

            var response = $"Message sent to: {recipient} at {DateTime.UtcNow} with content: {content.Substring(0, Math.Min(content.Length, content.Length > 50 ? 50 : content.Length))}...";

            Event.Dispatch("log", response);

            return response;
        }
    }
}

using helpers.Database;
using Microsoft.AspNetCore.Http;
using models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace helpers.Session
{
    public class SessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMongoDBHelper _mongoDBHelper;
        private readonly Timer _update_timer;

        public SessionManager(IHttpContextAccessor httpContextAccessor, IMongoDBHelper mongoDBHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _mongoDBHelper = mongoDBHelper;
            _update_timer = new Timer(new TimerCallback(RunAutoDelete));
            _update_timer.Change(1000, 0);
        }

        public async Task<AppUserSession> GetAuthorizedUser()
        {
            var authorizationString = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authorizationString)) return null;
            if (!authorizationString.StartsWith("Bearer")) return null;

            authorizationString = authorizationString.Trim();
            string[] authorizationArray = authorizationString.Split(Convert.ToChar(" "));

            if (!authorizationArray.Any()) return null;
            if (authorizationArray.Length != 2) return null;
            var sessionKey = authorizationArray[1].Trim();
            var collection = _mongoDBHelper.GetCollection<AppUserSession>("ApplicationUserSession", sessionKey);

            var filter = Builders<AppUserSession>.Filter;
            var sessionKeyFilter = filter.Eq(x => x.SessionKey, sessionKey);

            var finalFilter = filter.And(sessionKeyFilter);
            var details = await collection.FindAsync<AppUserSession>(finalFilter);
            var session = details.FirstOrDefault();
            if (session != null) return session;

            return null;
        }
        public async Task<AppUserSession> GetCurrentUserSession()
        {
            var authorizationString = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(authorizationString)) return null;
            if (!authorizationString.StartsWith("Bearer")) return null;

            authorizationString = authorizationString?.Trim();
            string[] authorizationArray = authorizationString.Split(Convert.ToChar(" "));

            if (!authorizationArray.Any()) return null;
            if (authorizationArray.Length != 2) return null;
            var sessionKey = authorizationArray[1].Trim();
            var collection = _mongoDBHelper.GetCollection<AppUserSession>("ApplicationUserSession", sessionKey);

            var filter = Builders<AppUserSession>.Filter;
            var sessionKeyFilter = filter.Eq(x => x.SessionKey, sessionKey);

            var finalFilter = filter.And(sessionKeyFilter);
            var session = (await collection.FindAsync<AppUserSession>(finalFilter)).FirstOrDefault();
            //var session = details.FirstOrDefault();
            if (session != null) return session;

            return null;
        }

        public async Task<AppUserSession> SetAuthorizedUser<T>(T userData)
        {
            var token = Guid.NewGuid().ToString();
            var collection = _mongoDBHelper.GetCollection<AppUserSession>("ApplicationUserSession", token);
            var data = new AppUserSession
            {
                SessionKey = token,
                DateModified = DateTime.UtcNow,
                Data = userData.Stringify()
            };

            _ = collection.InsertOneAsync(data);

            return data;
        }
        private void RunAutoDelete(object sender)
        {
            _ = AutoDeleteSessionData();
        }
        private async Task AutoDeleteSessionData()
        {
            var filter = Builders<AppUserSession>.Filter;
            var sessionFilter = filter.Lt(x => x.DateModified.AddHours(6), DateTime.Now);
            for (int i = 0; i <= 20; i++)
            {
                var collection = _mongoDBHelper.GetCollection<AppUserSession>($"ApplicationUserSession_{i}");
                collection.DeleteOne(sessionFilter);
            }
            await Task.Delay(TimeSpan.FromHours(1));
            _update_timer.Change(1000, 0);
        }
        public async Task DeleteSessionData(string token)
        {
            var filter = Builders<AppUserSession>.Filter;
            var sessionFilter = filter.Eq(x => x.SessionKey, token);
            var collection = _mongoDBHelper.GetCollection<AppUserSession>("ApplicationUserSession", token);
            _ = collection.DeleteOne(sessionFilter);
        }
    }
}

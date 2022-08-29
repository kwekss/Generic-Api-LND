using MongoDB.Bson;
using System;

namespace models
{
    public class AppUserSession
    {
        public ObjectId Id { get; set; }
        public DateTime DateModified { get; set; }
        public string SessionKey { get; set; }
        public string Data { get; set; }
    }
}

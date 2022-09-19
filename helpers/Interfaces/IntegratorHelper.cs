using models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public class IntegratorHelper : IIntegratorHelper
    {
        protected static List<Integrator> inMemoryIntegratorList;
        public const string InvalidAuthHeader = "Invalid authorization header";
        public const string InvalidIntegratorMessage = "Integrator account does not exist";
        public const string RequestTooOldMessage = "Invalid request. Request time is too old";
        public const string RequestInFutureMessage = "Invalid request. Request time cannot be in future";
        public const string InvalidRequestDate = "Invalid request. Request timestamp is invalid";
        public const string InvalidEncryptionMessage = "Invalid encryption provided. Integrator Code / Secret Key might be invalid";
        public const string InactiveIntegratorAccountMessage = "Integrator account disabled";
        public const string InvalidauthorizationStringMessage = "Invalid authorization string provided";
        public const string dateFormat = "yyyyMMddHHmmss";

        public IntegratorHelper()
        {
            inMemoryIntegratorList = GetIntegrators();
        }
        public async Task<string> ValidateIntegrator(string authorizationString, DateTime requestTime)
        {
            if (string.IsNullOrEmpty(authorizationString)) return InvalidAuthHeader;

            authorizationString = authorizationString.Trim();
            string[] authorizationArray = authorizationString.Split(Convert.ToChar(" "));
            if (!authorizationArray.Any()) return InvalidIntegratorMessage;
            if (authorizationArray.Length > 2) return InvalidauthorizationStringMessage;
            if (authorizationArray.Length < 2) return InvalidauthorizationStringMessage;

            var IntegratorCode = authorizationArray[0];
            var IntegratorEncryptedBody = authorizationArray[1]; // generated from Integrator Code + Integrator Secret Key

            var selectedIntegrator = inMemoryIntegratorList.FirstOrDefault(integrator => integrator.IntegratorCode == IntegratorCode);
            if (selectedIntegrator == null) return InvalidIntegratorMessage;
            var now = DateTime.Now;

            if (now.Year != requestTime.Year || now.Month != requestTime.Month || now.Day != requestTime.Day) throw new InvalidOperationException(InvalidRequestDate);
            var minuteIntervals = now.Minute - requestTime.Minute;

            if (minuteIntervals < -2) return RequestInFutureMessage;
            if (minuteIntervals > 2) return RequestTooOldMessage;

            var selectedIntegratorEnc = Sha256encrypt(requestTime, selectedIntegrator.IntegratorToken);
            if (selectedIntegratorEnc != IntegratorEncryptedBody) throw new InvalidOperationException(InvalidEncryptionMessage);
            return null;
        }

        public string Sha256encrypt(DateTime requestTime, string integratorSecretKey)
        {
            var phrase = $"{requestTime.ToString(dateFormat)}{integratorSecretKey}";
            byte[] hash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(phrase));
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += string.Format("{0:x2}", x);
            }
            return hashString;
        }

        private List<Integrator> GetIntegrators()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Integrators.json");
            if (!File.Exists(path)) return null;

            using (var reader = new StreamReader(path))
            {
                var result = reader.ReadToEnd();
                var integrators = JsonConvert.DeserializeObject<List<Integrator>>(result);
                return integrators;
            }
        }

        /*  JS Equivalent
            var hash = CryptoJS.SHA256("abc-def-ghi");
            var hexhash = hash.toString(CryptoJS.enc.hex);
            console.log(hexhash);
         */
    }
}

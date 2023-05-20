using helpers.Engine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public class IntegratorHelper : IIntegratorHelper
    {
        public const string InvalidAuthHeader = "Invalid authorization header";
        public const string InvalidIntegratorMessage = "Integrator account does not exist";
        public const string RequestTooOldMessage = "Invalid request. Request time is too old";
        public const string RequestInFutureMessage = "Invalid request. Request time cannot be in future";
        public const string InvalidRequestDate = "Invalid request. Request timestamp is invalid";
        public const string InvalidEncryptionMessage = "Invalid encryption provided. Integrator Code / Secret Key might be invalid";
        public const string InactiveIntegratorAccountMessage = "Integrator account disabled";
        public const string InvalidauthorizationStringMessage = "Invalid authorization string provided";
        public const string dateFormat = "yyyyMMddHHmmss";

        private readonly IIntegratorStorage _integratorStorage;

        public IntegratorHelper(IIntegratorStorage integratorStorage)
        {
            _integratorStorage = integratorStorage;
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

            var integrators = await _integratorStorage.GetIntegrators();

            var selectedIntegrator = integrators.FirstOrDefault(integrator => integrator.IntegratorCode == IntegratorCode);
            if (selectedIntegrator == null) return InvalidIntegratorMessage;
            var now = DateTime.Now;

            if (now.Year != requestTime.Year || now.Month != requestTime.Month || now.Day != requestTime.Day) throw new InvalidOperationException(InvalidRequestDate);
            var minuteIntervals = now.Minute - requestTime.Minute;

            if (minuteIntervals < -2) return RequestInFutureMessage;
            if (minuteIntervals > 2) return RequestTooOldMessage;

            var selectedIntegratorEnc = Utility.EncryptSha256($"{requestTime.ToString(dateFormat)}{selectedIntegrator.IntegratorToken}");
            if (selectedIntegratorEnc != IntegratorEncryptedBody) throw new InvalidOperationException(InvalidEncryptionMessage);
            return null;
        } 
    }
}

using AuthService.Models;
using AuthService.Providers;
using helpers;
using helpers.Atttibutes;
using helpers.Database;
using helpers.Database.Models;
using models;
using NpgsqlTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Features
{
    [Feature(Name = "VerifyUserPass")]
    public class VerifyUserCredentialsFeature : BaseServiceFeature
    {
        private readonly IDBHelper _dbHelper;
        private readonly IJwtProvider _jwtProvider;

        public VerifyUserCredentialsFeature(IDBHelper dbHelper, IJwtProvider jwtProvider) : base()
        {
            _dbHelper = dbHelper;
            _jwtProvider = jwtProvider;
        }

        [Entry]
        public async Task<ApiResponse> VerifyUserCredentials([FromJsonBody] UserAuthRequest input)
        {
            var response = IsInputValid(input);
            if (!string.IsNullOrWhiteSpace(response)) return new ApiResponse { Success = false, ResponseMessage = response };
            object user = await GetUserByCredentials(input.Username, input.Password);
            if (user == null)
                return new ApiResponse { Success = false, ResponseMessage = "Invalid username or password. Please check and try again" };

            var jwt = _jwtProvider.CreateJwtInfo(input.Username, "");

            return new ApiResponse { Success = true, ResponseMessage = "Credentials verification successful", Data = jwt };
        }

        private async Task<object> GetUserByCredentials(string username, string password)
        {
            var parameters = new List<StoreProcedureParameter> {
                new StoreProcedureParameter{ Name = "reqUsername", Type = NpgsqlDbType.Varchar , Value = username},
                new StoreProcedureParameter{ Name = "reqPassword", Type = NpgsqlDbType.Varchar , Value = password},
            };
            var db = await _dbHelper.Fetch<object>("GetUserByUsernameAndPassword", parameters);
            return db.FirstOrDefault();
        }

        private string IsInputValid(UserAuthRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.Username)) return "Username is required";
            if (string.IsNullOrWhiteSpace(input.Password)) return "Password is required";

            return null;
        }
    }
}

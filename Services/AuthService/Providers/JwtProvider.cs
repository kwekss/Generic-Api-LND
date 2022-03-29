using AuthService.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthService.Providers
{
    public class JwtProvider : IJwtProvider
    {
        private readonly AuthenticationConfiguration _config;

        public JwtProvider(AuthenticationConfiguration config)
        {
            _config = config;
        }

        public JwtInfo CreateJwtInfo(string username, string roles)
        {
            var expiresAt = DateTime.Now.AddMinutes(_config.SessionTimeout);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(_config.JwtEncryptionKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                /*Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Expiration, expiresAt.ToString()),
                    new Claim(ClaimTypes.Role, roles),
                }),*/
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            jwtToken = helpers.Utility.Encrypt(jwtToken, _config.JwtEncryptionKey);
            return new JwtInfo { Token = jwtToken, ExpiresAt = expiresAt };
        }
    }
}

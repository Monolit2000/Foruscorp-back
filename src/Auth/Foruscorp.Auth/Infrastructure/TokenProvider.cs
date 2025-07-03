using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Foruscorp.Auth.Infrastructure
{
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Create(User user)
        {
            var secretKey = _configuration["Jwt:Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            var claims = new List<Claim>
                {
                    new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);


            //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            //var claims = new[]
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub,      user.Id.ToString()),
            //    new Claim(JwtRegisteredClaimNames.Email,    user.Email),
            //};

            ////var hasedPass = new PasswordHasher<User>()
            ////    .HashPassword(user, "asdf");

            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(claims),
            //    Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            //    SigningCredentials = credentials,
            //    Issuer = _configuration["Jwt:Issuer"],
            //    Audience = _configuration["Jwt:Audience"]
            //};

            //var tokenHandler = new JsonWebTokenHandler();
            //var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            //return securityToken;
        }
    }
}

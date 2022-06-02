using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedExpert.Domain.Identity;
using MedExpert.Web.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedExpert.Web.Services
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly SymmetricSecurityKey _key;

        public JwtGenerator(AuthorizationConfig config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.TokenKey));
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim> {new(JwtRegisteredClaimNames.NameId, user.UserName)};

            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = credentials
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
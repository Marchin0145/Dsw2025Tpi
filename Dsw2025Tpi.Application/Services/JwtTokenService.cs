using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)// inyectar interfaz de configuracion
        {
            _config = config;
        }

        public string GenerateToken(string username, string role)
        {
            var jwtConfig = _config.GetSection("Jwt");
            var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);// algoritmo de seguridad de encriptacion

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username), // el nombre de usuario del usuario autenticado
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),// identificador unico del token
            new Claim("role", role)//aqui se puede agregar el rol del usuario
        };

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtConfig["ExpireInMinutes"] ?? "60")),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

      


    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationCore.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationCore.Features.Auth;

public class GenerateToken
{
    public class Request
    {
        public required string UserName { get; set; } = default!;
        public required string Password { get; set; } = default!;
    }

    public class Command : IRequest<Response>
    {
        public required Request Request { get; set; }
    }

    public class Response
    {
        public string AccessToken { get; set; } = default!;
    }

    public class Handler(IConfiguration configuration) : IRequestHandler<Command, Response>
    {
        public Task<Response> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = new
            {
                id = Guid.NewGuid(),
                name = configuration["UserAuth:User"]!,
                pass = configuration["UserAuth:Pass"]!
            };

            if (command.Request.UserName != user.name || command.Request.Password != user.pass)
            {
                throw new ForbiddenAccessException();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Sid, user.id.ToString()),
                new(ClaimTypes.Name, user.name)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var expiration = DateTime.Now.AddMinutes(720);

            var securityToken = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            var jwt = new JwtSecurityTokenHandler().WriteToken(securityToken);

            var result = new Response
            {
                AccessToken = jwt
            };

            return Task.FromResult(result);
        }
    }
}

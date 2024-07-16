using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationCore.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationCore.Features.Auth;
public class TokenCommand : IRequest<TokenCommandResponse>
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class TokenCommandHandler : IRequestHandler<TokenCommand, TokenCommandResponse>
{
    private readonly IConfiguration _configuration;

    public TokenCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<TokenCommandResponse> Handle(TokenCommand request, CancellationToken cancellationToken)
    {
        var user = new
        {
            id = Guid.NewGuid(),
            name = _configuration["UserAuth:User"]!,
            pass = _configuration["UserAuth:Pass"]!
        };

        if (request.UserName != user.name && request.Password != user.pass)
        {
            throw new ForbiddenAccessException();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Sid, user.id.ToString()),
            new(ClaimTypes.Name, user.name)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var expiration = DateTime.Now.AddMinutes(720);

        var secutiryToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(secutiryToken);

        var result = new TokenCommandResponse
        {
            AccessToken = jwt
        };

        return Task.FromResult(result);
    }
}

public class TokenCommandResponse
{
    public string AccessToken { get; set; } = default!;
}
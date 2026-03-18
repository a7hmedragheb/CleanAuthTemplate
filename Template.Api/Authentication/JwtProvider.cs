using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Template.Api.Authentication;

public class JwtProvider : IJwtProvider
{
	public (string token, int expiresIn) GenerateToken(ApplicationUser user)
	{
		Claim[] claims = [
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new(JwtRegisteredClaimNames.Email, user.Email!),
			new(JwtRegisteredClaimNames.GivenName, user.FirstName),
			new(JwtRegisteredClaimNames.FamilyName, user.LastName),
			new(JwtRegisteredClaimNames.Gender, user.Gender.ToString()),
			new(JwtRegisteredClaimNames.Birthdate, user.DateOfBirth.ToString("yyyy-MM-dd")),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		];

		var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("OJGU6faPX0DuRf2j6OfdbiMwVAGNXZnK"));

		var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

		var expiresIn = 30;

		var expirationDate = DateTime.UtcNow.AddMinutes(expiresIn);

		var token = new JwtSecurityToken(
			issuer: "Template",
			audience: "Template users",
			claims: claims,
			expires: expirationDate,
			signingCredentials: signingCredentials
		);

		return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresIn: expiresIn * 60);
	}
}
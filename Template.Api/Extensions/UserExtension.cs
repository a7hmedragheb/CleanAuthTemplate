using System.Security.Claims;

namespace Template.Api.Extensions;
public static class Userensions
{
	public static string? GetUserId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
}
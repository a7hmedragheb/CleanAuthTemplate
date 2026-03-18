using Microsoft.AspNetCore.Identity;

namespace Template.Api.Entities;

public sealed class ApplicationUser : IdentityUser
{
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public DateTime DateOfBirth { get; set; }
	public Gender Gender { get; set; }
}

public enum Gender
{
	Male,
	Female
}


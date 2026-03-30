using Microsoft.AspNetCore.Identity;

namespace Template.Api.Entities;

public sealed class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{
		Id = Guid.CreateVersion7().ToString();
		SecurityStamp = Guid.CreateVersion7().ToString();
	}

	public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedOn { get; set; }
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public DateTime DateOfBirth { get; set; }
	public Gender? Gender { get; set; }
	public string? PendingEmail { get; set; }
	public bool IsDeleted { get; set; }
	public DateTime? DeletedAt { get; set; }
	public string? ImageUrl { get; set; }
	public string? ImageThumbnailUrl { get; set; }
	public string? ImagePublicId { get; set; }

	public List<RefreshToken> RefreshTokens { get; set; } = [];
}

public enum Gender
{
	Male,
	Female
}


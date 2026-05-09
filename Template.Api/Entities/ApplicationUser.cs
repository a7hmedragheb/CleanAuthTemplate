namespace Template.Api.Entities;

public sealed class ApplicationUser : IdentityUser
{
	public ApplicationUser()
	{
		Id = Guid.CreateVersion7().ToString();
		SecurityStamp = Guid.CreateVersion7().ToString();
	}

	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public DateTime DateOfBirth { get; set; }
	public Gender? Gender { get; set; }
	public bool IsDisabled { get; set; }
	public string? ImageUrl { get; set; }
	public string? ImageThumbnailUrl { get; set; }
	public string? ImagePublicId { get; set; }

	public List<RefreshToken> RefreshTokens { get; set; } = [];
}
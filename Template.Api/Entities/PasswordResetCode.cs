namespace Template.Api.Entities;

public class PasswordResetCode
{
	public Guid Id { get; set; } = Guid.CreateVersion7();
	public string UserId { get; set; } = default!;
	public ApplicationUser User { get; set; } = default!;
	public string CodeHash { get; set; } = default!;
	public string IdentityToken { get; set; } = default!;
	public DateTime ExpiresAt { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UsedAt { get; set; }
	public bool IsUsed => UsedAt.HasValue;
	public int Attempts { get; set; } = 0;
}
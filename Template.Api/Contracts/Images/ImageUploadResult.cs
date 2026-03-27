namespace Template.Api.Contracts.Images;

public record UploadImageResult(
	string ImageUrl,
	string PublicId,
	string? ThumbnailUrl = null
);

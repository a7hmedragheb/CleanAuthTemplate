namespace Template.Api.Contracts.Images;

public record UploadImageRequest(
	IFormFile Image
);
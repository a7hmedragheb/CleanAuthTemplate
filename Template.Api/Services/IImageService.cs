using Template.Api.Contracts.Images;

namespace Template.Api.Services;

public interface IImageService
{
	// Cloudinary Storage 
	Task<UploadImageResult> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default);
	Task DeleteAsync(string ImagePublicId);

	// Local Storage
	//Task<string> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default);
	// Task DeleteAsync(string imagePath, string imageThumbnailPath, CancellationToken cancellationToken = default);
}

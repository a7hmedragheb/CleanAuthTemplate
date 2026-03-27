namespace Template.Api.Services;

public interface IImageService
{
	Task<string> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default);
	 Task DeleteAsync(string imagePath, string imageThumbnailPath, CancellationToken cancellationToken = default);

}

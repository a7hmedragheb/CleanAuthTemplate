namespace Template.Api.Services;

public interface IImageService
{
	Task<string> UploadAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}

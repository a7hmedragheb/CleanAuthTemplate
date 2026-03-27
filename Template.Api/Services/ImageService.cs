using Microsoft.AspNetCore.Hosting;

namespace Template.Api.Services;

public class ImageService : IImageService
{
	private readonly IWebHostEnvironment _webHostEnvironment;
	public ImageService(IWebHostEnvironment webHostEnvironment)
	{
		_webHostEnvironment = webHostEnvironment;
	}

	public async Task<string> UploadAsync(IFormFile image, string folder, CancellationToken cancellationToken = default)
	{
		var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", folder);
		Directory.CreateDirectory(folderPath);

		var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
		var fileName = $"{Guid.CreateVersion7()}{extension}";
		var filePath = Path.Combine(folderPath, fileName);

		await using var stream = new FileStream(filePath, FileMode.Create);
		await image.CopyToAsync(stream, cancellationToken);
		stream.Dispose();

		return $"/images/{folder}/{fileName}";
	}

	public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
	{
		var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

		if (File.Exists(filePath))
			File.Delete(filePath);

		return Task.CompletedTask;
	}
}

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Template.Api.Services;

public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
{
	private const int ThumbnailWidth = 200;

	public async Task<string> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default)
	{
		var folderPath = Path.Combine(webHostEnvironment.WebRootPath, "images", folder);
		Directory.CreateDirectory(folderPath);

		var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
		var fileName = $"{Guid.CreateVersion7()}{extension}";
		var filePath = Path.Combine(folderPath, fileName);

		// Save Original
		await using var stream = new FileStream(filePath, FileMode.Create);
		await file.CopyToAsync(stream, cancellationToken);
		stream.Dispose();

		// Save Thumbnail
		if (hasThumbnail)
		{
			var thumbFolderPath = Path.Combine(folderPath, "thumb");
			Directory.CreateDirectory(thumbFolderPath);

			var thumbFilePath = Path.Combine(thumbFolderPath, fileName);

			using var image = Image.Load(file.OpenReadStream());
			var ratio = (float)image.Width / ThumbnailWidth;
			var height = (int)(image.Height / ratio);

			image.Mutate(x => x.Resize(ThumbnailWidth, height));
			await image.SaveAsync(thumbFilePath, cancellationToken);
		}

		return $"/images/{folder}/{fileName}";
	}

	public Task DeleteAsync(string imagePath, string imageThumbnailPath, CancellationToken cancellationToken = default)
	{
		var filePath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

		if (File.Exists(filePath))
			File.Delete(filePath);


		if (!string.IsNullOrEmpty(imageThumbnailPath))
		{
			var ThumbPath = Path.Combine(webHostEnvironment.WebRootPath, imageThumbnailPath.TrimStart('/'));

			if (File.Exists(ThumbPath))
				File.Delete(ThumbPath);
		}

		return Task.CompletedTask;
	}

}
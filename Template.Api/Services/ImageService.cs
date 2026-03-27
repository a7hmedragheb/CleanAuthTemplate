using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Template.Api.Contracts.Images;

namespace Template.Api.Services;

public class ImageService(IOptions<CloudinarySettings> cloudinarySettings) : IImageService
{
	private readonly Cloudinary _cloudinary = new(new Account(
		cloudinarySettings.Value.CloudName,
		cloudinarySettings.Value.ApiKey,
		cloudinarySettings.Value.ApiSecret
	));
	public async Task<UploadImageResult> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default)
	{

		await using var stream = file.OpenReadStream();

		var uploadParams = new ImageUploadParams
		{
			File = new FileDescription(file.FileName, stream),
			Folder = folder,
			UseFilename = true,
		};


		var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

		var imageUrl = uploadResult.SecureUrl.ToString();

		var thumbnailUrl = hasThumbnail ? GetThumbnailUrl(imageUrl) : null;

		return new UploadImageResult(
			imageUrl,
			uploadResult.PublicId,
			thumbnailUrl
		);
	}

	public async Task DeleteAsync(string ImagePublicId)
	{
		var deleteParams = new DeletionParams(ImagePublicId);
		await _cloudinary.DestroyAsync(deleteParams);
	}

	private static string GetThumbnailUrl(string url)
	{
		var separator = "image/upload/";
		var urlParts = url.Split(separator);
		return $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";
	}

	// Local Storage
	//private const int ThumbnailWidth = 200;

	//public async Task<string> UploadAsync(IFormFile file, string folder, bool hasThumbnail = false, CancellationToken cancellationToken = default)
	//{
	//	var folderPath = Path.Combine(webHostEnvironment.WebRootPath, "images", folder);
	//	Directory.CreateDirectory(folderPath);

	//	var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
	//	var fileName = $"{Guid.CreateVersion7()}{extension}";
	//	var filePath = Path.Combine(folderPath, fileName);

	//	// Save Original
	//	await using var stream = new FileStream(filePath, FileMode.Create);
	//	await file.CopyToAsync(stream, cancellationToken);
	//	stream.Dispose();

	//	// Save Thumbnail
	//	if (hasThumbnail)
	//	{
	//		var thumbFolderPath = Path.Combine(folderPath, "thumb");
	//		Directory.CreateDirectory(thumbFolderPath);

	//		var thumbFilePath = Path.Combine(thumbFolderPath, fileName);

	//		using var image = Image.Load(file.OpenReadStream());
	//		var ratio = (float)image.Width / ThumbnailWidth;
	//		var height = (int)(image.Height / ratio);

	//		image.Mutate(x => x.Resize(ThumbnailWidth, height));
	//		await image.SaveAsync(thumbFilePath, cancellationToken);
	//	}

	//	return $"/images/{folder}/{fileName}";
	//}

	//public Task DeleteAsync(string imagePath, string imageThumbnailPath, CancellationToken cancellationToken = default)
	//{
	//	var filePath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

	//	if (File.Exists(filePath))
	//		File.Delete(filePath);


	//	if (!string.IsNullOrEmpty(imageThumbnailPath))
	//	{
	//		var ThumbPath = Path.Combine(webHostEnvironment.WebRootPath, imageThumbnailPath.TrimStart('/'));

	//		if (File.Exists(ThumbPath))
	//			File.Delete(ThumbPath);
	//	}

	//	return Task.CompletedTask;
	//}

}
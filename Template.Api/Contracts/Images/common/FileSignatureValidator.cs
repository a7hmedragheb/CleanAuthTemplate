using FluentValidation;

namespace Template.Api.Contracts.Images.common;
public class FileSignatureValidator : AbstractValidator<IFormFile>
{
	public FileSignatureValidator()
	{
		RuleFor(x => x)
			.Must(file =>
			{
				using var binary = new BinaryReader(file.OpenReadStream());
				var bytes = binary.ReadBytes(2);

				var fileSequenceHex = BitConverter.ToString(bytes);

				return FileSettings.AllowedSignatures
					.Any(signature => signature.Equals(fileSequenceHex, StringComparison.OrdinalIgnoreCase));
			})
			.WithMessage("Invalid file. Only JPEG, PNG, and WebP images are accepted.")
			.When(x => x is not null);
	}
}
using FluentValidation;
using Template.Api.Contracts.Images.common;

namespace Template.Api.Contracts.Images;
public class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
{
	public UploadImageRequestValidator()
	{
		RuleFor(x => x.Image)
			 .NotNull()
			 .WithMessage("Avatar is required.")
			 .SetValidator(new FileSizeValidator())
			 .SetValidator(new FileSignatureValidator());
	}
}

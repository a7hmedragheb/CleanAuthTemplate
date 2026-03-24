using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Api.Contracts.Users;
using Template.Api.Extensions;

[Route("/[Controller]")]
[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
	private readonly IUserService _userService;
	public AccountController(IUserService userService)
	{
		_userService = userService;
	}

	[HttpGet("profile")]
	public async Task<IActionResult> Info()
	{
		var result = await _userService.GetProfileAsync(User.GetUserId()!);

		return Ok(result.Value);
	}

	[HttpPut("update-profile")]
	public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
	{
		var result = await _userService.UpdateProfileAsync(User.GetUserId()!, request);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	[HttpPut("change-password")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);

		return result.IsSuccess ? NoContent() : result.ToProblem();
	}

	[HttpPost("change-email")]
	public async Task<IActionResult> SendChangeEmailCode([FromBody] ChangeEmailRequest request)
	{
		var result = await _userService.SendChangeEmailCodeAsync(User.GetUserId()!, request.NewEmail);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpPut("confirm-email-change")]
	public async Task<IActionResult> ConfirmEmailChange([FromQuery] ConfirmEmailChangeRequest request)
	{
		var result = await _userService.ConfirmEmailChangeAsync(User.GetUserId()!, request);
		return result.IsSuccess ? Ok() : result.ToProblem();
	}

	[HttpDelete("delete-account")]
	public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request, CancellationToken cancellationToken)
	{
		var result = await _userService.DeleteAccountAsync(User.GetUserId()!, request.Password);

		return result.IsSuccess ? Ok() : result.ToProblem();
	}
}

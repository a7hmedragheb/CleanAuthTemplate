using System.ComponentModel.DataAnnotations;

namespace Template.Api.Settings;
public class GoogleSettings
{
	[Required]
	public string ClientId { get; set; } = string.Empty;

	[Required]
	public string ClientSecret { get; set; } = string.Empty;
}

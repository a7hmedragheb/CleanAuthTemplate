using System.ComponentModel.DataAnnotations;

namespace Template.Api.Settings;
public class AppSettings
{
	[Required]
	public string FrontendUrl { get; set; } = string.Empty;
}

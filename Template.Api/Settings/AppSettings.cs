namespace Template.Api.Settings;
public class AppSettings
{
	[Required]
	public string FrontendBaseUrl { get; set; } = string.Empty;
}

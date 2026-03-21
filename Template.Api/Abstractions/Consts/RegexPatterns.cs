namespace Template.Api.Abstractions.Consts;

public class RegexPatterns
{
	public const string Password = "(?=(.*[0-9]))(?=.*[\\!@#$%^&*()\\\\[\\]{}\\-_+=~`|:;\"'<>,./?])(?=.*[a-z])(?=(.*[A-Z]))(?=(.*)).{8,}";
	public const string PhoneNumber = "^01[0,1,2,5]{1}[0-9]{8}$";
	public const string CharactersOnly_Eng = "^[a-zA-Z-_ ]*$";
	public const string NumbersOnly = @"^\d+$";

}

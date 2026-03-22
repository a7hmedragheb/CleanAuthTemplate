using System.Security.Cryptography;
using System.Text;

namespace Template.Api.Helpers;

public static class SecurityHelper
{
	public static readonly char[] _allowedNumber = AllowedNumber._allowedNumber;
	public static string GenerateVerificationCode(int length = 6)
	{
		var codeDigits = new char[length];

		do
		{
			var randomBytes = RandomNumberGenerator.GetBytes(length);

			for (int i = 0; i < length; i++)
			{
				if (randomBytes[i] < 256 - (256 % _allowedNumber.Length))
					codeDigits[i] = _allowedNumber[randomBytes[i] % _allowedNumber.Length];
			}

		} while (codeDigits.Contains('\0'));

		return new string(codeDigits);
	}

	public static string ComputeSha256Hash(string input)
	{
		var bytes = Encoding.UTF8.GetBytes(input);
		var hashed = SHA256.HashData(bytes);
		return Convert.ToBase64String(hashed);
	}
}
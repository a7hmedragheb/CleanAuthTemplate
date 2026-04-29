namespace SurveyBasket.Abstractions.Consts;

public static class DefaultRoles
{
	public partial class Admin
	{
		public const string Name = nameof(Admin);
		public const string Id = "01985b0d-533f-7800-9642-fc35edd0777c";
		public const string ConcurrencyStamp = "01985b0d-533f-7800-9642-fc37a757399c";
	}
	public partial class Member
	{
		public const string Name = nameof(Member);
		public const string Id = "01985b0d-533f-7800-9642-fc3691439f69";
		public const string ConcurrencyStamp = "01985b0d-533f-7800-9642-fc38cad80e6e";
	}

}


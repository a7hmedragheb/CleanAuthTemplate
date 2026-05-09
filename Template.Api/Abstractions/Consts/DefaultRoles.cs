namespace Template.Api.Abstractions.Consts;

public static class DefaultRoles
{
	public partial class Admin
	{
		public const string Name = nameof(Admin);
		public const string Id = "019dd94b-9c35-78a3-b7e1-90544b8be79e";
		public const string ConcurrencyStamp = "019dd94b-9c35-78a3-b7e1-90552fb9ab80";
	}
	public partial class Member
	{
		public const string Name = nameof(Member);
		public const string Id = "019dd94b-9c35-78a3-b7e1-90560e296a0b";
		public const string ConcurrencyStamp = "019dd94b-9c35-78a3-b7e1-90575fd7c247";
	}
}
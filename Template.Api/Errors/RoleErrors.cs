namespace Template.Api.Errors;

public record RoleErrors
{
	public static readonly Error RoleNotFound =
		  new("Role.RoleNotFound", "Role is not found", StatusCodes.Status404NotFound);
}
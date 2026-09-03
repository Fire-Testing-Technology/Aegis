namespace Aegis.Server.AspNetCore;

/// <summary>Stable activity-log action keys.</summary>
public static class ActivityActions
{
    public const string Login = "Auth.Login";
    public const string Logout = "Auth.Logout";

    public const string UserCreated = "User.Created";
    public const string UserEdited = "User.Edited";
    public const string UserDeleted = "User.Deleted";

    public const string LicenceCreated = "Licence.Created";
    public const string LicenceRevoked = "Licence.Revoked";

    public const string ComponentAdded = "Component.Added";
    public const string ComponentEdited = "Component.Edited";
}

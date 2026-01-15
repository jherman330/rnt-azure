namespace SimpleTodo.Api.Services;

public class UserContextService : IUserContextService
{
    public string GetCurrentUserId()
    {
        // TODO: Integrate with authentication system to extract user ID from HttpContext claims
        // For now, return a placeholder default user ID
        return "default-user";
    }
}

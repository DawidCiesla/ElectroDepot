namespace WebClient.Services
{
    public record UserSession(
        int ID,
        string Username,
        string Email,
        string Name
    );
}

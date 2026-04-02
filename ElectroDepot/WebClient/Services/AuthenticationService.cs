namespace WebClient.Services
{
    public class AuthenticationService
    {
        private UserSession? _currentUser;

        public event Action? OnAuthenticationStateChanged;

        public UserSession? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnAuthenticationStateChanged?.Invoke();
            }
        }

        public bool IsAuthenticated => CurrentUser != null;

        public void Login(UserSession session)
        {
            CurrentUser = session;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}

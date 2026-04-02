using ElectroDepotClassLibrary.DTOs;

namespace WebClient.Services
{
    public class AuthenticationService
    {
        private UserDTO? _currentUser;

        public event Action? OnAuthenticationStateChanged;

        public UserDTO? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnAuthenticationStateChanged?.Invoke();
            }
        }

        public bool IsAuthenticated => CurrentUser != null;

        public void Login(UserDTO user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}

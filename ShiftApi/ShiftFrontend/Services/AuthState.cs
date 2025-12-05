namespace ShiftFrontend.Services
{
    public class AuthState
    {
        public bool IsLoggedIn { get; private set; }
        public string? Role { get; private set; }

        public event Action? OnChange;

        public void SetLogin(string? role)
        {
            IsLoggedIn = true;
            Role = role;
            NotifyStateChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            Role = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}

namespace ShiftFrontend.Services
{
    public class AuthState
    {
        public bool IsLoggedIn { get; private set; }
        public string? Role { get; private set; }

        public string? Email { get; private set; }

        public event Action? OnChange;

        public void SetLogin(string? role, string? email)
        {
            IsLoggedIn = true;
            Role = role;
            Email = email;
            NotifyStateChanged();
        }

        public void Logout()
        {
            IsLoggedIn = false;
            Role = null;
            Email = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}

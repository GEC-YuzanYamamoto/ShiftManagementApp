using ShiftFrontend.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShiftFrontend.Services;

public class ShiftApiClient
{
    private readonly HttpClient _http;
    private readonly ProtectedLocalStorage _storage;

    private string? _jwtToken;
    private string? _role;

    private const string TokenKey = "shift.jwt";

    // 状態変化通知（ログイン/ログアウト/ロール変化など）
    public event Action? OnChange;

    public bool IsLoggedIn => !string.IsNullOrEmpty(_jwtToken);
    public string? CurrentRole => _role;

    public ShiftApiClient(IHttpClientFactory httpClientFactory, ProtectedLocalStorage storage)
    {
        _http = httpClientFactory.CreateClient("ShiftApi");
        _storage = storage;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    // ==== DTO ====
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    // =========================
    // Token 永続化 + Authorization 付与
    // =========================
    public async Task TryRestoreTokenAsync()
    {
        var result = await _storage.GetAsync<string>(TokenKey);
        if (result.Success && !string.IsNullOrEmpty(result.Value))
        {
            await SetTokenAsync(result.Value);
        }
    }

    public async Task SetTokenAsync(string? token)
    {
        _jwtToken = token;

        if (string.IsNullOrEmpty(token))
        {
            _role = null;
            _http.DefaultRequestHeaders.Authorization = null;
            await _storage.DeleteAsync(TokenKey);
            NotifyStateChanged();
            return;
        }

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // role抽出（UI分岐用：最終判断はAPI側Authorize）
        _role = TryExtractRole(token);

        await _storage.SetAsync(TokenKey, token);
        NotifyStateChanged();
    }

    public async Task ClearTokenAsync()
    {
        _jwtToken = null;
        _role = null;
        _http.DefaultRequestHeaders.Authorization = null;
        await _storage.DeleteAsync(TokenKey);
        NotifyStateChanged();
    }

    private static string? TryExtractRole(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            return jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role ||
                c.Type == "role" ||
                c.Type == "roles")?.Value;
        }
        catch
        {
            return null;
        }
    }

    // =========================
    // 共通：安全な GET(List) / GET(T)
    // =========================
    private async Task<List<T>> GetListSafeAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);

        if (res.StatusCode is HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden
            or HttpStatusCode.NotFound)
        {
            return new List<T>();
        }

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<T>>() ?? new List<T>();
    }

    private async Task<T?> GetSafeAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);

        if (res.StatusCode is HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden
            or HttpStatusCode.NotFound)
        {
            return default;
        }

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<T>();
    }

    // =========================
    // 共通：安全な POST/PUT/DELETE（bool返却）
    // =========================
    private async Task<bool> PostSafeAsync<T>(string url, T body)
    {
        var res = await _http.PostAsJsonAsync(url, body);

        if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return false;

        return res.IsSuccessStatusCode;
    }

    private async Task<bool> PutSafeAsync<T>(string url, T body)
    {
        var res = await _http.PutAsJsonAsync(url, body);

        if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return false;

        return res.IsSuccessStatusCode;
    }

    private async Task<bool> DeleteSafeAsync(string url)
    {
        var res = await _http.DeleteAsync(url);

        if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            return false;

        return res.IsSuccessStatusCode;
    }

    // =========================
    // ログイン
    // =========================
    public async Task<bool> LoginAsync(string email, string password)
    {
        var req = new LoginRequest { Email = email, Password = password };

        var res = await _http.PostAsJsonAsync("auth/login", req);
        if (!res.IsSuccessStatusCode) return false;

        var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
        if (body == null || string.IsNullOrEmpty(body.Token)) return false;

        await SetTokenAsync(body.Token);
        return true;
    }

    // =========================
    // テスト
    // =========================
    public async Task<string> TestAsync()
    {
        var res = await _http.GetAsync("shift-requests");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    // =========================
    // 希望シフト（一般ユーザー用）
    // =========================
    public Task<List<ShiftRequestDto>> GetMyShiftRequestsForMonthAsync(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var url = $"shift-requests?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return GetListSafeAsync<ShiftRequestDto>(url);
    }

    public async Task<bool> CreateShiftRequestAsync(DateOnly date, int shiftType)
    {
        var dto = new CreateShiftRequestDto
        {
            ShiftDate = date,
            ShiftType = (byte)shiftType
        };

        return await PostSafeAsync("shift-requests", dto);
    }

    // =========================
    // 管理：確定シフト
    // =========================
    public async Task<bool> CreateShiftAsync(ShiftManagementDto request)
    {
        // Task.FromResult(_http) は削除（不要）
        return await PostSafeAsync("shift-schedules", request);
    }

    public Task<List<ShiftRequestDto>> GetShiftRequestsAsync(DateOnly from, DateOnly to)
    {
        var url = $"shift-requests?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return GetListSafeAsync<ShiftRequestDto>(url);
    }

    public Task<List<ShiftScheduleDto>> GetShiftSchedulesAsync(DateOnly from, DateOnly to)
    {
        var url = $"shift-schedules?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        return GetListSafeAsync<ShiftScheduleDto>(url);
    }

    public async Task<bool> UpdateShiftScheduleAsync(int scheduleId, byte shiftType)
    {
        var dto = new UpdateShiftScheduleDto { ShiftType = shiftType };

        var res = await _http.PutAsJsonAsync($"shift-schedules/{scheduleId}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task MarkRequestAsApprovedAsync(int requestId)
    {
        var response = await _http.PostAsync($"shift-requests/{requestId}/approve", content: null);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new ApplicationException("権限がありません（未ログイン or ロール不足）");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApplicationException(
                $"シフト希望の承認に失敗しました (Status: {response.StatusCode}) Body: {body}");
        }
    }

    // =========================
    // ユーザー管理
    // =========================
    public Task<List<UserDto>> GetUsersAsync()
        => GetListSafeAsync<UserDto>("users");

    public Task<bool> CreateUserAsync(CreateUserDto dto)
        => PostSafeAsync("users", dto);

    public Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
        => PutSafeAsync($"users/{id}", dto);

    public Task<bool> DeleteUserAsync(int id)
        => DeleteSafeAsync($"users/{id}");
}

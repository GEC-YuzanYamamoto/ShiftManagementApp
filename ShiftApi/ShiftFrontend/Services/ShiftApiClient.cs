using ShiftFrontend.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;


namespace ShiftFrontend.Services;

public class ShiftApiClient
{
    private readonly HttpClient _http;
    private string? _jwtToken;
    private string? _role;

    public bool IsLoggedIn => !string.IsNullOrEmpty(_jwtToken);
    public string? CurrentRole => _role;
    public ShiftApiClient(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("ShiftApi");
    }

    // ==== DTO ====
    // LoginDto に合わせて Email + Password を送る
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // AuthController は { token } だけ返しているのでそれを受ける
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    // ==== トークンのセット ====
    public void SetToken(string? token)
    {
        _jwtToken = token;

        if (string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        // トークンから役割を抽出して保存
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // ClaimTypes.Role または "role" など、API 側の設定に合わせる
            _role = jwt.Claims
                .FirstOrDefault(c =>
                    c.Type == ClaimTypes.Role ||
                    c.Type == "role" ||
                    c.Type == "roles")?.Value;
        }
        catch
        {
            // パースに失敗したら何もしない（_role は null のまま）
        }
    }
    // ==== トークンのクリア ====
    public void ClearToken()
    {
        _jwtToken = null;
        _role = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }


    // ==== ログイン (/auth/login) ====
    public async Task<bool> LoginAsync(string email, string password)
    {
        var req = new LoginRequest
        {
            Email = email,
            Password = password
        };

        var res = await _http.PostAsJsonAsync("auth/login", req);
        if (!res.IsSuccessStatusCode)
        {
            return false;
        }

        var body = await res.Content.ReadFromJsonAsync<LoginResponse>();
        if (body == null || string.IsNullOrEmpty(body.Token))
        {
            return false;
        }

        SetToken(body.Token);
        return true;
    }

    // ==== 認証が必要なAPI呼び出しのテスト ====
    public async Task<string> TestAsync()
    {
        var res = await _http.GetAsync("shift-requests");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    public async Task<List<ShiftRequestDto>> GetMyShiftRequestsForMonthAsync(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var url = $"shift-requests?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        var res = await _http.GetAsync(url);

        // 401（未認証）のときは空リストを返す
        if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new List<ShiftRequestDto>();
        }

        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return new List<ShiftRequestDto>();
        }

        res.EnsureSuccessStatusCode();

        var data = await res.Content.ReadFromJsonAsync<List<ShiftRequestDto>>();
        return data ?? new List<ShiftRequestDto>();
    }

    // シフト希望登録
    public async Task<bool> CreateShiftRequestAsync(DateOnly date, int shiftType)
    {
        var dto = new CreateShiftRequestDto
        {
            ShiftDate = date,
            ShiftType = (byte)shiftType
        };

        var res = await _http.PostAsJsonAsync("shift-requests", dto);
        return res.IsSuccessStatusCode;
    }

    // ==== ユーザー管理API ====
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var res = await _http.GetAsync("users");
        if (res.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new List<UserDto>();
        }

        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<List<UserDto>>();
        return data ?? new List<UserDto>();
    }

    public async Task<bool> CreateUserAsync(CreateUserDto dto)
    {
        var res = await _http.PostAsJsonAsync("users", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var res = await _http.PutAsJsonAsync($"users/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var res = await _http.DeleteAsync($"users/{id}");
        return res.IsSuccessStatusCode;
    }
}
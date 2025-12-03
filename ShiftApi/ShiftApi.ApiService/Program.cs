using Microsoft.EntityFrameworkCore;
using ShiftApi.ApiService.Data;

var builder = WebApplication.CreateBuilder(args);

// Aspire のサービス共通設定
builder.AddServiceDefaults();

// DbContext(PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC / API
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 問題詳細レスポンス
builder.Services.AddProblemDetails();

var app = builder.Build();

// 開発環境のときだけ Swagger UI を有効化
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 例外ハンドラ
app.UseExceptionHandler();

// HTTPS リダイレクト
app.UseHttpsRedirection();

// コントローラのルート (/users など)
app.MapControllers();

// おまけの WeatherForecast API（残しておきたいなら）
string[] summaries =
[
    "Freezing", "Bracing", "Chilly", "Cool", "Mild",
    "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

// Aspire のデフォルトエンドポイント
app.MapDefaultEndpoints();

// 最後に一回だけ Run
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShiftApi.ApiService.Models;

namespace ShiftApi.ApiService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ShiftRequest> ShiftRequests => Set<ShiftRequest>();
    public DbSet<ShiftSchedule> ShiftSchedules => Set<ShiftSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DateOnly <-> DateTime 変換（DBは date 型にしたい）
        // PostgreSQL(Npgsql)なら DateOnly をそのまま扱える場合もありますが、
        // 環境差で詰まりやすいので明示変換しておくと安定します。
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d)
        );

        // -------------------------
        // User
        // -------------------------
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Email).IsRequired();

            // Emailは一意が一般的（ログインIDとして使う想定）
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Role).IsRequired();

            e.Property(x => x.CreatedAt)
             .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });

        // -------------------------
        // ShiftRequest
        // -------------------------
        modelBuilder.Entity<ShiftRequest>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.ShiftDate)
             .HasConversion(dateOnlyConverter)
             .HasColumnType("date");

            e.Property(x => x.ShiftType).IsRequired();
            e.Property(x => x.Status).IsRequired();

            e.Property(x => x.CreatedAt)
             .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            e.Property(x => x.UpdatedAt)
             .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // 関連：User 1 - * ShiftRequest
            e.HasOne(x => x.User)
             .WithMany(u => u.ShiftRequests)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // よく使う検索用（例：月表示で UserId + ShiftDate）
            e.HasIndex(x => new { x.UserId, x.ShiftDate });
        });

        // -------------------------
        // ShiftSchedule
        // -------------------------
        modelBuilder.Entity<ShiftSchedule>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.ShiftDate)
             .HasConversion(dateOnlyConverter)
             .HasColumnType("date");

            e.Property(x => x.ShiftType).IsRequired();

            e.Property(x => x.ConfirmedAt)
             .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            // 関連：User 1 - * ShiftSchedule（対象ユーザー）
            e.HasOne(x => x.User)
             .WithMany(u => u.ShiftSchedules)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // 関連：User 1 - * ShiftSchedule（確定者ユーザー）
            // ここは事故りやすいので明示するのが重要
            e.HasOne(x => x.ConfirmedByUser)
             .WithMany(u => u.ConfirmedSchedules)
             .HasForeignKey(x => x.ConfirmedBy)
             .OnDelete(DeleteBehavior.Restrict); // 管理者削除で連鎖削除しない

            e.HasIndex(x => new { x.UserId, x.ShiftDate });
        });
    }
}

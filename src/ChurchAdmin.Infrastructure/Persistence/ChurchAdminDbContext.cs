using ChurchAdmin.Application.Common.Interfaces;
using ChurchAdmin.Domain.Common;
using ChurchAdmin.Domain.Entities;
using ChurchAdmin.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Infrastructure.Persistence;

public sealed class ChurchAdminDbContext : DbContext, IChurchAdminDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ChurchAdminDbContext(
        DbContextOptions<ChurchAdminDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Church> Churches => Set<Church>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<WorkerTeam> WorkerTeams => Set<WorkerTeam>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCommonColumns(modelBuilder);

        modelBuilder.Entity<Church>(entity =>
        {
            entity.HasIndex(x => x.Slug)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(1000);
            entity.Property(x => x.PrimaryColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.SecondaryColor).HasMaxLength(20).IsRequired();
            entity.Property(x => x.WelcomeText).HasMaxLength(500);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => new { x.ChurchId, x.Email })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ExternalProvider).HasMaxLength(50);
            entity.Property(x => x.ExternalProviderUserId).HasMaxLength(256);

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasIndex(x => new { x.ChurchId, x.Name })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasIndex(x => new { x.ChurchId, x.Email })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(500).IsRequired();

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<WorkerTeam>(entity =>
        {
            entity.HasIndex(x => new { x.WorkerId, x.TeamId, x.EndDate });

            entity.HasOne(x => x.Worker)
                .WithMany(x => x.WorkerTeams)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Team)
                .WithMany(x => x.WorkerTeams)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasIndex(x => new { x.ChurchId, x.ServiceDate, x.ServiceType })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<FinanceEntry>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.VerifiedBy).HasMaxLength(256);

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CorrectionOfFinanceEntry)
                .WithMany()
                .HasForeignKey(x => x.CorrectionOfFinanceEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);

            entity.HasOne(x => x.Church)
                .WithMany()
                .HasForeignKey(x => x.ChurchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.EntityName).HasMaxLength(160).IsRequired();
            entity.Property(x => x.BeforeJson).HasColumnType("text");
            entity.Property(x => x.AfterJson).HasColumnType("text");
            entity.Property(x => x.Reason).HasMaxLength(1000);
        });

        SeedData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string currentUser = _currentUserService?.Email ?? "system";

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry
                 in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;

                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = currentUser;
                }
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = currentUser;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ConfigureCommonColumns(ModelBuilder modelBuilder)
    {
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType
                 in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.RowVersion))
                    .IsConcurrencyToken()
                    .HasColumnType("bytea")
                    .HasDefaultValueSql("gen_random_uuid()::text::bytea");

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.CreatedBy))
                    .HasMaxLength(256);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.UpdatedBy))
                    .HasMaxLength(256);
            }

            if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(SoftDeletableEntity.DeletedBy))
                    .HasMaxLength(256);
            }
        }
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        byte[] seedRowVersion = [1];

        Guid laborneChurchId = Guid.Parse("30000000-0000-0000-0000-000000000001");

        Guid worshipTeamId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        Guid mediaTeamId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        Guid childrenTeamId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        Guid usheringTeamId = Guid.Parse("10000000-0000-0000-0000-000000000004");
        Guid financeTeamId = Guid.Parse("10000000-0000-0000-0000-000000000005");

        Guid adminUserId = Guid.Parse("20000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Church>().HasData(
            new Church
            {
                Id = laborneChurchId,
                Name = "La Borne Church Cape Durbanville",
                Slug = "laborne",
                LogoUrl = null,
                PrimaryColor = "#111827",
                SecondaryColor = "#F9FAFB",
                WelcomeText = "Welcome to La Borne Church Admin",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            }
        );

        modelBuilder.Entity<Team>().HasData(
            new Team
            {
                Id = worshipTeamId,
                ChurchId = laborneChurchId,
                Name = "Worship",
                Description = "Music and praise team",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            },
            new Team
            {
                Id = mediaTeamId,
                ChurchId = laborneChurchId,
                Name = "Media",
                Description = "Sound, camera and livestream",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            },
            new Team
            {
                Id = childrenTeamId,
                ChurchId = laborneChurchId,
                Name = "Children",
                Description = "Children ministry",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            },
            new Team
            {
                Id = usheringTeamId,
                ChurchId = laborneChurchId,
                Name = "Ushering",
                Description = "Welcoming and seating",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            },
            new Team
            {
                Id = financeTeamId,
                ChurchId = laborneChurchId,
                Name = "Finance",
                Description = "Finance and treasury",
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminUserId,
                ChurchId = laborneChurchId,
                Email = "admin@church.local",
                DisplayName = "Church Admin",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTimeOffset.UnixEpoch,
                CreatedBy = "seed",
                IsDeleted = false,
                RowVersion = seedRowVersion
            }
        );
    }
}
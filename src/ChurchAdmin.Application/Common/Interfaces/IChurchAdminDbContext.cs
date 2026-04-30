using ChurchAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchAdmin.Application.Common.Interfaces;

public interface IChurchAdminDbContext
{
    DbSet<User> Users { get; }

    DbSet<Team> Teams { get; }

    DbSet<Worker> Workers { get; }

    DbSet<WorkerTeam> WorkerTeams { get; }

    DbSet<AttendanceRecord> AttendanceRecords { get; }

    DbSet<FinanceEntry> FinanceEntries { get; }

    DbSet<InventoryItem> InventoryItems { get; }

    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Church> Churches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

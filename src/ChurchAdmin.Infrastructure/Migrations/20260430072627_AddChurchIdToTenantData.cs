using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchAdmin.Infrastructure.Migrations
{
    public partial class AddChurchIdToTenantData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workers_Email",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Teams_Name",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ServiceDate_ServiceType",
                table: "AttendanceRecords");

            // ADD CHURCH ID COLUMNS
            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Workers",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Teams",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "InventoryItems",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "FinanceEntries",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            // 🔥 FIX: FULL CHURCH SEED (ALL REQUIRED FIELDS)
            migrationBuilder.Sql("""
INSERT INTO "Churches"
(
    "Id",
    "Name",
    "Slug",
    "LogoUrl",
    "PrimaryColor",
    "SecondaryColor",
    "WelcomeText",
    "IsActive",
    "CreatedAt",
    "CreatedBy",
    "RowVersion",
    "IsDeleted"
)
VALUES
(
    '30000000-0000-0000-0000-000000000001',
    'La Borne Church Cape Durbanville',
    'laborne',
    NULL,
    '#111827',
    '#F9FAFB',
    'Welcome to La Borne Church Admin',
    true,
    NOW(),
    'seed',
    decode('01', 'hex'),
    false
)
ON CONFLICT ("Id") DO NOTHING;
""");

            // 🔥 FIX: BACKFILL ALL TABLES
            migrationBuilder.Sql("""
UPDATE "Users" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "Workers" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "Teams" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "AttendanceRecords" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "FinanceEntries" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "InventoryItems" SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            // INDEXES
            migrationBuilder.CreateIndex(
                name: "IX_Workers_ChurchId_Email",
                table: "Workers",
                columns: new[] { "ChurchId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChurchId_Email",
                table: "Users",
                columns: new[] { "ChurchId", "Email" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ChurchId_Name",
                table: "Teams",
                columns: new[] { "ChurchId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ChurchId",
                table: "InventoryItems",
                column: "ChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceEntries_ChurchId",
                table: "FinanceEntries",
                column: "ChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ChurchId_ServiceDate_ServiceType",
                table: "AttendanceRecords",
                columns: new[] { "ChurchId", "ServiceDate", "ServiceType" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            // FOREIGN KEYS
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Churches_ChurchId",
                table: "Users",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Churches_ChurchId",
                table: "Workers",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Churches_ChurchId",
                table: "Teams",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Churches_ChurchId",
                table: "InventoryItems",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinanceEntries_Churches_ChurchId",
                table: "FinanceEntries",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Churches_ChurchId",
                table: "AttendanceRecords",
                column: "ChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
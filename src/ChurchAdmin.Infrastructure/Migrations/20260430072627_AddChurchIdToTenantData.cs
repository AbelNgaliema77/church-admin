using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchAdmin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChurchIdToTenantData : Migration
    {
        /// <inheritdoc />
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

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Workers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "Teams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "InventoryItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "FinanceEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChurchId",
                table: "AttendanceRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("""
INSERT INTO "Churches" ("Id", "Name", "Slug", "CreatedAt", "IsDeleted")
VALUES ('30000000-0000-0000-0000-000000000001', 'La Borne Church', 'la-borne-church', NOW(), false)
ON CONFLICT ("Id") DO NOTHING;
""");

            migrationBuilder.Sql("""
UPDATE "Teams"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "Users"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "Workers"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "AttendanceRecords"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "FinanceEntries"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

            migrationBuilder.Sql("""
UPDATE "InventoryItems"
SET "ChurchId" = '30000000-0000-0000-0000-000000000001'
WHERE "ChurchId" = '00000000-0000-0000-0000-000000000000';
""");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Churches_ChurchId",
                table: "AttendanceRecords",
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
                name: "FK_InventoryItems_Churches_ChurchId",
                table: "InventoryItems",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Churches_ChurchId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FinanceEntries_Churches_ChurchId",
                table: "FinanceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Churches_ChurchId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Churches_ChurchId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Churches_ChurchId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Churches_ChurchId",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Workers_ChurchId_Email",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Users_ChurchId_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ChurchId_Name",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ChurchId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_FinanceEntries_ChurchId",
                table: "FinanceEntries");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecords_ChurchId_ServiceDate_ServiceType",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "FinanceEntries");

            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "AttendanceRecords");

            migrationBuilder.CreateIndex(
                name: "IX_Workers_Email",
                table: "Workers",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Name",
                table: "Teams",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_ServiceDate_ServiceType",
                table: "AttendanceRecords",
                columns: new[] { "ServiceDate", "ServiceType" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }
    }
}
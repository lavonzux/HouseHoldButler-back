using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseHoldButler.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetAlertIsReadProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "BudgetAlerts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAt",
                table: "BudgetAlerts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "BudgetAlerts");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "BudgetAlerts");
        }
    }
}

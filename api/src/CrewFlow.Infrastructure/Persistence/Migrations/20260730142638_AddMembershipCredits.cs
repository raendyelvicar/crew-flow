using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipCredits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditsRemainingThisPeriod",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreditsPerPeriod",
                table: "MembershipPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SubscriptionId",
                table: "Bookings",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Subscriptions_SubscriptionId",
                table: "Bookings",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Subscriptions_SubscriptionId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SubscriptionId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreditsRemainingThisPeriod",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CreditsPerPeriod",
                table: "MembershipPlans");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Bookings");
        }
    }
}

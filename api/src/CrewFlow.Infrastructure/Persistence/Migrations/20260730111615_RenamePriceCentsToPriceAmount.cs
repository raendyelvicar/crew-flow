using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenamePriceCentsToPriceAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceCents",
                table: "MembershipPlans",
                newName: "PriceAmount");

            migrationBuilder.RenameColumn(
                name: "PriceCents",
                table: "CreditPacks",
                newName: "PriceAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "MembershipPlans",
                newName: "PriceCents");

            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "CreditPacks",
                newName: "PriceCents");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aegis.Server.AspNetCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareUrnToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoftwareUrn",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Products SET SoftwareUrn = 'urn:ftt:software:conecalc:7' WHERE ProductName = 'ConeCalc 7';
                UPDATE Products SET SoftwareUrn = 'urn:ftt:software:sbicalc:3' WHERE ProductName = 'SBICalc 3';
                UPDATE Products SET SoftwareUrn = 'urn:ftt:software:cablesoft:3' WHERE ProductName = 'CableSoft 3';
                UPDATE Products SET SoftwareUrn = 'urn:ftt:software:imosoft:3' WHERE ProductName = 'IMOSoft 3';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SoftwareUrn",
                table: "Products",
                column: "SoftwareUrn",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_SoftwareUrn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SoftwareUrn",
                table: "Products");
        }
    }
}

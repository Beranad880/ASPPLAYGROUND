using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationASP01.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRodneCislo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rodne_cislo",
                table: "persons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rodne_cislo",
                table: "persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}

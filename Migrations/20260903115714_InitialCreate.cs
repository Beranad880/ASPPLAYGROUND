using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplicationASP01.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jmeno = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    datum_narozeni = table.Column<DateOnly>(type: "date", nullable: false),
                    trvala_adresa = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    rodne_cislo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "persons");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriTrack_Connection.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaMetasNutricionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MetaCalorias",
                table: "TbUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MetaCarboidratos",
                table: "TbUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MetaGorduras",
                table: "TbUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MetaProteinas",
                table: "TbUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaCalorias",
                table: "TbUsers");

            migrationBuilder.DropColumn(
                name: "MetaCarboidratos",
                table: "TbUsers");

            migrationBuilder.DropColumn(
                name: "MetaGorduras",
                table: "TbUsers");

            migrationBuilder.DropColumn(
                name: "MetaProteinas",
                table: "TbUsers");
        }
    }
}

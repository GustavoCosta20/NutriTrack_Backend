using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriTrack_Connection.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoDeTabelasIniciais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TbUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    AlturaEmCm = table.Column<double>(type: "double precision", nullable: false),
                    PesoEmKg = table.Column<double>(type: "double precision", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Genero = table.Column<int>(type: "integer", nullable: false),
                    NivelDeAtividade = table.Column<int>(type: "integer", nullable: false),
                    Objetivo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TbRefeicao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbRefeicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbRefeicao_TbUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "TbUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TbAlimentosConsumidos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefeicaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Quantidade = table.Column<double>(type: "double precision", nullable: false),
                    Unidade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Calorias = table.Column<double>(type: "double precision", nullable: false),
                    Proteinas = table.Column<double>(type: "double precision", nullable: false),
                    Carboidratos = table.Column<double>(type: "double precision", nullable: false),
                    Gorduras = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbAlimentosConsumidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbAlimentosConsumidos_TbRefeicao_RefeicaoId",
                        column: x => x.RefeicaoId,
                        principalTable: "TbRefeicao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TbAlimentosConsumidos_RefeicaoId",
                table: "TbAlimentosConsumidos",
                column: "RefeicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_TbRefeicao_UsuarioId",
                table: "TbRefeicao",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbAlimentosConsumidos");

            migrationBuilder.DropTable(
                name: "TbRefeicao");

            migrationBuilder.DropTable(
                name: "TbUsers");
        }
    }
}

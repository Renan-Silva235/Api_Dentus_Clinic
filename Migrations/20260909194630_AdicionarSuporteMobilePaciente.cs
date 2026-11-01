using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSuporteMobilePaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdAcesso",
                table: "Pacientes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusConfirmacao",
                table: "Consultas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DispositivosPaciente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPaciente = table.Column<int>(type: "int", nullable: false),
                    TokenPush = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivosPaciente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispositivosPaciente_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_IdAcesso",
                table: "Pacientes",
                column: "IdAcesso",
                unique: true,
                filter: "[IdAcesso] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosPaciente_IdPaciente",
                table: "DispositivosPaciente",
                column: "IdPaciente");

            migrationBuilder.AddForeignKey(
                name: "FK_Pacientes_Logins_IdAcesso",
                table: "Pacientes",
                column: "IdAcesso",
                principalTable: "Logins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pacientes_Logins_IdAcesso",
                table: "Pacientes");

            migrationBuilder.DropTable(
                name: "DispositivosPaciente");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_IdAcesso",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "IdAcesso",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "StatusConfirmacao",
                table: "Consultas");
        }
    }
}

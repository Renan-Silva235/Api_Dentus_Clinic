using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class AjustarPlanosParaAppendOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlanoAnteriorId",
                table: "Planos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VersaoAtual",
                table: "Planos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Planos_PlanoAnteriorId",
                table: "Planos",
                column: "PlanoAnteriorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_Planos_PlanoAnteriorId",
                table: "Planos",
                column: "PlanoAnteriorId",
                principalTable: "Planos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planos_Planos_PlanoAnteriorId",
                table: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_Planos_PlanoAnteriorId",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "PlanoAnteriorId",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "VersaoAtual",
                table: "Planos");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDentistaNoPlano : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdDentista",
                table: "Planos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Planos_IdDentista",
                table: "Planos",
                column: "IdDentista");

            migrationBuilder.AddForeignKey(
                name: "FK_Planos_Dentistas_IdDentista",
                table: "Planos",
                column: "IdDentista",
                principalTable: "Dentistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Planos_Dentistas_IdDentista",
                table: "Planos");

            migrationBuilder.DropIndex(
                name: "IX_Planos_IdDentista",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "IdDentista",
                table: "Planos");
        }
    }
}

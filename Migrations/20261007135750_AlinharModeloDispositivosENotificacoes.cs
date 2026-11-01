using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class AlinharModeloDispositivosENotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DispositivosPaciente",
                newName: "Dispositivos");

            migrationBuilder.RenameIndex(
                name: "IX_DispositivosPaciente_IdPaciente",
                table: "Dispositivos",
                newName: "IX_Dispositivos_IdPaciente");

            // sp_rename não acompanha a tabela: sem isso a chave primária e a FK
            // ficam com o nome antigo e quebram migrations futuras que as referenciem
            migrationBuilder.Sql(
                "EXEC sp_rename N'PK_DispositivosPaciente', N'PK_Dispositivos', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_DispositivosPaciente_Pacientes_IdPaciente', N'FK_Dispositivos_Pacientes_IdPaciente', N'OBJECT';");

            migrationBuilder.AddColumn<string>(
                name: "Plataforma",
                table: "Dispositivos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Dispositivos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdConsulta = table.Column<int>(type: "int", nullable: false),
                    TipoNotificacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StatusEnvio = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Consultas_IdConsulta",
                        column: x => x.IdConsulta,
                        principalTable: "Consultas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Nome",
                table: "Servicos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Especialidades_Nome",
                table: "Especialidades",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_IdConsulta_StatusEnvio",
                table: "Notificacoes",
                columns: new[] { "IdConsulta", "StatusEnvio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_Servicos_Nome",
                table: "Servicos");

            migrationBuilder.DropIndex(
                name: "IX_Especialidades_Nome",
                table: "Especialidades");

            migrationBuilder.AlterColumn<bool>(
                name: "Ativo",
                table: "Dispositivos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.DropColumn(
                name: "Plataforma",
                table: "Dispositivos");

            migrationBuilder.Sql(
                "EXEC sp_rename N'FK_Dispositivos_Pacientes_IdPaciente', N'FK_DispositivosPaciente_Pacientes_IdPaciente', N'OBJECT';");
            migrationBuilder.Sql(
                "EXEC sp_rename N'PK_Dispositivos', N'PK_DispositivosPaciente', N'OBJECT';");

            migrationBuilder.RenameIndex(
                name: "IX_Dispositivos_IdPaciente",
                table: "Dispositivos",
                newName: "IX_DispositivosPaciente_IdPaciente");

            migrationBuilder.RenameTable(
                name: "Dispositivos",
                newName: "DispositivosPaciente");
        }
    }
}

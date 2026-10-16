using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class MoverEmailDoPacienteParaLogin : Migration
    {
        // O e-mail passa a existir somente em Logins, onde tem função de
        // credencial de acesso. Pacientes sem login recebem um, para o e-mail
        // deles não ser perdido junto com a coluna.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TipoAcesso 3 = PACIENTE. A senha é 'Paciente@123' com hash BCrypt,
            // marcada como primeiro acesso: só serve até o paciente definir a dele.
            migrationBuilder.Sql(@"
INSERT INTO Logins (Email, Senha, TipoAcesso, PrimeiroAcesso)
SELECT p.Email,
       '$2a$11$HyPw34w6lY5PqYtCAjS/huWyYQUatpkWbZuDobrKKiyit5tsuxbam',
       3,
       1
FROM Pacientes p
WHERE p.IdAcesso IS NULL;
");

            migrationBuilder.Sql(@"
UPDATE p
SET p.IdAcesso = l.Id
FROM Pacientes p
INNER JOIN Logins l ON l.Email = p.Email
WHERE p.IdAcesso IS NULL;
");

            migrationBuilder.DropIndex(
                name: "IX_Pacientes_Email",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Pacientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Pacientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            // Sem recuperar o e-mail do login, todos os pacientes ficariam com
            // string vazia e o índice único abaixo falharia no segundo registro.
            migrationBuilder.Sql(@"
UPDATE p
SET p.Email = l.Email
FROM Pacientes p
INNER JOIN Logins l ON l.Id = p.IdAcesso;
");

            migrationBuilder.CreateIndex(
                name: "IX_Pacientes_Email",
                table: "Pacientes",
                column: "Email",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarStatusConsulta : Migration
    {
        // Consultas.Status tinha dois valores para cada situação, gravados por
        // partes diferentes do sistema: 'Concluida' e 'Encerrada' para consulta
        // realizada, 'EmAtendimento' e 'Em Consulta' para atendimento em curso.
        // Ficam valendo 'Encerrada' e 'Em Consulta', que são os valores que o
        // ConsultaService valida e que o frontend sabe exibir.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O trigger recusa qualquer alteração em consulta realizada, o que
            // incluiria estas linhas. Desligar é necessário para migrá-las.
            migrationBuilder.Sql(
                "DISABLE TRIGGER trg_ImpedirAlteracaoConsultaRealizada ON Consultas;");

            migrationBuilder.Sql("UPDATE Consultas SET Status = 'Encerrada' WHERE Status = 'Concluida';");
            migrationBuilder.Sql("UPDATE Consultas SET Status = 'Em Consulta' WHERE Status = 'EmAtendimento';");

            migrationBuilder.Sql(
                "ENABLE TRIGGER trg_ImpedirAlteracaoConsultaRealizada ON Consultas;");

            // Com os dados padronizados, 'Concluida' deixa de existir.
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER trg_ImpedirAlteracaoConsultaRealizada
ON Consultas
INSTEAD OF UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Status = 'Encerrada')
    BEGIN
        RAISERROR('Consulta já realizada não pode ser alterada.', 16, 1);
        RETURN;
    END

    -- Sendo INSTEAD OF, este UPDATE é o que realmente grava: toda coluna nova
    -- em Consultas precisa ser adicionada aqui, senão ela nunca será salva.
    UPDATE c
    SET c.DataConsulta      = i.DataConsulta,
        c.HoraConsulta      = i.HoraConsulta,
        c.Retorno           = i.Retorno,
        c.Status            = i.Status,
        c.StatusConfirmacao = i.StatusConfirmacao,
        c.IdDentista        = i.IdDentista,
        c.IdPaciente        = i.IdPaciente,
        c.IdServico         = i.IdServico
    FROM Consultas c
    INNER JOIN inserted i ON c.Id = i.Id;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Os dados não voltam: não há como saber quais linhas eram
            // 'Concluida' antes da padronização. O trigger volta a aceitar os
            // dois valores, para continuar funcionando se alguém regravar o
            // valor antigo.
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER trg_ImpedirAlteracaoConsultaRealizada
ON Consultas
INSTEAD OF UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Status IN ('Concluida', 'Encerrada'))
    BEGIN
        RAISERROR('Consulta já realizada não pode ser alterada.', 16, 1);
        RETURN;
    END

    UPDATE c
    SET c.DataConsulta      = i.DataConsulta,
        c.HoraConsulta      = i.HoraConsulta,
        c.Retorno           = i.Retorno,
        c.Status            = i.Status,
        c.StatusConfirmacao = i.StatusConfirmacao,
        c.IdDentista        = i.IdDentista,
        c.IdPaciente        = i.IdPaciente,
        c.IdServico         = i.IdServico
    FROM Consultas c
    INNER JOIN inserted i ON c.Id = i.Id;
END
");
        }
    }
}

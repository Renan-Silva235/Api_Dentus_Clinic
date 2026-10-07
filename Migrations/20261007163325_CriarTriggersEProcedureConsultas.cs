using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class CriarTriggersEProcedureConsultas : Migration
    {
        // Mapeamento usado em todos os objetos abaixo:
        //   Consultas.StatusConfirmacao é o enum StatusConfirmacaoEnum gravado como int:
        //     0 = Pendente, 1 = Confirmada, 2 = Recusada
        //   Consultas.Status é texto e guarda o fluxo interno da clínica:
        //     'Agendada', 'Aguardando', 'EmAtendimento', 'Concluida', 'Cancelada'
        //   O status "Realizada" do modelo corresponde a Status = 'Concluida'.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TRIGGER trg_ImpedirConflitoHorarioDentista
ON Consultas
AFTER INSERT, UPDATE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN Consultas c
            ON  c.IdDentista   = i.IdDentista
            AND c.DataConsulta = i.DataConsulta
            AND c.HoraConsulta = i.HoraConsulta
            AND c.Id <> i.Id
        WHERE i.Status <> 'Cancelada' AND i.StatusConfirmacao <> 2  -- 2 = Recusada
          AND c.Status <> 'Cancelada' AND c.StatusConfirmacao <> 2
    )
    BEGIN
        RAISERROR('Dentista já possui consulta agendada nesse horário.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
");

            migrationBuilder.Sql(@"
CREATE TRIGGER trg_ImpedirAlteracaoConsultaRealizada
ON Consultas
INSTEAD OF UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Status = 'Concluida')  -- Concluida = Realizada
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

            migrationBuilder.Sql(@"
CREATE TRIGGER trg_CancelamentoAutomaticoRecusa
ON Consultas
AFTER UPDATE
AS
BEGIN
    -- Comparar inserted com deleted é o que evita loop: no UPDATE que este
    -- próprio trigger dispara, StatusConfirmacao já era 2 antes, então a
    -- condição fica falsa e ele não torna a executar.
    UPDATE c
    SET c.Status = 'Cancelada'
    FROM Consultas c
    INNER JOIN inserted i ON c.Id = i.Id
    INNER JOIN deleted  d ON d.Id = i.Id
    WHERE i.StatusConfirmacao = 2    -- virou Recusada
      AND d.StatusConfirmacao <> 2
      AND i.Status <> 'Cancelada';
END
");

            migrationBuilder.Sql(@"
CREATE PROCEDURE sp_ConfirmarPresenca
    @id_consulta INT,
    @acao        VARCHAR(20)
AS
BEGIN
    IF @acao NOT IN ('Confirmar', 'Recusar')
    BEGIN
        RAISERROR('Ação inválida. Use ''Confirmar'' ou ''Recusar''.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @status VARCHAR(20);
        SELECT @status = Status FROM Consultas WHERE Id = @id_consulta;

        IF @status IS NULL
        BEGIN
            ;THROW 50001, 'Consulta não encontrada.', 1;
        END

        IF @status = 'Concluida'
        BEGIN
            ;THROW 50002, 'Consulta já realizada não pode ser confirmada nem recusada.', 1;
        END

        IF @status = 'Cancelada'
        BEGIN
            ;THROW 50003, 'Consulta cancelada não pode ser confirmada nem recusada.', 1;
        END

        -- 'Recusar' grava apenas StatusConfirmacao = 2; o cancelamento do
        -- Status fica por conta do trg_CancelamentoAutomaticoRecusa.
        UPDATE Consultas
        SET StatusConfirmacao = CASE WHEN @acao = 'Confirmar' THEN 1 ELSE 2 END
        WHERE Id = @id_consulta;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ConfirmarPresenca;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_CancelamentoAutomaticoRecusa;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_ImpedirAlteracaoConsultaRealizada;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_ImpedirConflitoHorarioDentista;");
        }
    }
}

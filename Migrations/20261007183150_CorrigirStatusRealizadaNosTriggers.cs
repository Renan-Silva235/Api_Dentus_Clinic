using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirStatusRealizadaNosTriggers : Migration
    {
        // O projeto grava dois valores diferentes para consulta finalizada:
        // 'Encerrada' pela tela da recepcionista (ConsultaService.AtualizarStatusAsync)
        // e 'Concluida' pelo registro de atendimento (AtendimentoService).
        // Os objetos abaixo passam a tratar os dois como "Realizada".

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
CREATE OR ALTER PROCEDURE sp_ConfirmarPresenca
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

        IF @status IN ('Concluida', 'Encerrada')
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
            migrationBuilder.Sql(@"
CREATE OR ALTER TRIGGER trg_ImpedirAlteracaoConsultaRealizada
ON Consultas
INSTEAD OF UPDATE
AS
BEGIN
    IF EXISTS (SELECT 1 FROM deleted WHERE Status = 'Concluida')
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

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_ConfirmarPresenca
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
    }
}

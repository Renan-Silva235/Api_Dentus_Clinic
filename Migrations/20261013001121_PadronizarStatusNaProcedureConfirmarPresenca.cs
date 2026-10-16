using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DentusClinic.API.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarStatusNaProcedureConfirmarPresenca : Migration
    {
        // Complementa a PadronizarStatusConsulta: a procedure continuava
        // verificando 'Concluida', valor que deixou de existir na base.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        IF @status = 'Encerrada'
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

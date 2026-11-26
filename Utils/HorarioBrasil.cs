namespace DentusClinic.API.Utils;

public static class HorarioBrasil
{
    // O servidor de hospedagem roda com o fuso horário dele mesmo (ex.: Europa),
    // não o do Brasil. Usar DateTime.Now/DateTime.Today direto faria a API comparar
    // "agora" com o horário errado e rejeitar consultas futuras como se já tivessem
    // passado. Por isso todo "agora" usado em regra de negócio de horário de consulta
    // passa por aqui, convertido explicitamente para o fuso de São Paulo.
    private static readonly TimeZoneInfo FusoBrasil = ResolverFusoBrasil();

    private static TimeZoneInfo ResolverFusoBrasil()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }

    public static DateTime Agora() => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, FusoBrasil);

    public static DateOnly Hoje() => DateOnly.FromDateTime(Agora());
}

namespace GestionTubos.Shared;

public static class HoraArgentina
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
    public static DateTime Ahora => DateTime.UtcNow.EnArgentina();
    public static DateTime Hoy => Ahora.Date;

    // SQL Server devuelve DateTime sin Kind; los campos terminados en Utc son UTC.
    public static DateTime EnArgentina(this DateTime fechaUtc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc), Zona);
}

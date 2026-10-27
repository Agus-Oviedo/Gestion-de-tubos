namespace GestionTubos.Shared.Operaciones;

public sealed record ConteoAlquiler(int DiasTranscurridos, int DiasSinCargo, int DiasACobrar);

public static class CalculadoraAlquiler
{
    // Se cuentan cambios de fecha en Argentina. El día de entrega comienza en cero.
    public static ConteoAlquiler Calcular(DateTime entregaUtc, DateTime? devolucionUtc, int? diasSinCargo, string modalidad, DateTime corteUtc)
    {
        var inicio = DateOnly.FromDateTime(entregaUtc.EnArgentina());
        var fin = DateOnly.FromDateTime((devolucionUtc ?? corteUtc).EnArgentina());
        var dias = Math.Max(0, fin.DayNumber - inicio.DayNumber);
        var gratis = Math.Max(0, diasSinCargo ?? 0);
        return new(dias, gratis, modalidad.Equals("Alquiler", StringComparison.OrdinalIgnoreCase) ? Math.Max(0, dias - gratis) : 0);
    }
}

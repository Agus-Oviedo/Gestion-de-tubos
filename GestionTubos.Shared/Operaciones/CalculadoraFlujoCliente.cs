namespace GestionTubos.Shared.Operaciones;

public sealed record FlujoMesCliente(DateOnly Mes, int Entregados, int Devueltos);
public sealed record FlujoGasCliente(string Gas, int Entregados, int Devueltos);
public sealed record FlujoCliente(int Entregados, int Devueltos, int SaldoInicial, int SaldoFinal, double? PromedioDiasDevolucion, IReadOnlyList<FlujoMesCliente> Meses, IReadOnlyList<FlujoGasCliente> Gases)
{
    public int Variacion => Entregados - Devueltos;
}
public static class CalculadoraFlujoCliente
{
    public static FlujoCliente Calcular(IReadOnlyList<EstadisticaTuboClienteDto> tubos, DateOnly desde, DateOnly hasta)
    {
        if (desde > hasta) throw new ArgumentException("La fecha desde debe ser anterior o igual a la fecha hasta.");
        DateOnly Fecha(DateTime utc) => DateOnly.FromDateTime(utc.EnArgentina());
        bool EnPeriodo(DateTime utc) => Fecha(utc) >= desde && Fecha(utc) <= hasta;
        var entregas = tubos.Where(x => EnPeriodo(x.EntregaUtc)).ToList();
        // Una devolución cuenta por su propia fecha, incluso si la entrega fue anterior al período.
        var devoluciones = tubos.Where(x => x.DevolucionUtc.HasValue && EnPeriodo(x.DevolucionUtc.Value)).ToList();
        var inicial = tubos.Count(x => Fecha(x.EntregaUtc) < desde && (!x.DevolucionUtc.HasValue || Fecha(x.DevolucionUtc.Value) >= desde));
        var final = tubos.Count(x => Fecha(x.EntregaUtc) <= hasta && (!x.DevolucionUtc.HasValue || Fecha(x.DevolucionUtc.Value) > hasta));
        var meses = new List<FlujoMesCliente>();
        for (var mes = new DateOnly(desde.Year, desde.Month, 1); mes <= hasta;)
        {
            meses.Add(new(mes, entregas.Count(x => Fecha(x.EntregaUtc).Year == mes.Year && Fecha(x.EntregaUtc).Month == mes.Month),
                devoluciones.Count(x => Fecha(x.DevolucionUtc!.Value).Year == mes.Year && Fecha(x.DevolucionUtc.Value).Month == mes.Month)));
            if (mes.Year == 9999 && mes.Month == 12) break;
            mes = mes.AddMonths(1);
        }
        var gases = entregas.Concat(devoluciones).Select(x => x.Gas).Distinct().OrderBy(x => x)
            .Select(gas => new FlujoGasCliente(gas, entregas.Count(x => x.Gas == gas), devoluciones.Count(x => x.Gas == gas))).ToList();
        double? promedio = devoluciones.Count == 0 ? null : devoluciones.Average(x => (double)x.DiasConCliente);
        return new(entregas.Count, devoluciones.Count, inicial, final, promedio, meses, gases);
    }
}

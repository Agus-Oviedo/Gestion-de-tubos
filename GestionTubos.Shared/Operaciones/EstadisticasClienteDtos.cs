namespace GestionTubos.Shared.Operaciones;

public sealed record EstadisticaTuboClienteDto(long OperacionId, int TuboId, string Codigo, string Gas, string Capacidad, string Modalidad, string Remito, DateTime EntregaUtc, DateTime? DevolucionUtc, int DiasConCliente);
public sealed record EstadisticasClienteDto(int ClienteId, string Cliente, DateTime ActualizadoUtc, IReadOnlyList<EstadisticaTuboClienteDto> Tubos)
{
    public int TubosActuales => Tubos.Where(x => !x.DevolucionUtc.HasValue).Select(x => x.TuboId).Distinct().Count();
    public int TubosEntregados => Tubos.Count;
    public int TubosDevueltos => Tubos.Count(x => x.DevolucionUtc.HasValue);
}

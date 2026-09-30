using GestionTubos.Application.Interfaces;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared.Operaciones;
using Microsoft.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Services;

public sealed class OperacionService(GestionTubosDbContext db) : IOperacionService
{
    public async Task<IReadOnlyList<MovimientoDto>> MovimientosAsync(int? cilindroId, CancellationToken ct)
    {
        var query = db.MovimientosCilindro.IgnoreQueryFilters().AsNoTracking().Include(x => x.Tubo).AsQueryable();
        if (cilindroId.HasValue) query = query.Where(x => x.TuboId == cilindroId);
        var ubicaciones = await db.Ubicaciones.IgnoreQueryFilters().AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Nombre, ct);
        var clientes = await db.Clientes.IgnoreQueryFilters().AsNoTracking().ToDictionaryAsync(x => x.Id, NombreCliente, ct);
        return (await query.OrderByDescending(x => x.FechaHoraUtc).Take(500).ToListAsync(ct)).Select(x => new MovimientoDto(
            x.Id, x.TuboId, x.Tubo.CodigoInterno, x.FechaHoraUtc, x.Tipo,
            x.UbicacionOrigenId.HasValue && ubicaciones.TryGetValue(x.UbicacionOrigenId.Value, out var origen) ? origen : null,
            x.UbicacionDestinoId.HasValue && ubicaciones.TryGetValue(x.UbicacionDestinoId.Value, out var destino) ? destino : null,
            x.ClienteId.HasValue && clientes.TryGetValue(x.ClienteId.Value, out var cliente) ? cliente : null,
            x.EstadoAnterior, x.EstadoPosterior, x.UsuarioResponsable, x.Observaciones)).ToList();
    }

    public async Task<IReadOnlyList<PruebaHidraulicaDto>> PruebasAsync(CancellationToken ct) =>
        (await db.PruebasHidraulicas.IgnoreQueryFilters().AsNoTracking().Include(x => x.Tubo).OrderBy(x => x.FechaVencimiento).ToListAsync(ct)).Select(MapearPrueba).ToList();

    public async Task<PruebaHidraulicaDto> RegistrarPruebaAsync(PruebaHidraulicaGuardarDto dto, CancellationToken ct)
    {
        if (dto.FechaVencimiento <= dto.FechaRealizacion) throw new InvalidOperationException("El vencimiento debe ser posterior a la realización.");
        var cilindro = await db.Tubos.FirstOrDefaultAsync(x => x.Id == dto.CilindroId, ct) ?? throw new InvalidOperationException("No se encontró el cilindro.");
        var prueba = new PruebaHidraulica { TuboId = dto.CilindroId, FechaRealizacion = dto.FechaRealizacion, FechaVencimiento = dto.FechaVencimiento, Aprobada = dto.Aprobada, EntidadRealizadora = dto.EntidadRealizadora.Trim(), NumeroCertificado = Limpiar(dto.NumeroCertificado), Observaciones = Limpiar(dto.Observaciones) };
        db.PruebasHidraulicas.Add(prueba);
        var estadoAnterior = Estado(cilindro);
        if (dto.Aprobada && cilindro.EstadoDisponibilidad == EstadoDisponibilidad.EnPruebaHidraulica)
            cilindro.EstadoDisponibilidad = EstadoDisponibilidad.Disponible;
        else if (!dto.Aprobada)
            cilindro.EstadoDisponibilidad = EstadoDisponibilidad.EnMantenimiento;
        db.MovimientosCilindro.Add(Movimiento(cilindro, "Prueba hidráulica", cilindro.UbicacionId, cilindro.UbicacionId, null, $"Prueba {(dto.Aprobada ? "aprobada" : "rechazada")}. Vence {dto.FechaVencimiento:dd/MM/yyyy}", estadoAnterior));
        await db.SaveChangesAsync(ct);
        prueba.Tubo = cilindro;
        return MapearPrueba(prueba);
    }

    public async Task<IReadOnlyList<OperacionDto>> OperacionesAsync(bool soloAbiertas, CancellationToken ct)
    {
        var q = db.OperacionesCilindro.AsNoTracking().Include(x => x.Cliente).Include(x => x.Tubo).AsQueryable();
        if (soloAbiertas) q = q.Where(x => x.FechaDevolucionUtc == null);
        return (await q.OrderByDescending(x => x.FechaHoraUtc).ToListAsync(ct)).Select(MapearOperacion).ToList();
    }

    public async Task<OperacionDto> EntregarAsync(EntregaGuardarDto dto, CancellationToken ct)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == dto.ClienteId, ct) ?? throw new InvalidOperationException("No se encontró el cliente.");
        var cilindro = await db.Tubos.Include(x => x.Ubicacion).FirstOrDefaultAsync(x => x.Id == dto.CilindroId, ct) ?? throw new InvalidOperationException("No se encontró el cilindro.");
        if (cilindro.EstadoDisponibilidad != EstadoDisponibilidad.Disponible) throw new InvalidOperationException("El cilindro no está disponible.");
        if (cilindro.TipoPropietario == TipoPropietarioTubo.Cliente && cilindro.ClientePropietarioId != dto.ClienteId) throw new InvalidOperationException("Este cilindro pertenece a otro cliente y no puede ser entregado.");
        var ultimaPrueba = await db.PruebasHidraulicas.Where(x => x.TuboId == cilindro.Id && x.Aprobada).OrderByDescending(x => x.FechaVencimiento).FirstOrDefaultAsync(ct);
        if (ultimaPrueba is null || ultimaPrueba.FechaVencimiento < DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy)) throw new InvalidOperationException("El cilindro no posee una prueba hidráulica vigente.");
        var ubicacionCliente = await UbicacionesSistema.ObtenerEnPoderDelClienteAsync(db, ct);
        var operacion = new OperacionCilindro { Numero = await SiguienteNumero(ct), Tipo = dto.Modalidad, ClienteId = dto.ClienteId, TuboId = dto.CilindroId, DiasSinCargo = dto.DiasSinCargo, FechaLimite = dto.DiasSinCargo > 0 ? DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy.AddDays(dto.DiasSinCargo)) : null, PersonaRecibe = Limpiar(dto.PersonaRecibe), Observaciones = Limpiar(dto.Observaciones) };
        db.OperacionesCilindro.Add(operacion);
        var anterior = Estado(cilindro); var origen = cilindro.UbicacionId;
        cilindro.EstadoDisponibilidad = EstadoDisponibilidad.EnPoderCliente;
        cilindro.UbicacionId = ubicacionCliente;
        db.MovimientosCilindro.Add(Movimiento(cilindro, "Entrega", origen, ubicacionCliente, cliente.Id, dto.Observaciones, anterior));
        await db.SaveChangesAsync(ct); operacion.Cliente = cliente; operacion.Tubo = cilindro; return MapearOperacion(operacion);
    }

    public async Task<OperacionDto> RecibirAsync(RecepcionGuardarDto dto, CancellationToken ct)
    {
        var op = await db.OperacionesCilindro.Include(x => x.Cliente).Include(x => x.Tubo).FirstOrDefaultAsync(x => x.Id == dto.OperacionId, ct) ?? throw new InvalidOperationException("No se encontró la operación.");
        if (op.FechaDevolucionUtc.HasValue) throw new InvalidOperationException("La operación ya fue cerrada.");
        if (!await db.Ubicaciones.AnyAsync(x => x.Id == dto.UbicacionDestinoId, ct)) throw new InvalidOperationException("La ubicación no existe.");
        var cilindro = op.Tubo; var anterior = Estado(cilindro); var origen = cilindro.UbicacionId;
        cilindro.EstadoFisico = (EstadoFisico)dto.EstadoFisico; cilindro.EstadoContenido = (EstadoContenido)dto.EstadoContenido;
        cilindro.EstadoDisponibilidad = dto.RequiereInspeccion ? EstadoDisponibilidad.PendienteInspeccion : EstadoDisponibilidad.Disponible; cilindro.UbicacionId = dto.UbicacionDestinoId;
        op.FechaDevolucionUtc = DateTime.UtcNow; op.Estado = "Cerrada";
        db.MovimientosCilindro.Add(Movimiento(cilindro, "Recepción", origen, dto.UbicacionDestinoId, op.ClienteId, dto.Observaciones, anterior));
        await db.SaveChangesAsync(ct); return MapearOperacion(op);
    }

    public async Task<DashboardDto> DashboardAsync(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy); var proximas = hoy.AddDays(30);
        var propios=await db.Tubos.CountAsync(x=>x.TipoPropietario==TipoPropietarioTubo.Empresa,ct);var propiosPlanta=await db.Tubos.CountAsync(x=>x.TipoPropietario==TipoPropietarioTubo.Empresa&&x.EstadoDisponibilidad!=EstadoDisponibilidad.EnPoderCliente&&x.EstadoDisponibilidad!=EstadoDisponibilidad.EnReparto&&x.EstadoDisponibilidad!=EstadoDisponibilidad.Extraviado,ct);var propiosClientes=await db.Tubos.CountAsync(x=>x.TipoPropietario==TipoPropietarioTubo.Empresa&&x.EstadoDisponibilidad==EstadoDisponibilidad.EnPoderCliente,ct);var ajenosPlanta=await db.Tubos.CountAsync(x=>x.TipoPropietario==TipoPropietarioTubo.Cliente&&x.EstadoDisponibilidad!=EstadoDisponibilidad.EnPoderCliente&&x.EstadoDisponibilidad!=EstadoDisponibilidad.EnReparto&&x.EstadoDisponibilidad!=EstadoDisponibilidad.Extraviado,ct);var enCarga=await db.Tubos.CountAsync(x=>x.EstadoDisponibilidad==EstadoDisponibilidad.EnCarga,ct);var enReparto=await db.Tubos.CountAsync(x=>x.EstadoDisponibilidad==EstadoDisponibilidad.EnReparto,ct);var inspeccion=await db.Tubos.CountAsync(x=>x.EstadoDisponibilidad==EstadoDisponibilidad.PendienteInspeccion,ct);
        var ultimas = await db.PruebasHidraulicas.Where(x=>x.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.EnPoderCliente).GroupBy(x => x.TuboId).Select(g => g.OrderByDescending(x => x.FechaVencimiento).First()).ToListAsync(ct);
        var vencidas = ultimas.Count(x => !x.Aprobada || x.FechaVencimiento < hoy); var cerca = ultimas.Count(x => x.Aprobada && x.FechaVencimiento >= hoy && x.FechaVencimiento <= proximas);
        var abiertas = await db.OperacionesCilindro.CountAsync(x => x.FechaDevolucionUtc == null&&x.Tubo.TipoPropietario==TipoPropietarioTubo.Empresa, ct); var movimientos = await MovimientosAsync(null, ct);
        return new(propios,propiosPlanta,propiosClientes,ajenosPlanta,enCarga,enReparto,inspeccion,vencidas,cerca,abiertas,movimientos.Take(8).ToList());
    }

    public async Task IniciarCargaAsync(CargaIniciarDto dto,string usuario,CancellationToken ct)
    {
        var c=await db.Tubos.FirstOrDefaultAsync(x=>x.Id==dto.CilindroId,ct)??throw new InvalidOperationException("No se encontró el cilindro.");
        if(c.EstadoDisponibilidad!=EstadoDisponibilidad.Disponible)throw new InvalidOperationException("Solo se puede cargar un cilindro disponible.");
        if(c.EstadoContenido is not (EstadoContenido.Vacio or EstadoContenido.ParcialmenteLleno))throw new InvalidOperationException("El cilindro debe estar vacío o parcialmente lleno.");
        if(c.EstadoFisico is EstadoFisico.Danado or EstadoFisico.Inutilizable)throw new InvalidOperationException("El estado físico no permite iniciar la carga.");
        await ValidarPruebaVigente(c.Id,ct);var anterior=Estado(c);c.EstadoDisponibilidad=EstadoDisponibilidad.EnCarga;
        db.MovimientosCilindro.Add(MovimientoUsuario(c,"Ingreso a carga",c.UbicacionId,c.UbicacionId,null,dto.Observaciones,usuario,anterior));await db.SaveChangesAsync(ct);
    }

    public async Task FinalizarCargaAsync(CargaFinalizarDto dto,string usuario,CancellationToken ct)
    {
        var c=await db.Tubos.FirstOrDefaultAsync(x=>x.Id==dto.CilindroId,ct)??throw new InvalidOperationException("No se encontró el cilindro.");
        if(c.EstadoDisponibilidad!=EstadoDisponibilidad.EnCarga)throw new InvalidOperationException("El cilindro no se encuentra en carga.");
        await ValidarPruebaVigente(c.Id,ct);var anterior=Estado(c);c.EstadoContenido=EstadoContenido.Lleno;c.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;
        db.MovimientosCilindro.Add(MovimientoUsuario(c,"Carga finalizada",c.UbicacionId,c.UbicacionId,null,dto.Observaciones,usuario,anterior));await db.SaveChangesAsync(ct);
    }

    public async Task CancelarCargaAsync(CargaCancelarDto dto,string usuario,CancellationToken ct)
    {
        var c=await db.Tubos.FirstOrDefaultAsync(x=>x.Id==dto.CilindroId,ct)??throw new InvalidOperationException("No se encontró el cilindro.");
        if(c.EstadoDisponibilidad!=EstadoDisponibilidad.EnCarga)throw new InvalidOperationException("El cilindro no se encuentra en carga.");
        var destino=dto.Destino switch{"Disponible"=>EstadoDisponibilidad.Disponible,"Inspeccion"=>EstadoDisponibilidad.PendienteInspeccion,"Mantenimiento"=>EstadoDisponibilidad.EnMantenimiento,_=>throw new InvalidOperationException("Destino de cancelación inválido.")};
        var anterior=Estado(c);c.EstadoDisponibilidad=destino;db.MovimientosCilindro.Add(MovimientoUsuario(c,"Carga cancelada",c.UbicacionId,c.UbicacionId,null,dto.Motivo,usuario,anterior));await db.SaveChangesAsync(ct);
    }

    public async Task IngresarCilindroClienteAsync(IngresoCilindroClienteDto dto,string usuario,CancellationToken ct)
    {
        var c=await db.Tubos.FirstOrDefaultAsync(x=>x.Id==dto.CilindroId,ct)??throw new InvalidOperationException("No se encontró el cilindro.");
        if(c.TipoPropietario!=TipoPropietarioTubo.Cliente||c.ClientePropietarioId!=dto.ClienteId)throw new InvalidOperationException("El cilindro no pertenece al cliente seleccionado.");
        if(!await db.Ubicaciones.AnyAsync(x=>x.Id==dto.UbicacionDestinoId,ct))throw new InvalidOperationException("La ubicación de ingreso no existe.");
        if(c.EstadoDisponibilidad is EstadoDisponibilidad.Reservado or EstadoDisponibilidad.EnReparto)throw new InvalidOperationException("El cilindro está vinculado a una salida activa.");
        var destino=dto.Motivo switch{"Carga"=>EstadoDisponibilidad.EnCarga,"Inspeccion"=>EstadoDisponibilidad.PendienteInspeccion,"Mantenimiento"=>EstadoDisponibilidad.EnMantenimiento,"PruebaHidraulica"=>EstadoDisponibilidad.EnPruebaHidraulica,_=>throw new InvalidOperationException("Motivo de ingreso inválido.")};
        var anterior=Estado(c);var origen=c.UbicacionId;c.EstadoFisico=(EstadoFisico)dto.EstadoFisico;c.EstadoContenido=dto.Motivo=="Carga"?EstadoContenido.Vacio:(EstadoContenido)dto.EstadoContenido;c.EstadoDisponibilidad=destino;c.UbicacionId=dto.UbicacionDestinoId;
        var obs=$"Motivo: {dto.Motivo}. {Limpiar(dto.Observaciones)}".Trim();db.MovimientosCilindro.Add(MovimientoUsuario(c,"Ingreso de cilindro del cliente",origen,c.UbicacionId,dto.ClienteId,obs,usuario,anterior));await db.SaveChangesAsync(ct);
    }

    private async Task ValidarPruebaVigente(int cilindroId,CancellationToken ct){var hoy=DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy);var vigente=await db.PruebasHidraulicas.AnyAsync(x=>x.TuboId==cilindroId&&x.Aprobada&&x.FechaVencimiento>=hoy,ct);if(!vigente)throw new InvalidOperationException("El cilindro no posee una prueba hidráulica vigente.");}
    private static MovimientoCilindro MovimientoUsuario(Tubo c,string tipo,int? origen,int? destino,int? cliente,string? obs,string usuario,string anterior)=>new(){TuboId=c.Id,Tipo=tipo,UbicacionOrigenId=origen,UbicacionDestinoId=destino,ClienteId=cliente,EstadoAnterior=anterior,EstadoPosterior=Estado(c),Observaciones=Limpiar(obs),UsuarioResponsable=usuario};

    private async Task<string> SiguienteNumero(CancellationToken ct) => $"OP-{GestionTubos.Shared.HoraArgentina.Hoy:yyyyMMdd}-{(await db.OperacionesCilindro.CountAsync(ct) + 1):D5}";
    private static MovimientoCilindro Movimiento(Tubo c, string tipo, int? origen, int? destino, int? cliente, string? obs, string? anterior = null) => new() { TuboId = c.Id, Tipo = tipo, UbicacionOrigenId = origen, UbicacionDestinoId = destino, ClienteId = cliente, EstadoAnterior = anterior, EstadoPosterior = Estado(c), Observaciones = Limpiar(obs) };
    private static string Estado(Tubo x) => $"Físico={(int)x.EstadoFisico};Contenido={(int)x.EstadoContenido};Disponibilidad={(int)x.EstadoDisponibilidad};Ubicación={x.UbicacionId}";
    private static PruebaHidraulicaDto MapearPrueba(PruebaHidraulica x) { var hoy = DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy); var e = !x.Aprobada || x.FechaVencimiento < hoy ? "Vencida/No aprobada" : x.FechaVencimiento <= hoy.AddDays(30) ? "Próxima a vencer" : "Vigente"; return new(x.Id, x.TuboId, x.Tubo.CodigoInterno, x.FechaRealizacion, x.FechaVencimiento, x.Aprobada, e, x.EntidadRealizadora, x.NumeroCertificado, x.Observaciones); }
    private static OperacionDto MapearOperacion(OperacionCilindro x) => new(x.Id, x.Numero, x.Tipo, x.ClienteId, NombreCliente(x.Cliente), x.TuboId, x.Tubo.CodigoInterno, x.FechaHoraUtc, x.Estado, x.FechaLimite, x.FechaDevolucionUtc, x.PersonaRecibe, x.Observaciones);
    private static string NombreCliente(Cliente x) => x.RazonSocial ?? $"{x.Apellido}, {x.Nombre}".Trim(' ', ',');
    private static string? Limpiar(string? x) => string.IsNullOrWhiteSpace(x) ? null : x.Trim();
}

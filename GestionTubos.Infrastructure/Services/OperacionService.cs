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
        var q = db.OperacionesCilindro.AsNoTracking().Include(x => x.Cliente).Include(x => x.Tubo).Include(x => x.RemitoDevolucion).Include(x => x.RemitoEntrega).AsQueryable();
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

    public async Task<OperacionDto> RecibirAsync(RecepcionGuardarDto dto, string usuario, CancellationToken ct)
    {
        var op = await db.OperacionesCilindro.Include(x => x.Cliente).Include(x => x.Tubo).Include(x => x.RemitoDevolucion).Include(x => x.RemitoEntrega).FirstOrDefaultAsync(x => x.Id == dto.OperacionId, ct) ?? throw new InvalidOperationException("No se encontró la operación.");
        if (op.FechaDevolucionUtc.HasValue) throw new InvalidOperationException("La operación ya fue cerrada.");
        if (op.RemitoDevolucion is not null) throw new InvalidOperationException("La devolución ya tiene un remito emitido.");
        if (!await db.Ubicaciones.AnyAsync(x => x.Id == dto.UbicacionDestinoId, ct)) throw new InvalidOperationException("La ubicación no existe.");
        var cilindro = op.Tubo; var anterior = Estado(cilindro); var origen = cilindro.UbicacionId;
        cilindro.EstadoFisico = (EstadoFisico)dto.EstadoFisico; cilindro.EstadoContenido = (EstadoContenido)dto.EstadoContenido;
        cilindro.EstadoDisponibilidad = dto.RequiereInspeccion ? EstadoDisponibilidad.PendienteInspeccion : EstadoDisponibilidad.Disponible; cilindro.UbicacionId = dto.UbicacionDestinoId;
        op.FechaDevolucionUtc = DateTime.UtcNow; op.Estado = "Cerrada";
        var siguiente = (await db.RemitosDevolucion.MaxAsync(x => (long?)x.Id, ct) ?? 0) + 1;
        op.RemitoDevolucion = new RemitoDevolucion
        {
            Numero = $"DEV-{siguiente:000000}", UsuarioReceptor = usuario,
            PersonaEntrega = Limpiar(dto.PersonaEntrega), DniEntrega = Limpiar(dto.DniEntrega),
            Observaciones = Limpiar(dto.Observaciones)
        };
        db.MovimientosCilindro.Add(Movimiento(cilindro, "Recepción", origen, dto.UbicacionDestinoId, op.ClienteId, dto.Observaciones, anterior));
        await db.SaveChangesAsync(ct); return MapearOperacion(op);
    }

    public async Task<IReadOnlyList<RemitoHistorialDto>> RemitosAsync(int? clienteId, int? tuboId, string? tipo, CancellationToken ct)
    {
        var resultado = new List<RemitoHistorialDto>();
        if (string.IsNullOrWhiteSpace(tipo) || tipo.Equals("Entrega", StringComparison.OrdinalIgnoreCase))
        {
            var entregas = db.Remitos.AsNoTracking().Where(x => x.FechaFirmaUtc != null && x.Estado == "Firmado").Include(x => x.Pedido).ThenInclude(x => x.Cliente)
                .Include(x => x.Pedido).ThenInclude(x => x.Detalles).ThenInclude(x => x.Tubo).ThenInclude(x => x.TipoGas)
                .Include(x => x.Pedido).ThenInclude(x => x.Detalles).ThenInclude(x => x.Tubo).ThenInclude(x => x.CapacidadTubo).AsQueryable();
            if (clienteId.HasValue) entregas = entregas.Where(x => x.Pedido.ClienteId == clienteId.Value);
            if (tuboId.HasValue) entregas = entregas.Where(x => x.Pedido.Detalles.Any(d => d.TuboId == tuboId.Value));
            foreach (var x in await entregas.ToListAsync(ct))
                resultado.Add(new(x.Id, "Entrega", x.Numero, x.FechaFirmaUtc ?? x.FechaEmisionUtc, x.Estado, x.Pedido.ClienteId, NombreCliente(x.Pedido.Cliente), x.PedidoId, x.Pedido.Numero, null, x.UsuarioEntrega ?? x.UsuarioEmisor, x.PersonaRecibe, x.DniRecibe, x.Pedido.Observaciones,
                    x.Pedido.Detalles.Select(d => new RemitoTuboDto(d.TuboId, d.Tubo.CodigoInterno, d.Tubo.TipoGas.Nombre, $"{d.Tubo.CapacidadTubo.Valor} {d.Tubo.CapacidadTubo.Unidad}")).ToList()));
        }
        if (string.IsNullOrWhiteSpace(tipo) || tipo.Equals("Devolución", StringComparison.OrdinalIgnoreCase) || tipo.Equals("Devolucion", StringComparison.OrdinalIgnoreCase))
        {
            var devoluciones = db.RemitosDevolucion.AsNoTracking().Include(x => x.OperacionCilindro).ThenInclude(x => x.Cliente)
                .Include(x => x.OperacionCilindro).ThenInclude(x => x.Tubo).ThenInclude(x => x.TipoGas)
                .Include(x => x.OperacionCilindro).ThenInclude(x => x.Tubo).ThenInclude(x => x.CapacidadTubo).AsQueryable();
            if (clienteId.HasValue) devoluciones = devoluciones.Where(x => x.OperacionCilindro.ClienteId == clienteId.Value);
            if (tuboId.HasValue) devoluciones = devoluciones.Where(x => x.OperacionCilindro.TuboId == tuboId.Value);
            foreach (var x in await devoluciones.ToListAsync(ct))
            {
                var op = x.OperacionCilindro; var tubo = op.Tubo;
                resultado.Add(new(x.Id, "Devolución", x.Numero, x.FechaEmisionUtc, x.Estado, op.ClienteId, NombreCliente(op.Cliente), null, null, op.Id, x.UsuarioReceptor, x.PersonaEntrega, x.DniEntrega, x.Observaciones,
                    [new RemitoTuboDto(tubo.Id, tubo.CodigoInterno, tubo.TipoGas.Nombre, $"{tubo.CapacidadTubo.Valor} {tubo.CapacidadTubo.Unidad}")]));
            }
            var lotes=db.DevolucionesLote.AsNoTracking().Where(x=>x.Estado=="Confirmado").Include(x=>x.Cliente).Include(x=>x.RemitoEntrega).ThenInclude(x=>x.Pedido).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.TipoGas).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.CapacidadTubo).AsQueryable();
            if(clienteId.HasValue)lotes=lotes.Where(x=>x.ClienteId==clienteId.Value);if(tuboId.HasValue)lotes=lotes.Where(x=>x.Detalles.Any(d=>d.OperacionCilindro.TuboId==tuboId.Value));
            foreach(var x in await lotes.ToListAsync(ct))resultado.Add(new(x.Id,"Devolución",x.Numero,x.FechaConfirmacionUtc??x.FechaCreacionUtc,x.Estado,x.ClienteId,NombreCliente(x.Cliente),x.RemitoEntrega.PedidoId,x.RemitoEntrega.Pedido.Numero,null,x.UsuarioReceptor,x.PersonaEntrega,x.DniEntrega,x.Observaciones,x.Detalles.Select(d=>new RemitoTuboDto(d.OperacionCilindro.TuboId,d.OperacionCilindro.Tubo.CodigoInterno,d.OperacionCilindro.Tubo.TipoGas.Nombre,$"{d.OperacionCilindro.Tubo.CapacidadTubo.Valor} {d.OperacionCilindro.Tubo.CapacidadTubo.Unidad}")).ToList()));
        }
        return resultado.OrderByDescending(x => x.FechaUtc).ToList();
    }

    public async Task<DevolucionLoteDto> CrearDevolucionLoteAsync(DevolucionLoteCrearDto dto,string usuario,CancellationToken ct)
    {
        var pendiente=await db.DevolucionesLote.AsNoTracking().FirstOrDefaultAsync(x=>x.RemitoEntregaId==dto.RemitoEntregaId&&x.Estado=="Pendiente de firma",ct);if(pendiente is not null)return (await ObtenerDevolucionLoteAsync(pendiente.Id,ct))!;
        var ids=dto.OperacionIds.Distinct().ToList();if(ids.Count==0)throw new InvalidOperationException("Seleccioná al menos un tubo para devolver.");
        if(!await db.Ubicaciones.AnyAsync(x=>x.Id==dto.UbicacionDestinoId,ct))throw new InvalidOperationException("Seleccioná una ubicación de ingreso válida.");
        var operaciones=await db.OperacionesCilindro.Include(x=>x.Cliente).Include(x=>x.Tubo).ThenInclude(x=>x.TipoGas).Include(x=>x.Tubo).ThenInclude(x=>x.CapacidadTubo).Include(x=>x.RemitoEntrega).Where(x=>ids.Contains(x.Id)).ToListAsync(ct);
        if(operaciones.Count!=ids.Count||operaciones.Any(x=>x.FechaDevolucionUtc!=null))throw new InvalidOperationException("Uno de los tubos ya no está pendiente de devolución.");
        if(operaciones.Any(x=>x.RemitoEntregaId!=dto.RemitoEntregaId))throw new InvalidOperationException("Todos los tubos deben pertenecer al mismo remito de entrega.");
        var usados=await db.DevolucionesLoteDetalle.AnyAsync(x=>ids.Contains(x.OperacionCilindroId),ct);if(usados)throw new InvalidOperationException("Uno de los tubos ya está incluido en otro remito de devolución.");
        var numeros=await db.DevolucionesLote.Select(x=>x.Numero).ToListAsync(ct);var mayor=numeros.Select(x=>x.StartsWith("DEV-")&&long.TryParse(x[4..],out var n)?n:0).DefaultIfEmpty(0).Max();
        var lote=new DevolucionLote{Numero=$"DEV-{mayor+1:000000}",RemitoEntregaId=dto.RemitoEntregaId,ClienteId=operaciones[0].ClienteId,UsuarioReceptor=usuario,Observaciones=Limpiar(dto.Observaciones)};
        foreach(var op in operaciones)lote.Detalles.Add(new(){OperacionCilindroId=op.Id,EstadoFisico=dto.EstadoFisico,EstadoContenido=dto.EstadoContenido,UbicacionDestinoId=dto.UbicacionDestinoId,RequiereInspeccion=dto.RequiereInspeccion});
        db.DevolucionesLote.Add(lote);await db.SaveChangesAsync(ct);return (await ObtenerDevolucionLoteAsync(lote.Id,ct))!;
    }

    public async Task<DevolucionLoteDto?> ObtenerDevolucionLoteAsync(long id,CancellationToken ct)
    {
        var x=await db.DevolucionesLote.AsNoTracking().Include(x=>x.Cliente).Include(x=>x.RemitoEntrega).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.TipoGas).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.CapacidadTubo).FirstOrDefaultAsync(x=>x.Id==id,ct);
        return x is null?null:MapearDevolucion(x);
    }

    public async Task<DevolucionLoteDto> FirmarDevolucionLoteAsync(long id,DevolucionLoteFirmarDto dto,string usuario,CancellationToken ct)
    {
        var lote=await db.DevolucionesLote.Include(x=>x.Cliente).Include(x=>x.RemitoEntrega).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.TipoGas).Include(x=>x.Detalles).ThenInclude(x=>x.OperacionCilindro).ThenInclude(x=>x.Tubo).ThenInclude(x=>x.CapacidadTubo).FirstOrDefaultAsync(x=>x.Id==id,ct)??throw new InvalidOperationException("No se encontró el remito de devolución.");
        if(lote.Estado!="Pendiente de firma")throw new InvalidOperationException("El remito de devolución ya fue confirmado.");
        if(!dto.FirmaDataUrl.StartsWith("data:image/png;base64,",StringComparison.Ordinal)||dto.FirmaDataUrl.Length<100)throw new InvalidOperationException("La firma no es válida.");
        foreach(var d in lote.Detalles){var op=d.OperacionCilindro;if(op.FechaDevolucionUtc.HasValue)throw new InvalidOperationException("Uno de los tubos ya fue devuelto.");var tubo=op.Tubo;var anterior=Estado(tubo);var origen=tubo.UbicacionId;tubo.EstadoFisico=(EstadoFisico)d.EstadoFisico;tubo.EstadoContenido=(EstadoContenido)d.EstadoContenido;tubo.EstadoDisponibilidad=d.RequiereInspeccion?EstadoDisponibilidad.PendienteInspeccion:EstadoDisponibilidad.Disponible;tubo.UbicacionId=d.UbicacionDestinoId;op.FechaDevolucionUtc=DateTime.UtcNow;op.Estado="Cerrada";db.MovimientosCilindro.Add(MovimientoUsuario(tubo,"Devolución",origen,d.UbicacionDestinoId,op.ClienteId,$"{lote.Numero}. {lote.Observaciones}",usuario,anterior));}
        lote.PersonaEntrega=dto.PersonaEntrega.Trim();lote.DniEntrega=dto.DniEntrega.Trim();lote.FirmaDataUrl=dto.FirmaDataUrl;lote.FechaConfirmacionUtc=DateTime.UtcNow;lote.Estado="Confirmado";lote.UsuarioReceptor=usuario;await db.SaveChangesAsync(ct);return MapearDevolucion(lote);
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
    private static OperacionDto MapearOperacion(OperacionCilindro x) => new(x.Id, x.Numero, x.Tipo, x.ClienteId, NombreCliente(x.Cliente), x.TuboId, x.Tubo.CodigoInterno, x.FechaHoraUtc, x.Estado, x.FechaLimite, x.FechaDevolucionUtc, x.PersonaRecibe, x.Observaciones, x.RemitoDevolucion?.Numero, x.RemitoEntrega?.Numero,x.RemitoEntregaId);
    private static DevolucionLoteDto MapearDevolucion(DevolucionLote x)=>new(x.Id,x.Numero,x.RemitoEntregaId,x.RemitoEntrega.Numero,x.ClienteId,NombreCliente(x.Cliente),x.FechaCreacionUtc,x.FechaConfirmacionUtc,x.Estado,x.PersonaEntrega,x.DniEntrega,x.FirmaDataUrl,x.Observaciones,x.Detalles.Select(d=>new DevolucionLoteDetalleDto(d.OperacionCilindroId,d.OperacionCilindro.TuboId,d.OperacionCilindro.Tubo.CodigoInterno,d.OperacionCilindro.Tubo.TipoGas.Nombre,$"{d.OperacionCilindro.Tubo.CapacidadTubo.Valor} {d.OperacionCilindro.Tubo.CapacidadTubo.Unidad}")).ToList());
    private static string NombreCliente(Cliente x) => x.RazonSocial ?? $"{x.Apellido}, {x.Nombre}".Trim(' ', ',');
    private static string? Limpiar(string? x) => string.IsNullOrWhiteSpace(x) ? null : x.Trim();
}

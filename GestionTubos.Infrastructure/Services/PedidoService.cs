using GestionTubos.Application.Interfaces;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared.Pedidos;
using GestionTubos.Shared;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GestionTubos.Infrastructure.Services;
public sealed partial class PedidoService(GestionTubosDbContext db,IUsuarioActual? actor=null) : IPedidoService
{
    public async Task<IReadOnlyList<PedidoDto>> ObtenerAsync(CancellationToken ct) =>
        await db.Pedidos.AsNoTracking().OrderByDescending(x=>x.FechaHoraUtc).Select(x=>new PedidoDto(x.Id,x.Numero,x.ClienteId,x.Cliente.RazonSocial ?? ((x.Cliente.Apellido ?? "")+" "+(x.Cliente.Nombre ?? "")).Trim(),x.FechaHoraUtc,x.Estado,x.UsuarioCreador,x.Observaciones,x.Detalles.Select(d=>new PedidoDetalleDto(d.TuboId,d.Tubo.CodigoInterno,d.Tubo.TipoGas.Nombre,d.Tubo.CapacidadTubo.Valor+" "+d.Tubo.CapacidadTubo.Unidad,(int)d.Tubo.TipoPropietario,d.Tubo.ClientePropietarioId)).ToList(),x.Remito==null?null:x.Remito.Numero,x.ModalidadEntrega,x.DireccionEntregaId,x.DireccionEntregaTexto,x.FechaPrevistaEntrega,x.ContactoEntrega,x.TelefonoEntrega,x.Remito==null?null:x.Remito.PersonaRecibe,x.Remito==null?null:x.Remito.DniRecibe,x.Remito==null?null:x.Remito.FirmaDataUrl,x.Remito==null?null:x.Remito.FechaFirmaUtc,x.Remito==null?null:x.Remito.UsuarioEntrega,x.Remito==null?null:x.Remito.HuellaDocumento,x.Remito==null?0:x.Remito.Version)).ToListAsync(ct);

    public async Task<PedidoDto> CrearAsync(PedidoCrearDto dto,string usuario,CancellationToken ct)
    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var ids=dto.CilindroIds.Distinct().ToList(); if(ids.Count==0)throw new InvalidOperationException("Seleccioná al menos un cilindro.");
        var cliente=await db.Clientes.FirstOrDefaultAsync(x=>x.Id==dto.ClienteId,ct)??throw new InvalidOperationException("El cliente no existe.");
        var tubos=await db.Tubos.Where(x=>ids.Contains(x.Id)).ToListAsync(ct); if(tubos.Count!=ids.Count||tubos.Any(x=>x.EstadoDisponibilidad!=EstadoDisponibilidad.Disponible||(x.TipoPropietario==TipoPropietarioTubo.Empresa&&x.EstadoContenido!=EstadoContenido.Lleno)))throw new InvalidOperationException("Uno de los cilindros ya no está disponible o es un tubo propio que no está lleno.");
        if(tubos.Any(x=>x.TipoPropietario==TipoPropietarioTubo.Cliente&&x.ClientePropietarioId!=dto.ClienteId))throw new InvalidOperationException("Un cilindro propiedad de un cliente solo puede entregarse a ese mismo cliente.");
        DireccionCliente?direccion=null;if(dto.ModalidadEntrega=="Domicilio")direccion=await db.DireccionesCliente.FirstOrDefaultAsync(x=>x.Id==dto.DireccionEntregaId&&x.ClienteId==dto.ClienteId&&x.Activa,ct)??throw new InvalidOperationException("La dirección seleccionada no pertenece al cliente.");
        var siguiente=(await db.Pedidos.MaxAsync(x=>(long?)x.Id,ct)??0)+1;var numeroRemito=await SiguienteNumeroRemito(ct);
        var pedido=new Pedido{Numero=$"PED-{siguiente:000000}",ClienteId=dto.ClienteId,UsuarioCreador=usuario,Observaciones=Limpiar(dto.Observaciones),ModalidadEntrega=dto.ModalidadEntrega,DireccionEntregaId=direccion?.Id,DireccionEntregaTexto=direccion is null?null:$"{direccion.Calle} {direccion.Numero}, {direccion.Localidad}, {direccion.Provincia}".Replace("  "," "),FechaPrevistaEntrega=dto.FechaPrevistaEntrega,ContactoEntrega=Limpiar(dto.ContactoEntrega),TelefonoEntrega=TelefonoWhatsApp.Normalizar(dto.TelefonoEntrega)??cliente.Telefono,Estado="Preparado"};
        pedido.Remito=new Remito{Numero=numeroRemito,UsuarioEmisor=usuario,Version=1};
        foreach(var tubo in tubos){pedido.Detalles.Add(new PedidoDetalle{Tubo=tubo});tubo.EstadoDisponibilidad=EstadoDisponibilidad.Reservado;}
        db.Add(pedido);await db.SaveChangesAsync(ct);var creado=(await Foto(pedido.Id,ct))!;await Registrar(null,creado,"Creación","Alta del remito",usuario,1,ct);if(tx is not null)await tx.CommitAsync(ct);return creado;
    }

    public async Task<PedidoDto> CrearYEntregarAsync(PedidoCrearDto dto,string usuario,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(dto.PersonaRecibe)||string.IsNullOrWhiteSpace(dto.DniRecibe))throw new InvalidOperationException("Completá la aclaración y el DNI de quien recibe.");
        if(!dto.FirmaDataUrl.StartsWith("data:image/png;base64,",StringComparison.Ordinal)||dto.FirmaDataUrl.Length<100)throw new InvalidOperationException("La firma electrónica no es válida.");
        await using var transaccion=await db.Database.BeginTransactionAsync(ct);
        var creado=await CrearAsync(dto,usuario,ct);
        var entregado=await ConfirmarEntregaAsync(creado.Id,new PedidoEntregarDto{VersionEsperada=creado.Version,Modalidad="Entrega",DiasSinCargo=0,PersonaRecibe=dto.PersonaRecibe,DniRecibe=dto.DniRecibe,FirmaDataUrl=dto.FirmaDataUrl,Observaciones=dto.Observaciones},usuario,ct);
        await transaccion.CommitAsync(ct);return entregado;
    }

    public async Task<RemitoDto> EmitirRemitoAsync(long pedidoId,string usuario,CancellationToken ct)
    {
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var pedido=await db.Pedidos.Include(x=>x.Remito).FirstOrDefaultAsync(x=>x.Id==pedidoId,ct)??throw new InvalidOperationException("El pedido no existe.");
        if(pedido.Remito is not null)throw new InvalidOperationException("El pedido ya tiene un remito emitido.");
        var remito=new Remito{Numero=await SiguienteNumeroRemito(ct),Pedido=pedido,UsuarioEmisor=usuario,Version=1};pedido.Estado="Preparado";db.Add(remito);await db.SaveChangesAsync(ct);await Registrar(null,await Foto(pedido.Id,ct),"Emisión","Emisión de remito para pedido existente",usuario,1,ct);await tx.CommitAsync(ct);return new(remito.Id,remito.Numero,pedido.Id,remito.FechaEmisionUtc,remito.Estado,remito.UsuarioEmisor);
    }

    public async Task EliminarRemitoAsync(long pedidoId,CancellationToken ct,string usuario="Sistema",string motivo="Eliminación de borrador solicitada")
    {
        if(string.IsNullOrWhiteSpace(motivo)||motivo.Trim().Length<3)throw new InvalidOperationException("Indicá el motivo de la eliminación.");
        await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
        var pedido=await db.Pedidos.Include(x=>x.Remito).Include(x=>x.Detalles).ThenInclude(x=>x.Tubo).Include(x=>x.RepartoDetalle).FirstOrDefaultAsync(x=>x.Id==pedidoId,ct)??throw new InvalidOperationException("El remito no existe.");
        if(pedido.Remito is null)throw new InvalidOperationException("El registro no tiene un remito.");
        if(pedido.Estado=="Entregado"||pedido.Remito.FechaFirmaUtc.HasValue||pedido.Remito.Estado=="Firmado")throw new InvalidOperationException("Un remito entregado y firmado no puede eliminarse.");
        if(pedido.RepartoDetalle is not null||pedido.Estado=="En reparto")throw new InvalidOperationException("Quitá primero el remito del reparto antes de eliminarlo.");
        var antes=(await Foto(pedidoId,ct))!;var version=pedido.Remito.Version+1;
        foreach(var detalle in pedido.Detalles)
        {
            if(detalle.Tubo.EstadoDisponibilidad!=EstadoDisponibilidad.Reservado)throw new InvalidOperationException($"El tubo {detalle.Tubo.CodigoInterno} ya no está reservado y el remito no puede eliminarse.");
            detalle.Tubo.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;
        }
        db.Remitos.Remove(pedido.Remito);db.PedidoDetalles.RemoveRange(pedido.Detalles);db.Pedidos.Remove(pedido);await db.SaveChangesAsync(ct);await Registrar(antes,null,"Eliminación",motivo.Trim(),usuario,version,ct);await tx.CommitAsync(ct);
    }

    public async Task<PedidoDto> ConfirmarEntregaAsync(long pedidoId,PedidoEntregarDto dto,string usuario,CancellationToken ct)
    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var pedido=await db.Pedidos.Include(x=>x.Remito).Include(x=>x.Detalles).ThenInclude(x=>x.Tubo).FirstOrDefaultAsync(x=>x.Id==pedidoId,ct)??throw new InvalidOperationException("El pedido no existe.");
        if(pedido.Remito is null)throw new InvalidOperationException("Primero debés emitir el remito.");
        if(dto.VersionEsperada!=pedido.Remito.Version)throw new InvalidOperationException("El remito fue modificado. Actualizá la pantalla y revisá los datos antes de firmar.");
        if(pedido.Remito.FechaFirmaUtc.HasValue)throw new InvalidOperationException("El remito ya fue firmado y no puede modificarse.");
        if(!dto.FirmaDataUrl.StartsWith("data:image/png;base64,",StringComparison.Ordinal)||dto.FirmaDataUrl.Length<100)throw new InvalidOperationException("La firma electrónica no es válida.");
        if(pedido.Estado=="Entregado")throw new InvalidOperationException("El pedido ya fue entregado.");
        if(pedido.ModalidadEntrega=="RetiroPlanta"&&pedido.Estado=="En reparto")throw new InvalidOperationException("Un retiro en planta no puede formar parte de un reparto.");
        if(pedido.Estado is not ("Preparado" or "En reparto")||pedido.Detalles.Any(x=>x.Tubo.EstadoDisponibilidad is not (EstadoDisponibilidad.Reservado or EstadoDisponibilidad.EnReparto)))throw new InvalidOperationException("El pedido no está preparado para la entrega.");
        var antes=(await Foto(pedidoId,ct))!;
        if(dto.Observaciones is not null)pedido.Observaciones=Limpiar(dto.Observaciones);
        var hoy=DateOnly.FromDateTime(GestionTubos.Shared.HoraArgentina.Hoy);var ids=pedido.Detalles.Select(x=>x.TuboId).ToList();
        var pruebas=await db.PruebasHidraulicas.Where(x=>ids.Contains(x.TuboId)&&x.Aprobada).GroupBy(x=>x.TuboId).Select(g=>new{TuboId=g.Key,Vence=g.Max(x=>x.FechaVencimiento)}).ToDictionaryAsync(x=>x.TuboId,x=>x.Vence,ct);
        if(ids.Any(id=>!pruebas.TryGetValue(id,out var vence)||vence<hoy))throw new InvalidOperationException("Todos los cilindros deben tener una prueba hidráulica vigente.");
        var baseNumero=await db.OperacionesCilindro.CountAsync(ct);var numeroOperacion=0;var ubicacionCliente=await UbicacionesSistema.ObtenerEnPoderDelClienteAsync(db,ct);
        for(var i=0;i<pedido.Detalles.Count;i++)
        {
            var tubo=pedido.Detalles.ElementAt(i).Tubo;var anterior=Estado(tubo);var origen=tubo.UbicacionId;
            tubo.EstadoDisponibilidad=EstadoDisponibilidad.EnPoderCliente;
            tubo.UbicacionId=ubicacionCliente;
            if(tubo.TipoPropietario!=TipoPropietarioTubo.Cliente)
            {
                numeroOperacion++;
                db.OperacionesCilindro.Add(new OperacionCilindro{Numero=$"OP-{GestionTubos.Shared.HoraArgentina.Hoy:yyyyMMdd}-{baseNumero+numeroOperacion:D5}",Tipo=dto.Modalidad,ClienteId=pedido.ClienteId,TuboId=tubo.Id,RemitoEntregaId=pedido.Remito.Id,DiasSinCargo=dto.DiasSinCargo,FechaLimite=dto.DiasSinCargo>0?hoy.AddDays(dto.DiasSinCargo):null,PersonaRecibe=Limpiar(dto.PersonaRecibe),Observaciones=pedido.Observaciones});
            }
            db.MovimientosCilindro.Add(new MovimientoCilindro{TuboId=tubo.Id,Tipo="Entrega de pedido",UbicacionOrigenId=origen,UbicacionDestinoId=ubicacionCliente,ClienteId=pedido.ClienteId,EstadoAnterior=anterior,EstadoPosterior=Estado(tubo),UsuarioResponsable=usuario,Observaciones=$"{pedido.Numero} / {pedido.Remito.Numero}. {pedido.Observaciones}".Trim()});
        }
        pedido.Remito.Version++;var fechaFirma=DateTime.UtcNow;pedido.Estado="Entregado";pedido.Remito.Estado="Firmado";pedido.Remito.PersonaRecibe=dto.PersonaRecibe.Trim();pedido.Remito.DniRecibe=dto.DniRecibe.Trim();pedido.Remito.FirmaDataUrl=dto.FirmaDataUrl;pedido.Remito.FechaFirmaUtc=fechaFirma;pedido.Remito.UsuarioEntrega=usuario;pedido.Remito.HuellaDocumento=CrearHuella(pedido,dto,fechaFirma);
        var detalleReparto=await db.RepartoDetalles.Include(x=>x.Reparto).FirstOrDefaultAsync(x=>x.PedidoId==pedido.Id,ct);if(detalleReparto is not null){detalleReparto.Estado="Entregado";var otros=await db.RepartoDetalles.Where(x=>x.RepartoId==detalleReparto.RepartoId&&x.Id!=detalleReparto.Id).Select(x=>x.Estado).ToListAsync(ct);if(otros.All(x=>x=="Entregado"))detalleReparto.Reparto.Estado=detalleReparto.Reparto.Observaciones!=null&&detalleReparto.Reparto.Observaciones.Contains("NO ENTREGADO")?"Completado con novedades":"Completado";}
        await db.SaveChangesAsync(ct);var firmado=(await Foto(pedido.Id,ct))!;await Registrar(antes,firmado,"Firma y entrega","Entrega confirmada con firma del cliente",usuario,pedido.Remito.Version,ct);if(tx is not null)await tx.CommitAsync(ct);return firmado;
    }
    private static string Estado(Tubo x)=>$"Físico={(int)x.EstadoFisico};Contenido={(int)x.EstadoContenido};Disponibilidad={(int)x.EstadoDisponibilidad};Ubicación={x.UbicacionId}";
    private async Task<string> SiguienteNumeroRemito(CancellationToken ct)
    {
        var numeros=await db.Remitos.AsNoTracking().Select(x=>x.Numero).Concat(db.AuditoriaRemitos.Select(x=>x.NumeroRemito)).ToListAsync(ct);
        var mayor=numeros.Select(x=>x.StartsWith("REM-")&&long.TryParse(x[4..],out var n)?n:0).DefaultIfEmpty(0).Max();
        return $"REM-{mayor+1:000000}";
    }
    private static string CrearHuella(Pedido pedido,PedidoEntregarDto dto,DateTime fechaFirma){var contenido=$"{pedido.Remito!.Numero}|{pedido.Numero}|{pedido.ClienteId}|{string.Join(',',pedido.Detalles.OrderBy(x=>x.TuboId).Select(x=>x.TuboId))}|{dto.PersonaRecibe.Trim()}|{dto.DniRecibe.Trim()}|{fechaFirma:O}|{dto.FirmaDataUrl}|{pedido.Observaciones}";return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(contenido)));}
    private static string? Limpiar(string? x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim();
}

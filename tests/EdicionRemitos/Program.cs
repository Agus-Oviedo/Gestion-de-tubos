using GestionTubos.Application.Interfaces;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using GestionTubos.Shared.Pedidos;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Reflection;
var root=args[0];var cfg=new ConfigurationBuilder().AddJsonFile(Path.Combine(root,"GestionTubos.Api","appsettings.json")).Build();
var cs=new SqlConnectionStringBuilder(cfg.GetConnectionString("GestionTubos")){InitialCatalog="GestionTubos_PruebaAuditoria_"+Guid.NewGuid().ToString("N")};
var options=new DbContextOptionsBuilder<GestionTubosDbContext>().UseSqlServer(cs.ConnectionString).Options;
GestionTubosDbContext Db()=>new(options);
await using var inicial=Db();int checks=0;void Check(bool ok,string m){if(!ok)throw new Exception(m);checks++;}
async Task<T> Accion<T>(Func<PedidoService,Task<T>> f){await using var d=Db();return await f(new(d,new Actor()));}
async Task Rechaza(Func<Task> f,string m){try{await f();}catch(InvalidOperationException){checks++;return;}throw new Exception("Aceptó "+m);}
RemitoEditarDto Editar(PedidoDto p)=>new(){VersionEsperada=p.Version,ClienteId=p.ClienteId,CilindroIds=p.Cilindros.Select(t=>t.CilindroId).ToList(),ModalidadEntrega=p.ModalidadEntrega,DireccionEntregaId=p.DireccionEntregaId,FechaPrevistaEntrega=p.FechaPrevistaEntrega,ContactoEntrega=p.ContactoEntrega,TelefonoEntrega=p.TelefonoEntrega,Observaciones=p.Observaciones,Motivo="Corregir datos del remito"};
try{
 await inicial.Database.MigrateAsync();
 var cli=new Cliente{Apellido="Prueba",Nombre="Uno",Tipo=(TipoCliente)1};var otro=new Cliente{Apellido="Prueba",Nombre="Dos",Tipo=(TipoCliente)1};var gas=new TipoGas{Nombre="Gas Auditoría"};var cap=new CapacidadTubo{Valor=99,Unidad="m3"};var planta=new Ubicacion{Nombre="Planta Auditoría",EsInterna=true};inicial.AddRange(cli,otro,gas,cap,planta);await inicial.SaveChangesAsync();
 var ts=Enumerable.Range(1,5).Select(i=>new Tubo{CodigoInterno="AUD-"+i,TipoGasId=gas.Id,CapacidadTuboId=cap.Id,UbicacionId=planta.Id,TipoPropietario=TipoPropietarioTubo.Empresa,EstadoContenido=EstadoContenido.Lleno,EstadoDisponibilidad=EstadoDisponibilidad.Disponible}).ToList();inicial.AddRange(ts);await inicial.SaveChangesAsync();
 await inicial.Database.MigrateAsync();Check(await inicial.Tubos.CountAsync()==5&&await inicial.Clientes.CountAsync()==2,"Migración conserva tubos y clientes");
 foreach(var t in ts)inicial.PruebasHidraulicas.Add(new(){TuboId=t.Id,Aprobada=true,FechaRealizacion=DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),FechaVencimiento=DateOnly.FromDateTime(DateTime.Today.AddYears(1)),EntidadRealizadora="Prueba"});await inicial.SaveChangesAsync();
 var p=await Accion(s=>s.CrearAsync(new(){ClienteId=cli.Id,CilindroIds=[ts[0].Id,ts[1].Id],ModalidadEntrega="RetiroPlanta",Observaciones="Pendiente de pago"},"Administrador Prueba",default));
 Check(p.Version==1,"Creación inicia versión");var hist=await Accion(s=>s.AuditoriaAsync(p.Id,null,null,default));Check(hist.Registros.Count==1&&hist.Registros[0].UsuarioId=="usuario-prueba"&&hist.Registros[0].Usuario=="Administrador Prueba","Creación registra identidad real");
 var editar=Editar(p);editar.ClienteId=otro.Id;editar.CilindroIds=[ts[1].Id,ts[2].Id];editar.Observaciones="Pagó en efectivo.\nSaldo cero.";editar.ContactoEntrega="Contacto corregido";editar.FechaPrevistaEntrega=DateOnly.FromDateTime(DateTime.Today.AddDays(1));
 p=await Accion(s=>s.EditarAsync(p.Id,editar,"Operador Prueba",default));Check(p.Version==2&&p.NumeroRemito=="REM-000001"&&p.ClienteId==otro.Id,"Edición conserva número y cambia cliente");
 await using(var d=Db()){Check((await d.Tubos.SingleAsync(x=>x.Id==ts[0].Id)).EstadoDisponibilidad==EstadoDisponibilidad.Disponible&&(await d.Tubos.SingleAsync(x=>x.Id==ts[2].Id)).EstadoDisponibilidad==EstadoDisponibilidad.Reservado,"Edición libera y reserva tubos");Check((await d.Tubos.SingleAsync(x=>x.Id==ts[4].Id)).EstadoDisponibilidad==EstadoDisponibilidad.Disponible,"No altera otros tubos");}
 hist=await Accion(s=>s.AuditoriaAsync(p.Id,null,null,default));Check(hist.Registros.Count==2&&hist.Registros[0].Cambios.Any(x=>x.Campo=="Descripción / observaciones"&&x.Antes=="Pendiente de pago"&&x.Despues==editar.Observaciones)&&hist.Registros[0].Cambios.Any(x=>x.Campo=="Tubos")&&hist.Registros[0].Usuario=="Operador Prueba","Auditoría conserva antes, después y responsable");
 await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,editar,"Otra persona",default));},"versión obsoleta");
 var sinMotivo=Editar(p);sinMotivo.Motivo=" ";await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,sinMotivo,"Prueba",default));},"motivo vacío");
 var sinCambios=Editar(p);await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,sinCambios,"Prueba",default));},"edición sin cambios");
 var invalido=Editar(p);invalido.CilindroIds=[-1];await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,invalido,"Prueba",default));},"tubo inexistente");
 var direccion=Editar(p);direccion.ModalidadEntrega="Domicilio";direccion.DireccionEntregaId=-1;await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,direccion,"Prueba",default));},"dirección inválida");
 Check((await Accion(s=>s.AuditoriaAsync(p.Id,null,null,default))).Registros.Count==2,"Cambios rechazados no generan auditoría ni reservas");
 await Rechaza(async()=>{await Accion(s=>s.ConfirmarEntregaAsync(p.Id,new(){VersionEsperada=1,PersonaRecibe="Prueba",DniRecibe="123",FirmaDataUrl="data:image/png;base64,"+new string('A',100)},"Prueba",default));},"firmar una versión desactualizada");
 p=await Accion(s=>s.ConfirmarEntregaAsync(p.Id,new(){VersionEsperada=p.Version,Modalidad="Entrega",PersonaRecibe="Prueba",DniRecibe="123",FirmaDataUrl="data:image/png;base64,"+new string('A',100)},"Responsable Entrega",default));
 Check(p.Version==3&&p.FechaFirmaUtc.HasValue,"Firma incrementa versión");hist=await Accion(s=>s.AuditoriaAsync(p.Id,null,null,default));Check(hist.Registros[0].Accion=="Firma y entrega"&&hist.Registros[0].Cambios.Any(x=>x.Campo=="Firma"),"Firma queda auditada");
 var firmado=Editar(p);firmado.Observaciones="Alterar firmado";await Rechaza(async()=>{await Accion(s=>s.EditarAsync(p.Id,firmado,"Prueba",default));},"editar firmado");
 await Rechaza(async()=>{await using var d=Db();await new PedidoService(d,new Actor()).EliminarRemitoAsync(p.Id,default,"Prueba","Intento firmado");},"eliminar firmado");
 await Rechaza(async()=>{await Accion(s=>s.ConfirmarEntregaAsync(p.Id,new(),"Prueba",default));},"volver a firmar");
 var borrador=await Accion(s=>s.CrearAsync(new(){ClienteId=cli.Id,CilindroIds=[ts[3].Id],ModalidadEntrega="RetiroPlanta"},"Prueba",default));
 await using(var d=Db())await new PedidoService(d,new Actor()).EliminarRemitoAsync(borrador.Id,default,"Administrador Prueba","Borrador duplicado");
 hist=await Accion(s=>s.AuditoriaAsync(borrador.Id,null,null,default));Check(hist.Registros.Count==2&&hist.Registros[0].Accion=="Eliminación"&&hist.Registros[0].Motivo=="Borrador duplicado","Auditoría sobrevive al borrado de pendientes");
 await Rechaza(async()=>{await using var d=Db();var r=await d.AuditoriaRemitos.FirstAsync();r.Motivo="Alterado";await d.SaveChangesAsync();},"cambiar auditoría");
 await Rechaza(async()=>{await using var d=Db();d.Remove(await d.AuditoriaRemitos.FirstAsync());await d.SaveChangesAsync();},"borrar auditoría");
 // El endpoint no ofrece edición para usuarios sin rol de administrador.
 var controller=typeof(GestionTubos.Api.Controllers.PedidosController);var roles=controller.GetMethod("Editar")!.GetCustomAttributes<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles;
 Check(roles=="Superadministrador,Administrador","API restringida a administradores");
 await Rechaza(async()=>{await using var d=Db();var firmadoDb=await d.Pedidos.SingleAsync(x=>x.Id==p.Id);firmadoDb.Observaciones="Cambio fuera del editor";await d.SaveChangesAsync();},"modificar firmado fuera del servicio");
 await Rechaza(async()=>{await using var d=Db();var firmadoDb=await d.Remitos.SingleAsync(x=>x.PedidoId==p.Id);firmadoDb.Estado="Anulado";await d.SaveChangesAsync();},"anular firmado desde otra operación");
 int direccionId;
 await using(var d=Db()){var dir=new DireccionCliente{ClienteId=cli.Id,Calle="Prueba",Numero="1",Localidad="Santa Rosa",Provincia="La Pampa"};d.Add(dir);await d.SaveChangesAsync();direccionId=dir.Id;}
 var aDomicilio=await Accion(s=>s.CrearAsync(new(){ClienteId=cli.Id,CilindroIds=[ts[3].Id],ModalidadEntrega="Domicilio",DireccionEntregaId=direccionId},"Prueba",default));
 long repartoId;
 await using(var d=Db()){var r=await new RepartoServiceAuditado(d,new Actor()).CrearAsync(new(){PedidoIds=[aDomicilio.Id],Responsable="Repartidor"},"Encargado",default);repartoId=r.Id;}
 await using(var d=Db())await new RepartoServiceAuditado(d,new Actor()).IniciarAsync(repartoId,"Repartidor",default);
 await using(var d=Db())await new RepartoServiceAuditado(d,new Actor()).NoEntregadoAsync(repartoId,aDomicilio.Id,new(){Accion="CancelarPedido",Motivo="Cliente no recibe",UbicacionRetornoId=planta.Id},"Repartidor",default);
 hist=await Accion(s=>s.AuditoriaAsync(aDomicilio.Id,null,null,default));Check(hist.Registros.Count==4&&hist.Registros.Any(x=>x.Accion=="Salida a reparto")&&hist.Registros[0].Accion=="Anulación sin entrega"&&hist.Registros.All(x=>x.UsuarioId=="usuario-prueba"),"Repartos también auditados");
 await Rechaza(async()=>{await using var d=Db();await new RepartoServiceAuditado(d,new Actor()).CrearAsync(new(){PedidoIds=[p.Id],Responsable="Prueba"},"Prueba",default);},"cambiar reparto de firmado");
 // Estado inicial de un remito previo, sin atribuir cambios que el sistema no registró.
 await using(var d=Db()){
 var legado=new Pedido{Numero="PED-LEGADO",ClienteId=cli.Id,Estado="Preparado",UsuarioCreador="Anterior",Remito=new Remito{Numero="REM-LEGADO",UsuarioEmisor="Anterior"}};d.Add(legado);await d.SaveChangesAsync();var s=new PedidoService(d,new Actor());await s.RegistrarEstadosInicialesAsync(default);await s.RegistrarEstadosInicialesAsync(default);Check(await d.AuditoriaRemitos.CountAsync(x=>x.PedidoId==legado.Id)==1,"Estado inicial idempotente");}
 Console.WriteLine(checks+" verificaciones de edición, auditoría, migración y bloqueo de firmados correctas.");
}finally{if(!cs.InitialCatalog.StartsWith("GestionTubos_PruebaAuditoria_"))throw new Exception("Base inesperada");await inicial.Database.EnsureDeletedAsync();}
sealed class Actor:IUsuarioActual{public string Id=>"usuario-prueba";}

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Infrastructure.Services;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Shared;
using GestionTubos.Shared.Operaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
var cfg=new ConfigurationBuilder().AddJsonFile(Path.Combine(args[0],"GestionTubos.Api","appsettings.json")).Build();
var cs=new SqlConnectionStringBuilder(cfg.GetConnectionString("GestionTubos"));cs.InitialCatalog="GestionTubos_PruebaCarga_"+Guid.NewGuid().ToString("N");
var opciones=new DbContextOptionsBuilder<GestionTubosDbContext>().UseSqlServer(cs.ConnectionString).Options;
GestionTubosDbContext Db()=>new(opciones);
async Task<EnvioCargaDto> Accion(Func<CargaLoteService,Task<EnvioCargaDto>> f){await using var d=Db();return await f(new(d));}
int casos=0;void Check(bool ok,string msg){if(!ok)throw new Exception(msg);casos++;}
async Task Rechaza(Func<Task> f,string msg){try{await f();}catch(InvalidOperationException){casos++;return;}throw new Exception("Aceptó "+msg);}
// Regresión del formulario: la selección debe copiarse antes de DataAnnotations.
var pantalla=new GestionTubos.Client.Pages.StockVacios();
const BindingFlags privados=BindingFlags.Instance|BindingFlags.NonPublic;
T Campo<T>(string nombre)=>(T)pantalla.GetType().GetField(nombre,privados)!.GetValue(pantalla)!;
bool ValidarFormulario(string nombre,EditContext contexto)=>(bool)pantalla.GetType().GetMethod(nombre,privados)!.Invoke(pantalla,[contexto])!;
using var proveedorFormulario=new ServiceCollection().BuildServiceProvider();
var nuevoFormulario=Campo<EnvioCargaCrearDto>("nuevo");nuevoFormulario.Transporte="Oxígeno Norte";
var contextoEnvio=new EditContext(nuevoFormulario);using var validacionEnvio=contextoEnvio.EnableDataAnnotationsValidation(proveedorFormulario);
Campo<HashSet<int>>("seleccion").UnionWith([1,2,3,4]);
Check(ValidarFormulario("ValidarEnvio",contextoEnvio)&&nuevoFormulario.TuboIds.Count==4,"Cuatro seleccionados validan antes del envío");
Campo<HashSet<int>>("seleccion").Clear();
Check(!ValidarFormulario("ValidarEnvio",contextoEnvio)&&nuevoFormulario.TuboIds.Count==0,"Desmarcar elimina selección anterior");
Campo<HashSet<int>>("seleccion").Add(2);nuevoFormulario.Transporte="";
Check(!ValidarFormulario("ValidarEnvio",contextoEnvio),"Transporte obligatorio sigue validado");
nuevoFormulario.Transporte="Oxígeno Norte";
Check(ValidarFormulario("ValidarEnvio",contextoEnvio)&&nuevoFormulario.TuboIds.SequenceEqual([2]),"Selección individual valida");
var formularioRecepcion=Campo<RecepcionCargaCrearDto>("recepcion");formularioRecepcion.UbicacionDestinoId=1;
var contextoRecepcion=new EditContext(formularioRecepcion);using var validacionRecepcion=contextoRecepcion.EnableDataAnnotationsValidation(proveedorFormulario);
Campo<Dictionary<long,string>>("retornos").Add(15,"Vacio");
Check(ValidarFormulario("ValidarRecepcion",contextoRecepcion)&&formularioRecepcion.Tubos.Single().Resultado=="Vacio","Recepción copia selección y condición antes de validar");
Campo<Dictionary<long,string>>("retornos").Clear();
Check(!ValidarFormulario("ValidarRecepcion",contextoRecepcion),"Recepción vacía sigue rechazada");
// Búsqueda y selección a través de varias páginas.
var vaciosFormulario=Enumerable.Range(1,60).Select(i=>new VacioCargaDto(i,"TUB-"+i,i%2==0?"Oxígeno":"Argon","10 m³","Planta",true,"Apto")).ToList();
pantalla.GetType().GetField("vacios",privados)!.SetValue(pantalla,vaciosFormulario);
Campo<HashSet<int>>("seleccion").Clear();
pantalla.GetType().GetProperty("BuscarVacio",privados)!.SetValue(pantalla,"Argon");
Check(Campo<HashSet<int>>("seleccion").Count==0,"Buscar no selecciona automáticamente");
Check(((IEnumerable<VacioCargaDto>)pantalla.GetType().GetProperty("VaciosPagina",privados)!.GetValue(pantalla)!).Count()==25,"Página de 25 tubos filtrados");
pantalla.GetType().GetMethod("SeleccionarTodos",privados)!.Invoke(pantalla,null);
Check(Campo<HashSet<int>>("seleccion").Count==30&&Campo<HashSet<int>>("seleccion").All(x=>x%2!=0),"Seleccionar todos respeta búsqueda y abarca todas las páginas");
pantalla.GetType().GetField("paginaVacios",privados)!.SetValue(pantalla,2);
Check(((IEnumerable<VacioCargaDto>)pantalla.GetType().GetProperty("VaciosPagina",privados)!.GetValue(pantalla)!).Count()==5&&Campo<HashSet<int>>("seleccion").Count==30,"Cambio de página conserva selección");
pantalla.GetType().GetProperty("BuscarVacio",privados)!.SetValue(pantalla,"Oxígeno");
Check(Campo<int>("paginaVacios")==1&&Campo<HashSet<int>>("seleccion").Count==30,"Cambiar búsqueda reinicia página y conserva selección");
pantalla.GetType().GetMethod("SeleccionarTodos",privados)!.Invoke(pantalla,null);
Check(Campo<HashSet<int>>("seleccion").Count==60,"Selecciones de distintas búsquedas se acumulan");
pantalla.GetType().GetMethod("SeleccionarTodos",privados)!.Invoke(pantalla,null);
Check(Campo<HashSet<int>>("seleccion").Count==30&&Campo<HashSet<int>>("seleccion").All(x=>x%2!=0),"Desmarcar búsqueda conserva otros tubos");
Check(ValidarFormulario("ValidarEnvio",contextoEnvio),"Selección lista para vista previa");
await (Task)pantalla.GetType().GetMethod("Crear",privados)!.Invoke(pantalla,null)!;
Check(Campo<bool>("mostrarVista")&&Campo<EnvioCargaCrearDto>("modeloVista").TuboIds.Count==30,"Preparar muestra vista previa con selección");
Campo<HashSet<int>>("seleccion").Clear();
Check(Campo<EnvioCargaCrearDto>("modeloVista").TuboIds.Count==30,"Vista previa conserva copia de la selección revisada");
pantalla.GetType().GetMethod("CerrarVista",privados)!.Invoke(pantalla,null);
Check(!Campo<bool>("mostrarVista"),"Volver a editar cierra vista sin confirmar");
await using var basePrueba=Db();
try{
 await basePrueba.Database.EnsureCreatedAsync();
 var gas=new TipoGas{Nombre="Prueba"};var cap=new CapacidadTubo{Valor=10,Unidad="m3"};var planta=new Ubicacion{Nombre="Planta",EsInterna=true};basePrueba.AddRange(gas,cap,planta);await basePrueba.SaveChangesAsync();
 // Alta propia siempre llena; otros propietarios respetan la elección.
 await using(var d=Db()){
  var servicio=new TuboService(d);
  GestionTubos.Shared.Tubos.TuboGuardarDto Alta(string codigo,int propietario,int contenido)=>new(){CodigoInterno=codigo,TipoGasId=gas.Id,CapacidadTuboId=cap.Id,UbicacionId=planta.Id,TipoPropietario=propietario,NombrePropietarioExterno="Tercero",EstadoContenido=contenido,EstadoDisponibilidad=1};
  var propio=await servicio.CrearAsync(Alta("ALTA-PROPIA",1,4),default);Check(propio.EstadoContenido==1,"Alta propia convierte desconocido a lleno");
  var vacio=await servicio.CrearAsync(Alta("ALTA-TERCERO-VACIO",4,2),default);Check(vacio.EstadoContenido==2,"Alta tercero vacío conserva vacío");
  var lleno=await servicio.CrearAsync(Alta("ALTA-TERCERO-LLENO",4,1),default);Check(lleno.EstadoContenido==1,"Alta tercero lleno conserva lleno");
  var editar=Alta("ALTA-PROPIA",1,2);await servicio.ActualizarAsync(propio.Id,editar,default);Check((await servicio.ObtenerAsync(propio.Id,default))!.EstadoContenido==2,"Editar propio conserva vacío real");
  await d.MovimientosCilindro.Where(x=>x.TuboId==propio.Id||x.TuboId==vacio.Id||x.TuboId==lleno.Id).ExecuteDeleteAsync();
  await d.Tubos.Where(x=>x.Id==propio.Id||x.Id==vacio.Id||x.Id==lleno.Id).ExecuteDeleteAsync();
 }
 var tubos=Enumerable.Range(1,5).Select(i=>new Tubo{CodigoInterno="PRUEBA-"+i,TipoGasId=gas.Id,CapacidadTuboId=cap.Id,UbicacionId=planta.Id,TipoPropietario=TipoPropietarioTubo.Empresa,EstadoContenido=EstadoContenido.Vacio,EstadoDisponibilidad=EstadoDisponibilidad.Disponible}).ToList();basePrueba.AddRange(tubos);await basePrueba.SaveChangesAsync();foreach(var t in tubos)basePrueba.PruebasHidraulicas.Add(new(){TuboId=t.Id,FechaRealizacion=DateOnly.FromDateTime(HoraArgentina.Hoy),FechaVencimiento=DateOnly.FromDateTime(HoraArgentina.Hoy.AddYears(1)),Aprobada=true,EntidadRealizadora="Prueba"});await basePrueba.SaveChangesAsync();
 var e=await Accion(s=>s.Crear(new(){Destino="Buenos Aires",Transporte="Prueba",TuboIds=tubos.Take(4).Select(x=>x.Id).ToList()},"Prueba",default));
 await using(var d=Db()){Check(await d.Tubos.CountAsync(x=>x.EstadoDisponibilidad==EstadoDisponibilidad.PreparadoParaCarga&&x.UbicacionId==planta.Id)==4,"Reserva conserva planta");}
 await Rechaza(async()=>{await Accion(s=>s.Crear(new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[0].Id]},"Prueba",default));},"doble reserva");
 var cancelado=await Accion(s=>s.Crear(new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[4].Id]},"Prueba",default));cancelado=await Accion(s=>s.Cancelar(cancelado.Id,default));Check(cancelado.Estado=="Cancelado","Cancelar borrador");
 var numeroPendiente=e.Numero;
 e=await Accion(s=>s.ActualizarPendiente(e.Id,new(){Destino="BA corregido",Transporte="Prueba",TuboIds=[tubos[0].Id,tubos[1].Id,tubos[2].Id,tubos[4].Id]},default));
 Check(e.Numero==numeroPendiente&&e.Estado=="Preparado"&&e.Tubos.Any(t=>t.TuboId==tubos[4].Id),"Corregir selección conserva remito pendiente");
 await using(var d=Db()){Check((await d.Tubos.SingleAsync(t=>t.Id==tubos[3].Id)).EstadoDisponibilidad==EstadoDisponibilidad.Disponible&&(await d.Tubos.SingleAsync(t=>t.Id==tubos[4].Id)).EstadoDisponibilidad==EstadoDisponibilidad.PreparadoParaCarga,"Quitar libera y agregar reserva sin salida");}
 await Rechaza(async()=>{await Accion(s=>s.ActualizarPendiente(e.Id,new(){Destino="BA",Transporte="Prueba",TuboIds=[-1]},default));},"corrección con tubo inexistente");
 e=await Accion(s=>s.ActualizarPendiente(e.Id,new(){Destino="Buenos Aires",Transporte="Prueba",TuboIds=tubos.Take(4).Select(t=>t.Id).ToList()},default));
 e=await Accion(s=>s.Salida(e.Id,"Prueba",default));Check(e.Numero==numeroPendiente,"Confirmación mantiene el número del pendiente");
 await Rechaza(async()=>{await Accion(s=>s.ActualizarPendiente(e.Id,new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[0].Id]},default));},"edición después de confirmar salida");
 Check(e.Estado=="Enviado"&&e.FechaSalidaUtc.HasValue,"Salida confirmada");
 await using(var d=Db()){Check(await d.Tubos.CountAsync(x=>x.EstadoDisponibilidad==EstadoDisponibilidad.EnviadoACarga&&x.UbicacionId!=planta.Id)==4,"Salida fuera de planta");}
 await Rechaza(async()=>{await Accion(s=>s.Salida(e.Id,"Prueba",default));},"doble salida");
 e=await Accion(s=>s.Recibir(e.Id,new(){UbicacionDestinoId=planta.Id,Tubos=[new(){DetalleId=e.Tubos[0].Id,Resultado="Lleno"}]},"Prueba",default));Check(e.Estado=="Parcial"&&e.Tubos.Count(x=>x.Estado=="Pendiente")==3&&e.Recepciones.Count==1,"Recepción parcial");
 await Rechaza(async()=>{await Accion(s=>s.Recibir(e.Id,new(){UbicacionDestinoId=planta.Id,Tubos=[new(){DetalleId=e.Tubos[0].Id}]},"Prueba",default));},"recepción repetida");
 e=await Accion(s=>s.Recibir(e.Id,new(){UbicacionDestinoId=planta.Id,Tubos=[new(){DetalleId=e.Tubos[1].Id,Resultado="Vacio"},new(){DetalleId=e.Tubos[2].Id,Resultado="Danado"}]},"Prueba",default));Check(e.Recepciones.Count==2&&e.Tubos.Count(x=>x.Estado=="Pendiente")==1,"Segunda recepción");
 await using(var d=Db()){var ts=await d.Tubos.OrderBy(x=>x.Id).ToListAsync();Check(ts[0].EstadoContenido==EstadoContenido.Lleno&&ts[0].EstadoDisponibilidad==EstadoDisponibilidad.Disponible,"Lleno disponible");Check(ts[1].EstadoContenido==EstadoContenido.Vacio&&ts[1].EstadoDisponibilidad==EstadoDisponibilidad.Disponible,"Rechazado vuelve a vacíos");Check(ts[2].EstadoFisico==EstadoFisico.Danado&&ts[2].EstadoDisponibilidad==EstadoDisponibilidad.EnMantenimiento,"Dañado a mantenimiento");}
 e=await Accion(s=>s.Resolver(e.Id,new(){DetalleId=e.Tubos[3].Id,Motivo="Faltante definitivo de prueba"},"Prueba",default));Check(e.Estado=="Cerrado con faltantes","Cierre con faltante");
 await Rechaza(async()=>{await Accion(s=>s.Recibir(e.Id,new(){UbicacionDestinoId=planta.Id,Tubos=[new(){DetalleId=e.Tubos[3].Id}]},"Prueba",default));},"recepción después de cierre");
 await using(var d=Db()){
  var cliente=new Cliente{Nombre="Prueba",Apellido="Devolución",Tipo=(TipoCliente)1};d.Clientes.Add(cliente);await d.SaveChangesAsync();
  var tubo=await d.Tubos.SingleAsync(x=>x.Id==tubos[4].Id);tubo.EstadoContenido=EstadoContenido.Lleno;tubo.EstadoDisponibilidad=EstadoDisponibilidad.EnPoderCliente;
  var op=new OperacionCilindro{Numero="OP-PRUEBA-DEV",TuboId=tubo.Id,ClienteId=cliente.Id,Tipo="Alquiler"};d.OperacionesCilindro.Add(op);await d.SaveChangesAsync();
  await new OperacionService(d).RecibirAsync(new(){OperacionId=op.Id,UbicacionDestinoId=planta.Id,EstadoContenido=1,EstadoFisico=1},"Prueba",default);
  Check(tubo.EstadoContenido==EstadoContenido.Vacio,"Devolución propia siempre vacía aunque soliciten lleno");
 }
 var otra=await Accion(s=>s.Crear(new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[4].Id]},"Prueba",default));
 await Rechaza(async()=>{await using var d=Db();await new TuboService(d).EliminarAsync(tubos[4].Id,default);},"baja de tubo reservado");
 otra=await Accion(s=>s.Salida(otra.Id,"Prueba",default));
 await Rechaza(async()=>{await using var d=Db();await new TuboService(d).EliminarAsync(tubos[4].Id,default);},"baja de tubo enviado");
 otra=await Accion(s=>s.Recibir(otra.Id,new(){UbicacionDestinoId=planta.Id,Tubos=[new(){DetalleId=otra.Tubos[0].Id,Resultado="Lleno"}]},"Prueba",default));Check(otra.Estado=="Completo","Cierre completo");
 await using(var d=Db()){var t=await d.Tubos.SingleAsync(x=>x.Id==tubos[4].Id);t.CodigoInterno="CODIGO-EDITADO";await d.SaveChangesAsync();}
 var historico=await Accion(async s=>(await s.Obtener(otra.Id,default))!);Check(historico.Tubos[0].Codigo=="PRUEBA-5"&&historico.Recepciones[0].Tubos[0].Codigo=="PRUEBA-5","Datos originales del remito inmutables");
 var directo=await Accion(s=>s.Crear(new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[1].Id]},"Prueba",default,true));
 Check(directo.Estado=="Enviado"&&directo.FechaSalidaUtc.HasValue,"Confirmación desde vista previa guarda envío y salida juntos");
 await using(var d=Db()){var t=await d.Tubos.SingleAsync(x=>x.Id==tubos[1].Id);Check(t.EstadoDisponibilidad==EstadoDisponibilidad.EnviadoACarga&&t.UbicacionId!=planta.Id,"Confirmación directa descuenta stock en planta");}
 await Rechaza(async()=>{await Accion(s=>s.Crear(new(){Destino="BA",Transporte="Prueba",TuboIds=[tubos[1].Id]},"Prueba",default,true));},"doble confirmación del mismo tubo");
 // La descripción se conserva o se actualiza al confirmar y aparece en el PDF.
 await using(var d=Db()){
  var cliente=new Cliente{Nombre="Cliente",Apellido="Descripción",Tipo=(TipoCliente)1};d.Clientes.Add(cliente);await d.SaveChangesAsync();
  var servicio=new PedidoService(d);
  for(var i=0;i<2;i++){
   var tubo=await d.Tubos.SingleAsync(x=>x.Id==tubos[i==0?0:4].Id);tubo.EstadoContenido=EstadoContenido.Lleno;tubo.EstadoDisponibilidad=EstadoDisponibilidad.Disponible;tubo.UbicacionId=planta.Id;await d.SaveChangesAsync();
   var creado=await servicio.CrearAsync(new(){ClienteId=cliente.Id,CilindroIds=[tubo.Id],ModalidadEntrega="RetiroPlanta",Observaciones="Pendiente de pago"},"Prueba",default);
   Check(creado.Observaciones=="Pendiente de pago","Descripción guardada al preparar");
   var descripcion=i==0?null:"Pagó en efectivo.\nSaldo pendiente: 0. " + new string('a',900);
   var firmado=await servicio.ConfirmarEntregaAsync(creado.Id,new(){VersionEsperada=creado.Version,Modalidad="Entrega",PersonaRecibe="Prueba",DniRecibe="1234",FirmaDataUrl="data:image/png;base64,"+new string('A',100),Observaciones=descripcion},"Prueba",default);
   var esperado=descripcion??"Pendiente de pago";
   Check(firmado.Observaciones==esperado,"Confirmar conserva o actualiza descripción del remito");
   Check((await d.OperacionesCilindro.SingleAsync(x=>x.RemitoEntregaId==d.Remitos.Single(r=>r.PedidoId==creado.Id).Id)).Observaciones==esperado,"Operación conserva descripción final");
   await Rechaza(async()=>{await servicio.ConfirmarEntregaAsync(creado.Id,new(){Observaciones="Modificado"},"Prueba",default);},"modificar descripción firmada");
   if(i==1){
    PdfSharp.Fonts.GlobalFontSettings.UseWindowsFontsUnderWindows=true;
    var builder=Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(new Microsoft.AspNetCore.Builder.WebApplicationOptions{ContentRootPath=args[0],WebRootPath=Path.Combine(args[0],"GestionTubos.Client","wwwroot")});
    var pdf=new GestionTubos.Api.Services.RemitoPdfGenerator(builder.Environment);
    var salida=Path.Combine(args[0],".run-logs","descripcion-remito.pdf");
    await File.WriteAllBytesAsync(salida,pdf.Entrega(firmado with { Cilindros=Enumerable.Range(1,24).SelectMany(_=>firmado.Cilindros).ToList() },"DNI 12345678"));
    Check(new FileInfo(salida).Length>1000,"PDF con descripción extensa generado");
   }
  }
 }
 Console.WriteLine(casos+" verificaciones de carga correctas en base temporal.");
}finally{if(!cs.InitialCatalog.StartsWith("GestionTubos_PruebaCarga_"))throw new Exception("Base inesperada");await basePrueba.Database.EnsureDeletedAsync();}

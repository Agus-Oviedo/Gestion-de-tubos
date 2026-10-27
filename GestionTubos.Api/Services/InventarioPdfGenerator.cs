using GestionTubos.Shared;
using GestionTubos.Shared.Tubos;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
namespace GestionTubos.Api.Services;
public sealed class InventarioPdfGenerator(IWebHostEnvironment entorno)
{
 public byte[] Crear(IReadOnlyList<TuboDto> tubos,string filtros)
 {
  using var doc=new PdfDocument();using var medirDoc=new PdfDocument();using var medir=XGraphics.FromPdfPage(medirDoc.AddPage());
  var normal=new XFont("Arial",8,XFontStyleEx.Regular,new XPdfFontOptions(PdfFontEncoding.Unicode));var bold=new XFont("Arial",8,XFontStyleEx.Bold,new XPdfFontOptions(PdfFontEncoding.Unicode));var titulo=new XFont("Arial",18,XFontStyleEx.Bold,new XPdfFontOptions(PdfFontEncoding.Unicode));
  var azul=XColor.FromArgb(7,58,112);double[] widths=[28,110,66,62,64,110,112,90,128];
  List<string> Wrap(string texto,double ancho){var resultado=new List<string>();var linea="";foreach(var palabra in texto.Split(' ',StringSplitOptions.RemoveEmptyEntries)){if(medir.MeasureString(linea+(linea.Length>0?" ":"")+palabra,normal).Width>ancho&&linea.Length>0){resultado.Add(linea);linea="";}foreach(var c in (linea.Length>0?" ":"")+palabra){if(medir.MeasureString(linea+c,normal).Width>ancho&&linea.Length>0){resultado.Add(linea);linea="";}linea+=c;}}if(linea.Length>0||resultado.Count==0)resultado.Add(linea);return resultado;}
  string Contenido(int n)=>n switch{1=>"Lleno",2=>"Vacío",3=>"Parcial",_=>"Sin determinar"};
  string Estado(int n)=>n switch{1=>"Disponible",2=>"Reservado",3=>"En reparto",4=>"Con cliente",5=>"En carga",6=>"Inspección",7=>"Mantenimiento",8=>"Prueba hidráulica",9=>"Baja",10=>"Extraviado",11=>"Preparado carga",12=>"Enviado carga",13=>"En batería",_=>"Otro"};
  string Gas(string g)=>string.Concat(g.Select(c=>c is >= '₀' and <= '₉'?(char)('0'+c-'₀'):c));
  var filas=tubos.OrderBy(t=>t.CodigoInterno).Select(t=>new[]{"",t.CodigoInterno,Gas(t.Gas),t.Capacidad,Contenido(t.EstadoContenido),t.Ubicacion,t.Propietario,Estado(t.EstadoDisponibilidad),t.FechaVencimientoPrueba?.ToString("dd/MM/yyyy")??"Sin registrar"}.Select((x,i)=>Wrap(x,widths[i]-12)).ToArray()).ToList();
  var bloques=new List<List<List<string>[]>>();var bloque=new List<List<string>[]>();var altoBloque=0d;
  foreach(var fila in filas){var alto=Math.Max(29,fila.Max(c=>c.Count)*12+10);if(altoBloque+alto>315&&bloque.Count>0){bloques.Add(bloque);bloque=[];altoBloque=0;}bloque.Add(fila);altoBloque+=alto;}if(bloque.Count>0||bloques.Count==0)bloques.Add(bloque);
  var filtrosLineas=Wrap("Filtros: "+filtros,770);var fecha=HoraArgentina.Ahora;
  for(var i=0;i<bloques.Count;i++){
   var p=doc.AddPage();p.Size=PdfSharp.PageSize.A4;p.Orientation=PdfSharp.PageOrientation.Landscape;using var g=XGraphics.FromPdfPage(p);var margen=35d;var ancho=widths.Sum();var y=30d;
   var logoPath=Path.Combine(entorno.WebRootPath??"","oxigeno-norte-logo.png");if(!File.Exists(logoPath))logoPath=Path.Combine(entorno.ContentRootPath,"..","GestionTubos.Client","wwwroot","oxigeno-norte-logo.png");if(File.Exists(logoPath)){using var logo=XImage.FromFile(logoPath);g.DrawImage(logo,margen,y,95,46);}else g.DrawString("OXÍGENO NORTE",bold,XBrushes.Black,margen,y+20);
   g.DrawString("LISTADO DE TUBOS",titulo,new XSolidBrush(azul),new XRect(margen,y,ancho,25),XStringFormats.TopRight);g.DrawString("Control de inventario · "+fecha.ToString("dd/MM/yyyy HH:mm"),normal,XBrushes.Black,new XRect(margen,y+30,ancho,15),XStringFormats.TopRight);
   y+=60;g.DrawLine(new XPen(azul,2),margen,y,margen+ancho,y);y+=18;
   foreach(var linea in filtrosLineas.Take(4)){g.DrawString(linea,normal,XBrushes.Black,margen,y);y+=13;}
   g.DrawString($"Total del listado: {tubos.Count} tubos · Marcar la casilla al verificar físicamente cada tubo.",bold,XBrushes.Black,margen,y+4);y+=24;
   string[] headers=["✓","Código del tubo","Gas","Capacidad","Contenido","Ubicación","Propietario","Estado","Vencimiento PH"];
   void Fila(List<string>[] valores,double alto,bool cabecera){var x=margen;for(var c=0;c<widths.Length;c++){if(cabecera)g.DrawRectangle(new XSolidBrush(azul),x,y,widths[c],alto);g.DrawRectangle(XPens.LightGray,x,y,widths[c],alto);if(c==0&&!cabecera)g.DrawRectangle(XPens.Gray,x+8,y+(alto-10)/2,10,10);else for(var l=0;l<valores[c].Count;l++)g.DrawString(valores[c][l],cabecera?bold:normal,cabecera?XBrushes.White:XBrushes.Black,new XRect(x+6,y+6+l*12,widths[c]-12,12),XStringFormats.TopLeft);x+=widths[c];}y+=alto;}
   Fila(headers.Select(h=>new List<string>{h=="✓"?"OK":h}).ToArray(),27,true);
   foreach(var fila in bloques[i])Fila(fila,Math.Max(29,fila.Max(c=>c.Count)*12+10),false);
   g.DrawString("Responsable: ______________________________    Fecha de control: ____ / ____ / ______",normal,XBrushes.Black,margen,p.Height.Point-54);
   g.DrawString($"Oxígeno Norte · Listado de control · Página {i+1} de {bloques.Count}",normal,XBrushes.Gray,new XRect(margen,p.Height.Point-32,ancho,12),XStringFormats.Center);
  }
  doc.Info.Title="Listado de tubos";using var salida=new MemoryStream();doc.Save(salida,false);return salida.ToArray();
 }
}

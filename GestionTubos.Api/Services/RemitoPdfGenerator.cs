using GestionTubos.Shared;
using GestionTubos.Shared.Operaciones;
using GestionTubos.Shared.Pedidos;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace GestionTubos.Api.Services;

public sealed class RemitoPdfGenerator(IWebHostEnvironment environment)
{
    private static readonly XColor Azul = XColor.FromArgb(7, 58, 112);
    private static readonly XColor Celeste = XColor.FromArgb(237, 246, 252);

    public byte[] Entrega(PedidoDto remito, string documentoCliente) => Crear(
        "REMITO", remito.NumeroRemito ?? remito.Numero,
        remito.FechaFirmaUtc ?? remito.FechaHoraUtc, remito.Cliente, documentoCliente,
        $"Pedido: {remito.Numero}", "Tipo: Entrega", $"Estado: {remito.Estado}",
        remito.Cilindros.Select(x => (x.Codigo, x.Gas, x.Capacidad)).ToList(),
        remito.PersonaRecibe, remito.DniRecibe, remito.FirmaDataUrl,
        remito.FechaFirmaUtc.HasValue ? $"Firmado el {remito.FechaFirmaUtc.Value.EnArgentina():dd/MM/yyyy HH:mm}" : null,
        remito.Observaciones);

    public byte[] Devolucion(DevolucionLoteDto remito, string documentoCliente) => Crear(
        "DEVOLUCIÓN", remito.Numero, remito.FechaConfirmacionUtc ?? remito.FechaCreacionUtc,
        remito.Cliente, documentoCliente, $"Remito original: {remito.NumeroRemitoEntrega}",
        "Tipo: Devolución", $"Estado: {remito.Estado}",
        remito.Tubos.Select(x => (x.CodigoCilindro, x.Gas, x.Capacidad)).ToList(),
        remito.PersonaEntrega, remito.DniEntrega, remito.FirmaDataUrl,
        remito.FechaConfirmacionUtc.HasValue ? $"Devolución confirmada el {remito.FechaConfirmacionUtc.Value.EnArgentina():dd/MM/yyyy HH:mm}" : null,
        remito.Observaciones, remito.Tubos.Select(x => x.DiasEnPoderCliente).ToList());

    public byte[] Carga(string titulo,string numero,DateTime fecha,string destino,string transporte,string estado,IReadOnlyList<string> tubos,string? observaciones,string usuario)
    {
        using var documento=new PdfDocument();using var medicion=new PdfDocument();using var medir=XGraphics.FromPdfPage(medicion.AddPage());
        var unicode=new XPdfFontOptions(PdfFontEncoding.Unicode);var normal=new XFont("Arial",10,XFontStyleEx.Regular,unicode);var negrita=new XFont("Arial",10,XFontStyleEx.Bold,unicode);var pequeno=new XFont("Arial",8,XFontStyleEx.Regular,unicode);
        const double margen=42,ancho=511;
        var recepcion=titulo.StartsWith("RECEPCIÓN");var columnas=recepcion?new[]{185d,85d,100d,141d}:new[]{ancho*.60,ancho*.20,ancho*.20};
        List<string> Envolver(string texto,double anchoTexto)
        {
            var salida=new List<string>();var linea="";
            foreach(var parrafo in texto.Replace("\r", "").Split('\n'))
            {
                foreach(var palabra in parrafo.Split(' ',StringSplitOptions.RemoveEmptyEntries))
                {
                    var propuesta=linea.Length==0?palabra:linea+" "+palabra;
                    if(medir.MeasureString(propuesta,normal).Width>anchoTexto&&linea.Length>0){salida.Add(linea);linea="";}
                    if(medir.MeasureString(palabra,normal).Width>anchoTexto)
                    {
                        foreach(var c in palabra){if(medir.MeasureString(linea+c,normal).Width>anchoTexto&&linea.Length>0){salida.Add(linea);linea="";}linea+=c;}
                    }
                    else linea=linea.Length==0?palabra:linea+" "+palabra;
                }
                if(linea.Length>0){salida.Add(linea);linea="";}
            }
            if(linea.Length>0||salida.Count==0)salida.Add(linea);return salida;
        }
        var filas=tubos.Select(t=>{var partes=t.Split('|').Select(x=>x.Trim()).ToArray();return Enumerable.Range(0,columnas.Length).Select(i=>Envolver(i<partes.Length?partes[i]:"",columnas[i]-16)).ToArray();}).ToList();
        var bloques=new List<List<List<string>[]>>();var actual=new List<List<string>[]>();double altura=0;
        foreach(var fila in filas){var alto=Math.Max(28,fila.Max(x=>x.Count)*14+12);if(altura+alto>280&&actual.Count>0){bloques.Add(actual);actual=[];altura=0;}actual.Add(fila);altura+=alto;}if(actual.Count>0||bloques.Count==0)bloques.Add(actual);
        var obs=string.IsNullOrWhiteSpace(observaciones)?new List<string>():Envolver(observaciones,ancho-28);
        var obsSeparadas=obs.Count>3;var paginasObservaciones=obsSeparadas?(obs.Count+27)/28:0;var totalPaginas=bloques.Count+paginasObservaciones;
        var logoPath=Path.Combine(environment.WebRootPath??"","oxigeno-norte-logo.png");
        for(var paginaIndice=0;paginaIndice<totalPaginas;paginaIndice++)
        {
            var p=documento.AddPage();p.Size=PdfSharp.PageSize.A4;using var g=XGraphics.FromPdfPage(p);
            var y=42d;
            if(File.Exists(logoPath)){using var logo=XImage.FromFile(logoPath);g.DrawImage(logo,margen,y,125,60);}
            var tit=new XFont("Arial",22,XFontStyleEx.Bold,unicode);
            g.DrawString(titulo,tit,new XSolidBrush(Azul),new XRect(margen,y+2,ancho,25),XStringFormats.TopRight);
            g.DrawString(numero,new XFont("Arial",14,XFontStyleEx.Bold,unicode),XBrushes.Black,new XRect(margen,y+30,ancho,20),XStringFormats.TopRight);
            g.DrawString("Fecha: "+fecha.EnArgentina().ToString("dd/MM/yyyy HH:mm"),normal,XBrushes.Black,new XRect(margen,y+51,ancho,15),XStringFormats.TopRight);
            y+=78;g.DrawLine(new XPen(Azul,2),margen,y,margen+ancho,y);y+=18;
            g.DrawString("OXÍGENO NORTE",negrita,XBrushes.Black,margen,y);g.DrawString("Santa Rosa, La Pampa",normal,XBrushes.Black,margen,y+16);
            var datos=new[]{"Transporte: "+transporte,"Estado: "+estado,"Registrado por: "+usuario};var datoY=y;
            foreach(var dato in datos){foreach(var linea in Envolver(dato,ancho*.57)){g.DrawString(linea,normal,XBrushes.Black,new XRect(margen+ancho*.40,datoY,ancho*.60,15),XStringFormats.TopRight);datoY+=16;}}
            y=Math.Max(y+58,datoY+10);g.DrawLine(XPens.LightGray,margen,y,margen+ancho,y);y+=18;
            var destinoLineas=Envolver(destino,ancho-28);var altoDestino=Math.Max(58,28+destinoLineas.Count*20);
            g.DrawRectangle(new XSolidBrush(Celeste),margen,y,ancho,altoDestino);
            g.DrawString("DESTINO / CARGADORA",new XFont("Arial",8,XFontStyleEx.Bold,unicode),XBrushes.Gray,margen+14,y+14);
            foreach(var linea in destinoLineas){g.DrawString(linea,new XFont("Arial",15,XFontStyleEx.Bold,unicode),new XSolidBrush(Azul),margen+14,y+34);y+=20;}
            y+=altoDestino-destinoLineas.Count*20+18;
            if(estado=="Preparado"){g.DrawString("PENDIENTE DE VERIFICACIÓN · SALIDA NO CONFIRMADA",negrita,XBrushes.DarkRed,margen,y);y+=22;}
            if(paginaIndice<bloques.Count)
            {
                string[] encabezados=recepcion?["Código del cilindro","Gas","Capacidad","Resultado"]:["Código del cilindro","Gas","Capacidad"];
                DibujarFila(g,margen,y,columnas,28,encabezados,true,normal,negrita);y+=28;
                foreach(var fila in bloques[paginaIndice])
                {
                    var alto=Math.Max(28,fila.Max(x=>x.Count)*14+12);var x=margen;
                    for(var i=0;i<columnas.Length;i++){g.DrawRectangle(XPens.LightGray,x,y,columnas[i],alto);for(var j=0;j<fila[i].Count;j++){var texto=fila[i][j];if(i==1&&texto.Any(EsSubindice))DibujarFormula(g,texto,x+8,y+6+j*14,columnas[i]-16,14,normal);else g.DrawString(texto,normal,XBrushes.Black,new XRect(x+8,y+6+j*14,columnas[i]-16,14),XStringFormats.TopLeft);}x+=columnas[i];}y+=alto;
                }
                DibujarFila(g,margen,y,[ancho-100,100],28,["Total de tubos del remito",tubos.Count.ToString()],false,normal,negrita,true);y+=42;
                if(paginaIndice==bloques.Count-1&&!obsSeparadas&&obs.Count>0){g.DrawString("Observaciones",negrita,new XSolidBrush(Azul),margen,y);y+=18;foreach(var linea in obs){g.DrawString(linea,normal,XBrushes.Black,margen,y);y+=14;}}
            }
            else
            {
                g.DrawString("Observaciones",negrita,new XSolidBrush(Azul),margen,y);y+=25;foreach(var linea in obs.Skip((paginaIndice-bloques.Count)*28).Take(28)){g.DrawString(linea,normal,XBrushes.Black,margen,y);y+=16;}
            }
            if(paginaIndice==totalPaginas-1&&y<650)
            {
                y+=18;g.DrawRoundedRectangle(XPens.LightGray,margen,y,ancho,105,6,6);
                g.DrawString("Registrado por",pequeno,XBrushes.Gray,margen+260,y+18);
                foreach(var linea in Envolver(usuario,ancho-280).Take(2)){g.DrawString(linea,negrita,XBrushes.Black,margen+260,y+36);y+=14;}
                g.DrawString(fecha.EnArgentina().ToString("dd/MM/yyyy HH:mm"),normal,XBrushes.Black,margen+260,y+52);
            }
            g.DrawString($"Documento generado por el Sistema de Gestión de Cilindros · {numero} · {paginaIndice+1}/{totalPaginas}",new XFont("Arial",7,XFontStyleEx.Regular,unicode),XBrushes.Gray,new XRect(margen,p.Height.Point-35,ancho,12),XStringFormats.Center);
        }
        documento.Info.Title=titulo+" "+numero;using var salidaPdf=new MemoryStream();documento.Save(salidaPdf,false);return salidaPdf.ToArray();
    }
    private byte[] Crear(string titulo, string numero, DateTime fecha, string cliente, string documentoCliente,
        string dato1, string dato2, string dato3, IReadOnlyList<(string Codigo, string Gas, string Capacidad)> tubos,
        string? persona, string? dni, string? firma, string? textoFirma, string? observaciones, IReadOnlyList<int>? diasConCliente = null)
    {
        using var documento = new PdfDocument();
        documento.Info.Title = $"{cliente}_{numero}";
        var pagina = documento.AddPage();
        pagina.Size = PdfSharp.PageSize.A4;
        var g = XGraphics.FromPdfPage(pagina);
        var unicode = new XPdfFontOptions(PdfFontEncoding.Unicode);
        var normal = new XFont("Arial", 10, XFontStyleEx.Regular, unicode);
        var negrita = new XFont("Arial", 10, XFontStyleEx.Bold, unicode);
        var encabezado = new XFont("Arial", 22, XFontStyleEx.Bold, unicode);
        var margen = 42d; var ancho = pagina.Width.Point - margen * 2; var y = 42d;

        void Pie() => g.DrawString($"Documento generado por el Sistema de Gestión de Cilindros · {numero}", new XFont("Arial", 7, XFontStyleEx.Regular, unicode), XBrushes.Gray, new XRect(margen, pagina.Height.Point - 35, ancho, 12), XStringFormats.Center);
        void Continuar()
        {
            Pie(); g.Dispose(); pagina = documento.AddPage(); pagina.Size = PdfSharp.PageSize.A4; g = XGraphics.FromPdfPage(pagina);
            g.DrawString($"{titulo} {numero} · Continuación", negrita, new XSolidBrush(Azul), margen, 45);
            g.DrawLine(new XPen(Azul, 2), margen, 58, margen + ancho, 58); y = 82;
        }
        IEnumerable<string> Lineas(string texto)
        {
            foreach (var parrafo in texto.Replace("\r", "").Split('\n'))
            {
                var linea = "";
                foreach (var caracter in parrafo)
                {
                    if (g.MeasureString(linea + caracter, normal).Width > ancho - 4 && linea.Length > 0) { yield return linea; linea = ""; }
                    linea += caracter;
                }
                yield return linea;
            }
        }

        var logoPath = Path.Combine(environment.WebRootPath ?? string.Empty, "oxigeno-norte-logo.png");
        if (File.Exists(logoPath)) using (var logo = XImage.FromFile(logoPath)) g.DrawImage(logo, margen, y, 125, 60);
        g.DrawString(titulo, encabezado, new XSolidBrush(Azul), new XRect(margen, y + 2, ancho, 25), XStringFormats.TopRight);
        g.DrawString(numero, new XFont("Arial", 14, XFontStyleEx.Bold, unicode), XBrushes.Black, new XRect(margen, y + 30, ancho, 20), XStringFormats.TopRight);
        g.DrawString($"Fecha: {fecha.EnArgentina():dd/MM/yyyy HH:mm}", normal, XBrushes.Black, new XRect(margen, y + 51, ancho, 15), XStringFormats.TopRight);
        y += 78; g.DrawLine(new XPen(Azul, 2), margen, y, margen + ancho, y); y += 18;

        g.DrawString("OXÍGENO NORTE", negrita, XBrushes.Black, margen, y);
        g.DrawString("Santa Rosa, La Pampa", normal, XBrushes.Black, margen, y + 16);
        g.DrawString(dato1, negrita, XBrushes.Black, new XRect(margen, y, ancho, 15), XStringFormats.TopRight);
        g.DrawString(dato2, normal, XBrushes.Black, new XRect(margen, y + 16, ancho, 15), XStringFormats.TopRight);
        g.DrawString(dato3, normal, XBrushes.Black, new XRect(margen, y + 32, ancho, 15), XStringFormats.TopRight);
        y += 58; g.DrawLine(XPens.LightGray, margen, y, margen + ancho, y); y += 18;

        g.DrawRectangle(new XSolidBrush(Celeste), margen, y, ancho, 58);
        g.DrawString("CLIENTE", new XFont("Arial", 8, XFontStyleEx.Bold, unicode), XBrushes.Gray, margen + 14, y + 14);
        g.DrawString(cliente, new XFont("Arial", 15, XFontStyleEx.Bold, unicode), new XSolidBrush(Azul), margen + 14, y + 34);
        g.DrawString(documentoCliente, normal, XBrushes.Black, new XRect(margen, y + 34, ancho - 14, 15), XStringFormats.TopRight);
        y += 76;

        var columnas = diasConCliente is null
            ? new[] { ancho * .60, ancho * .20, ancho * .20 }
            : new[] { ancho * .36, ancho * .18, ancho * .20, ancho * .26 };
        string[] titulos = diasConCliente is null ? ["Código del cilindro", "Gas", "Capacidad"] : ["Código del cilindro", "Gas", "Capacidad", "Días con el cliente"];
        DibujarFila(g, margen, y, columnas, 28, titulos, true, normal, negrita); y += 28;
        for (var i = 0; i < tubos.Count; i++)
        {
            if (y + 27 > pagina.Height.Point - 65) { Continuar(); DibujarFila(g, margen, y, columnas, 28, titulos, true, normal, negrita); y += 28; }
            var tubo = tubos[i];
            string[] valores = diasConCliente is null ? [tubo.Codigo, tubo.Gas, tubo.Capacidad] : [tubo.Codigo, tubo.Gas, tubo.Capacidad, diasConCliente[i].ToString()];
            DibujarFila(g, margen, y, columnas, 27, valores, false, normal, negrita); y += 27;
        }
        if (y + 45 > pagina.Height.Point - 65) Continuar();
        DibujarFila(g, margen, y, [ancho - columnas[^1], columnas[^1]], 28, ["Total de cilindros", tubos.Count.ToString()], false, normal, negrita, true); y += 45;
        if (!string.IsNullOrWhiteSpace(observaciones))
        {
            if (y + 40 > pagina.Height.Point - 65) Continuar();
            g.DrawString("Descripción / observaciones", negrita, new XSolidBrush(Azul), margen, y); y += 20;
            foreach (var linea in Lineas(observaciones).ToList())
            {
                if (y + 16 > pagina.Height.Point - 65) Continuar();
                g.DrawString(linea, normal, XBrushes.Black, margen, y); y += 15;
            }
            y += 16;
        }
        if (y + 115 > pagina.Height.Point - 55) Continuar();
        g.DrawRoundedRectangle(XPens.LightGray, margen, y, ancho, 105, 6, 6);
        if (TryImagen(firma, out var imagenFirma) && imagenFirma is not null) { using (imagenFirma) g.DrawImage(imagenFirma, margen + 18, y + 12, 210, 75); }
        g.DrawString(persona ?? string.Empty, negrita, XBrushes.Black, margen + 260, y + 30);
        g.DrawString(string.IsNullOrWhiteSpace(dni) ? string.Empty : $"DNI {dni}", normal, XBrushes.Black, margen + 260, y + 48);
        g.DrawString(textoFirma ?? string.Empty, normal, XBrushes.Black, margen + 260, y + 66);
        Pie(); g.Dispose();

        using var salida = new MemoryStream(); documento.Save(salida, false); return salida.ToArray();
    }

    private static void DibujarFila(XGraphics g, double x, double y, double[] anchos, double alto, string[] textos, bool cabecera, XFont normal, XFont negrita, bool total = false)
    {
        for (var i = 0; i < anchos.Length; i++)
        {
            var fondo = cabecera ? new XSolidBrush(Azul) : total ? new XSolidBrush(Celeste) : XBrushes.White;
            g.DrawRectangle(fondo, x, y, anchos[i], alto); g.DrawRectangle(XPens.LightGray, x, y, anchos[i], alto);
            if (!cabecera && !total && i == 1 && textos[i].Any(EsSubindice))
                DibujarFormula(g, textos[i], x + 8, y, anchos[i] - 16, alto, normal);
            else
                g.DrawString(textos[i], cabecera || total ? negrita : normal, cabecera ? XBrushes.White : XBrushes.Black,
                    new XRect(x + 8, y, anchos[i] - 16, alto), total ? XStringFormats.Center : XStringFormats.CenterLeft);
            x += anchos[i];
        }
    }

    private static void DibujarFormula(XGraphics g, string formula, double x, double y, double ancho, double alto, XFont normal)
    {
        var posicion = formula.IndexOfAny(['₀','₁','₂','₃','₄','₅','₆','₇','₈','₉']);
        if (posicion < 0) { g.DrawString(formula, normal, XBrushes.Black, new XRect(x, y, ancho, alto), XStringFormats.CenterLeft); return; }
        var baseFormula = formula[..posicion];
        var subindice = string.Concat(formula[posicion..].Select(NumeroNormal));
        g.DrawString(baseFormula, normal, XBrushes.Black, new XRect(x, y, ancho, alto), XStringFormats.CenterLeft);
        var desplazamiento = g.MeasureString(baseFormula, normal).Width;
        var fuenteSubindice = new XFont("Arial", 7, XFontStyleEx.Regular, new XPdfFontOptions(PdfFontEncoding.Unicode));
        g.DrawString(subindice, fuenteSubindice, XBrushes.Black, new XRect(x + desplazamiento, y + 4, ancho - desplazamiento, alto), XStringFormats.CenterLeft);
    }

    private static bool EsSubindice(char c) => c is >= '₀' and <= '₉';
    private static char NumeroNormal(char c) => c switch {'₀'=>'0','₁'=>'1','₂'=>'2','₃'=>'3','₄'=>'4','₅'=>'5','₆'=>'6','₇'=>'7','₈'=>'8','₉'=>'9',_=>c};

    private static bool TryImagen(string? dataUrl, out XImage? imagen)
    {
        imagen = null; if (string.IsNullOrWhiteSpace(dataUrl)) return false;
        var coma = dataUrl.IndexOf(','); if (coma < 0) return false;
        try { var bytes = Convert.FromBase64String(dataUrl[(coma + 1)..]); imagen = XImage.FromStream(new MemoryStream(bytes)); return true; }
        catch { return false; }
    }
}

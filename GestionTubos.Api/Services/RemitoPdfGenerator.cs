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
        remito.Observaciones);

    private byte[] Crear(string titulo, string numero, DateTime fecha, string cliente, string documentoCliente,
        string dato1, string dato2, string dato3, IReadOnlyList<(string Codigo, string Gas, string Capacidad)> tubos,
        string? persona, string? dni, string? firma, string? textoFirma, string? observaciones)
    {
        using var documento = new PdfDocument();
        documento.Info.Title = $"{cliente}_{numero}";
        var pagina = documento.AddPage();
        pagina.Size = PdfSharp.PageSize.A4;
        using var g = XGraphics.FromPdfPage(pagina);
        var unicode = new XPdfFontOptions(PdfFontEncoding.Unicode);
        var normal = new XFont("Arial", 10, XFontStyleEx.Regular, unicode);
        var negrita = new XFont("Arial", 10, XFontStyleEx.Bold, unicode);
        var encabezado = new XFont("Arial", 22, XFontStyleEx.Bold, unicode);
        var margen = 42d; var ancho = pagina.Width.Point - margen * 2; var y = 42d;

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

        var columnas = new[] { ancho * .60, ancho * .20, ancho * .20 };
        DibujarFila(g, margen, y, columnas, 28, ["Código del cilindro", "Gas", "Capacidad"], true, normal, negrita); y += 28;
        foreach (var tubo in tubos) { DibujarFila(g, margen, y, columnas, 27, [tubo.Codigo, tubo.Gas, tubo.Capacidad], false, normal, negrita); y += 27; }
        DibujarFila(g, margen, y, [columnas[0] + columnas[1], columnas[2]], 28, ["Total de cilindros", tubos.Count.ToString()], false, normal, negrita, true); y += 45;

        if (!string.IsNullOrWhiteSpace(observaciones)) { g.DrawString($"Observaciones: {observaciones}", normal, XBrushes.Black, new XRect(margen, y, ancho, 35)); y += 42; }
        g.DrawRoundedRectangle(XPens.LightGray, margen, y, ancho, 105, 6, 6);
        if (TryImagen(firma, out var imagenFirma) && imagenFirma is not null) { using (imagenFirma) g.DrawImage(imagenFirma, margen + 18, y + 12, 210, 75); }
        g.DrawString(persona ?? string.Empty, negrita, XBrushes.Black, margen + 260, y + 30);
        g.DrawString(string.IsNullOrWhiteSpace(dni) ? string.Empty : $"DNI {dni}", normal, XBrushes.Black, margen + 260, y + 48);
        g.DrawString(textoFirma ?? string.Empty, normal, XBrushes.Black, margen + 260, y + 66);
        g.DrawString($"Documento generado por el Sistema de Gestión de Cilindros · {numero}", new XFont("Arial", 7, XFontStyleEx.Regular, unicode), XBrushes.Gray, new XRect(margen, pagina.Height.Point - 35, ancho, 12), XStringFormats.Center);

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

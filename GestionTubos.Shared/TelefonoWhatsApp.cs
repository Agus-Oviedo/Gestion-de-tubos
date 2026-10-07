using System.Text;

namespace GestionTubos.Shared;

public static class TelefonoWhatsApp
{
    private const string PrefijoLocal = "5492954";

    public static string? Normalizar(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono)) return null;

        var original = telefono.Trim();
        var digitos = SoloDigitos(original);
        if (digitos.Length == 0) return null;

        // Un número que comienza con + ya incluye el país y el área elegidos por el operador.
        if (original.StartsWith('+')) return "+" + digitos;

        // Los números argentinos escritos con 54 se conservan y se presentan con el signo +.
        if (digitos.StartsWith("54") && digitos.Length >= 11) return "+" + digitos;

        // Diez dígitos representan característica argentina + número celular.
        var nacional = digitos.TrimStart('0');
        if (nacional.Length >= 9) return "+549" + nacional;

        // Un número corto se considera local de Santa Rosa (2954).
        return "+" + PrefijoLocal + nacional;
    }

    public static string? Destino(string? telefono)
    {
        var normalizado = Normalizar(telefono);
        return normalizado is null ? null : SoloDigitos(normalizado);
    }

    private static string SoloDigitos(string valor)
    {
        var resultado = new StringBuilder(valor.Length);
        foreach (var caracter in valor)
            if (char.IsDigit(caracter)) resultado.Append(caracter);
        return resultado.ToString();
    }
}

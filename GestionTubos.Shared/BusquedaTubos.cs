using System.Globalization;
using System.Text;
namespace GestionTubos.Shared;
public static class BusquedaTubos
{
 private static string Normalizar(string? valor)=>string.Concat((valor??"").ToLowerInvariant().Select(c=>c switch{'á' or 'à' or 'ä' or 'â'=>'a','é' or 'è' or 'ë' or 'ê'=>'e','í' or 'ì' or 'ï' or 'î'=>'i','ó' or 'ò' or 'ö' or 'ô'=>'o','ú' or 'ù' or 'ü' or 'û'=>'u','ñ'=>'n',>= '₀' and <= '₉'=>(char)('0'+c-'₀'),_=>c}).Where(c=>c is not (>= '\u0300' and <= '\u036f'))).Trim();
 private static string? GasCanonico(string valor)=>valor switch{"o2" or "oxigeno"=>"o2","n2" or "nitrogeno"=>"n2","co2" or "dioxido de carbono" or "anhidrido carbonico"=>"co2","ar" or "argon"=>"ar","he" or "helio"=>"he","h2" or "hidrogeno"=>"h2",_=>null};
 public static bool Coincide(string? buscar,string gas,params string?[] valores)
 {
  if(string.IsNullOrWhiteSpace(buscar))return true;
  var consulta=Normalizar(buscar);var nombreGas=Normalizar(gas);var canonico=GasCanonico(nombreGas);
  var gasConsultado=GasCanonico(consulta);if(gasConsultado is not null)return canonico==gasConsultado;
  var aliases=canonico switch{"o2"=>"o2 oxigeno","n2"=>"n2 nitrogeno","co2"=>"co2 dioxido de carbono anhidrido carbonico","ar"=>"ar argon","he"=>"he helio","h2"=>"h2 hidrogeno",_=>nombreGas};
  var texto=Normalizar(string.Join(" ",valores.Append(gas).Append(aliases)));
  return consulta.Split(' ',StringSplitOptions.RemoveEmptyEntries).All(t=>GasCanonico(t) is string g?canonico==g:texto.Contains(t,StringComparison.Ordinal));
 }
}

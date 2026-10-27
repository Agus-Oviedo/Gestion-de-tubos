using System.ComponentModel.DataAnnotations;
namespace GestionTubos.Shared.Tubos;
public sealed class ListadoTubosPdfDto
{
 [MinLength(1,ErrorMessage="No hay tubos para exportar.")]public List<int>TuboIds{get;set;}=[];
 [StringLength(1000)]public string Filtros{get;set;}="Todos los tubos";
}

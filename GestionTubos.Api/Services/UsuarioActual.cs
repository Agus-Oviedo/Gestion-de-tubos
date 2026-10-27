using System.Security.Claims;
using GestionTubos.Application.Interfaces;
namespace GestionTubos.Api.Services;
public sealed class UsuarioActual(IHttpContextAccessor contexto):IUsuarioActual
{public string Id=>contexto.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)??"Sistema";}

using GestionTubos.Application.Interfaces;
using GestionTubos.Domain.Entities;
using GestionTubos.Domain.Enums;
using GestionTubos.Infrastructure.Data;
using GestionTubos.Shared.Clientes;
using Microsoft.EntityFrameworkCore;

namespace GestionTubos.Infrastructure.Services;

public sealed class ClienteService(GestionTubosDbContext dbContext) : IClienteService
{
    public async Task<IReadOnlyList<ClienteDto>> ObtenerTodosAsync(string? buscar, CancellationToken cancellationToken)
    {
        var query = dbContext.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            query = query.Where(x =>
                (x.Nombre != null && x.Nombre.Contains(texto)) ||
                (x.Apellido != null && x.Apellido.Contains(texto)) ||
                (x.RazonSocial != null && x.RazonSocial.Contains(texto)) ||
                (x.Dni != null && x.Dni.Contains(texto)) ||
                (x.CuitCuil != null && x.CuitCuil.Contains(texto)));
        }

        var clientes = await query.OrderBy(x => x.RazonSocial ?? x.Apellido).ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);
        return clientes.Select(Mapear).ToList();
    }

    public async Task<ClienteDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return cliente is null ? null : Mapear(cliente);
    }

    public async Task<ClienteDto> CrearAsync(ClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        await ValidarDuplicadosAsync(dto, null, cancellationToken);
        var cliente = new Cliente();
        Copiar(dto, cliente);
        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Mapear(cliente);
    }

    public async Task<ClienteDto?> ActualizarAsync(int id, ClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null) return null;
        await ValidarDuplicadosAsync(dto, id, cancellationToken);
        Copiar(dto, cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Mapear(cliente);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken)
    {
        var cliente = await dbContext.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null) return false;
        cliente.Activo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<DireccionClienteDto>?> ObtenerDireccionesAsync(int clienteId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Clientes.AnyAsync(x => x.Id == clienteId, cancellationToken)) return null;
        return await dbContext.DireccionesCliente.AsNoTracking()
            .Where(x => x.ClienteId == clienteId)
            .OrderByDescending(x => x.EsPrincipalEntrega).ThenByDescending(x => x.EsFiscal)
            .Select(x => new DireccionClienteDto(x.Id, x.Calle, x.Numero, x.Localidad, x.Provincia, x.EsFiscal, x.EsPrincipalEntrega))
            .ToListAsync(cancellationToken);
    }

    public async Task<DireccionClienteDto?> CrearDireccionAsync(int clienteId, DireccionClienteGuardarDto dto, CancellationToken cancellationToken)
    {
        if (!await dbContext.Clientes.AnyAsync(x => x.Id == clienteId, cancellationToken)) return null;
        if (dto.EsPrincipalEntrega)
        {
            var anteriores = await dbContext.DireccionesCliente
                .Where(x => x.ClienteId == clienteId && x.EsPrincipalEntrega).ToListAsync(cancellationToken);
            foreach (var anterior in anteriores) anterior.EsPrincipalEntrega = false;
        }
        var direccion = new DireccionCliente
        {
            ClienteId = clienteId, Calle = dto.Calle.Trim(), Numero = Limpiar(dto.Numero),
            PisoDepartamento = Limpiar(dto.PisoDepartamento), Localidad = dto.Localidad.Trim(),
            Provincia = dto.Provincia.Trim(), CodigoPostal = Limpiar(dto.CodigoPostal),
            EsFiscal = dto.EsFiscal, EsPrincipalEntrega = dto.EsPrincipalEntrega
        };
        dbContext.DireccionesCliente.Add(direccion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new(direccion.Id, direccion.Calle, direccion.Numero, direccion.Localidad, direccion.Provincia, direccion.EsFiscal, direccion.EsPrincipalEntrega);
    }

    public async Task<bool> EliminarDireccionAsync(int clienteId, int direccionId, CancellationToken cancellationToken)
    {
        var direccion = await dbContext.DireccionesCliente
            .FirstOrDefaultAsync(x => x.Id == direccionId && x.ClienteId == clienteId, cancellationToken);
        if (direccion is null) return false;
        direccion.Activa = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidarDuplicadosAsync(ClienteGuardarDto dto, int? id, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(dto.Dni) && await dbContext.Clientes
            .AnyAsync(x => x.Id != id && x.Dni == dto.Dni.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe un cliente con ese DNI.");
        if (!string.IsNullOrWhiteSpace(dto.CuitCuil) && await dbContext.Clientes
            .AnyAsync(x => x.Id != id && x.CuitCuil == dto.CuitCuil.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe un cliente con ese CUIT/CUIL.");
    }

    private static void Copiar(ClienteGuardarDto dto, Cliente cliente)
    {
        cliente.Tipo = (TipoCliente)dto.Tipo;
        cliente.Nombre = Limpiar(dto.Nombre);
        cliente.Apellido = Limpiar(dto.Apellido);
        cliente.RazonSocial = Limpiar(dto.RazonSocial);
        cliente.NombreComercial = Limpiar(dto.NombreComercial);
        cliente.Dni = Limpiar(dto.Dni);
        cliente.CuitCuil = Limpiar(dto.CuitCuil);
        cliente.Responsable = Limpiar(dto.Responsable);
        cliente.Telefono = Limpiar(dto.Telefono);
        cliente.Correo = Limpiar(dto.Correo);
        cliente.CondicionFiscal = (CondicionFiscal)dto.CondicionFiscal;
    }

    private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static ClienteDto Mapear(Cliente x) => new(
        x.Id, (int)x.Tipo,
        x.Tipo == TipoCliente.Empresa ? x.RazonSocial ?? "Empresa sin nombre" : $"{x.Apellido}, {x.Nombre}".Trim(' ', ','),
        x.Nombre, x.Apellido, x.RazonSocial, x.NombreComercial, x.Dni, x.CuitCuil,
        x.Responsable, x.Telefono, x.Correo, (int)x.CondicionFiscal);
}

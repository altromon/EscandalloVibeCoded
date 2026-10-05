using EscandalloWeb.Data;
using EscandalloWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EscandalloWeb.Services;

public class SesionUsuarioService(IDbContextFactory<AppDbContext> dbFactory)
{
    public int? UsuarioActualId { get; private set; }
    public int? CentroActualId { get; private set; }

    public event Action? OnEstadoCambiado;

    public Task AsegurarSesionInicialAsync() => Task.CompletedTask;

    public async Task<Usuario?> ObtenerUsuarioActualAsync()
    {
        if (!UsuarioActualId.HasValue)
        {
            return null;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Usuarios
            .Include(u => u.Centros)
            .Include(u => u.Pagos.OrderByDescending(p => p.Fecha))
            .FirstOrDefaultAsync(u => u.Id == UsuarioActualId.Value);
    }

    public async Task<(bool Exito, string Mensaje)> IniciarSesionAsync(string email, string password)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var usuario = await db.Usuarios
            .Include(u => u.Centros)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower() && u.PasswordHash == password);

        if (usuario is null)
        {
            return (false, "Credenciales incorrectas.");
        }

        UsuarioActualId = usuario.Id;
        CentroActualId = usuario.Centros.FirstOrDefault()?.Id;
        NotificarCambio();
        return (true, "Sesión iniciada correctamente.");
    }

    public async Task<(bool Exito, string Mensaje)> RegistrarUsuarioAsync(string nombre, string email, string password, string nombrePrimerCentro, bool cargarPlantillaExcel)
    {
        if (UsuarioActualId.HasValue)
        {
            return (false, "No puedes crear cuentas para otros clientes con una sesión iniciada.");
        }

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Completa todos los campos obligatorios.");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var emailNormalizado = email.Trim().ToLower();
        if (await db.Usuarios.AnyAsync(u => u.Email.ToLower() == emailNormalizado))
        {
            return (false, "Ya existe una cuenta registrada con ese correo electrónico.");
        }

        var nuevoUsuario = new Usuario
        {
            Nombre = nombre.Trim(),
            Email = emailNormalizado,
            PasswordHash = password,
            Plan = PlanSuscripcion.Ninguno,
            SuscripcionActiva = false
        };

        db.Usuarios.Add(nuevoUsuario);
        await db.SaveChangesAsync();

        var nombreCentro = string.IsNullOrWhiteSpace(nombrePrimerCentro) ? "Centro Principal" : nombrePrimerCentro.Trim();
        var centro = cargarPlantillaExcel
            ? ExcelTemplateSeeder.CrearCentroDesdeExcel(nuevoUsuario.Id, nombreCentro, "")
            : new CentroEstetica { UsuarioId = nuevoUsuario.Id, Nombre = nombreCentro, Iva = 0.21m, Beneficio = 0.50m };

        db.Centros.Add(centro);
        await db.SaveChangesAsync();

        UsuarioActualId = nuevoUsuario.Id;
        CentroActualId = centro.Id;
        NotificarCambio();
        return (true, "Cuenta creada. Activa tu suscripción para gestionar los escandallos.");
    }

    public void CerrarSesion()
    {
        UsuarioActualId = null;
        CentroActualId = null;
        NotificarCambio();
    }

    public void SeleccionarCentro(int centroId)
    {
        CentroActualId = centroId;
        NotificarCambio();
    }

    public async Task<(bool Exito, string Mensaje, CentroEstetica? Centro)> CrearCentroAsync(string nombre, string direccion, bool cargarPlantillaExcel)
    {
        if (!UsuarioActualId.HasValue)
        {
            return (false, "Debes iniciar sesión.", null);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return (false, "El nombre del centro es obligatorio.", null);
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var usuario = await db.Usuarios.Include(u => u.Centros).FirstOrDefaultAsync(u => u.Id == UsuarioActualId.Value);
        if (usuario is null)
        {
            return (false, "Usuario no encontrado.", null);
        }

        if (!usuario.SuscripcionActiva)
        {
            return (false, "Necesitas una suscripción activa para añadir centros.", null);
        }

        if (usuario.Plan == PlanSuscripcion.ProfesionalMensual && usuario.Centros.Count >= 1)
        {
            return (false, "El plan Profesional Mensual incluye 1 centro. Actualiza al plan Multicentro para gestionar varios centros.", null);
        }

        var nuevoCentro = cargarPlantillaExcel
            ? ExcelTemplateSeeder.CrearCentroDesdeExcel(usuario.Id, nombre.Trim(), direccion.Trim())
            : new CentroEstetica
            {
                UsuarioId = usuario.Id,
                Nombre = nombre.Trim(),
                Direccion = direccion.Trim(),
                Iva = 0.21m,
                Beneficio = 0.50m
            };

        db.Centros.Add(nuevoCentro);
        await db.SaveChangesAsync();

        CentroActualId = nuevoCentro.Id;
        NotificarCambio();
        return (true, $"Centro '{nuevoCentro.Nombre}' creado correctamente.", nuevoCentro);
    }

    public async Task<(bool Exito, string Mensaje)> ProcesarPagoSuscripcionSimuladaAsync(PlanSuscripcion plan, string numeroTarjeta, string titular)
    {
        if (!UsuarioActualId.HasValue)
        {
            return (false, "Debes iniciar sesión para suscribirte.");
        }

        var digitos = new string((numeroTarjeta ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length < 4 || string.IsNullOrWhiteSpace(titular))
        {
            return (false, "Introduce un titular y un número de tarjeta válido (mínimo 4 dígitos para la simulación).");
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == UsuarioActualId.Value);
        if (usuario is null)
        {
            return (false, "Usuario no encontrado.");
        }

        var importe = plan == PlanSuscripcion.MulticentroAnual ? 290.00m : 29.00m;
        var ultimos4 = digitos[^4..];

        usuario.Plan = plan;
        usuario.SuscripcionActiva = true;
        usuario.MetodoPagoUltimos4 = ultimos4;
        usuario.FechaExpiracionSuscripcion = plan == PlanSuscripcion.MulticentroAnual
            ? DateTime.UtcNow.AddYears(1)
            : DateTime.UtcNow.AddMonths(1);

        var pago = new RegistroPago
        {
            UsuarioId = usuario.Id,
            Fecha = DateTime.UtcNow,
            Plan = plan,
            Importe = importe,
            ReferenciaTransaccion = $"MOCK-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Estado = "Completado"
        };

        db.Pagos.Add(pago);
        await db.SaveChangesAsync();

        NotificarCambio();
        return (true, $"Pago simulado procesado ({importe:N2} €). Suscripción {plan} activada.");
    }

    public async Task CancelarSuscripcionAsync()
    {
        if (!UsuarioActualId.HasValue)
        {
            return;
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == UsuarioActualId.Value);
        if (usuario is not null)
        {
            usuario.SuscripcionActiva = false;
            usuario.Plan = PlanSuscripcion.Ninguno;
            await db.SaveChangesAsync();
            NotificarCambio();
        }
    }

    public void NotificarCambio() => OnEstadoCambiado?.Invoke();
}

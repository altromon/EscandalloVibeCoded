namespace EscandalloWeb.Models;

public enum PlanSuscripcion
{
    Ninguno = 0,
    ProfesionalMensual = 1,
    MulticentroAnual = 2
}

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public PlanSuscripcion Plan { get; set; } = PlanSuscripcion.Ninguno;
    public bool SuscripcionActiva { get; set; }
    public DateTime? FechaExpiracionSuscripcion { get; set; }
    public string? MetodoPagoUltimos4 { get; set; }

    public List<CentroEstetica> Centros { get; set; } = new();
    public List<RegistroPago> Pagos { get; set; } = new();
}

public class RegistroPago
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public PlanSuscripcion Plan { get; set; }
    public decimal Importe { get; set; }
    public string ReferenciaTransaccion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Completado";
}

public class CentroEstetica
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    // Hoja "Datos"
    public decimal Iva { get; set; } = 0.21m;
    public decimal Beneficio { get; set; } = 0.50m;

    public List<GastoGeneral> GastosGenerales { get; set; } = new();
    public List<CategoriaPersonal> Personal { get; set; } = new();
    public List<Producto> Productos { get; set; } = new();
    public List<Servicio> Servicios { get; set; } = new();

    // SUBTOTAL(109, Tabla13[Precio Unitario])
    public decimal TotalPrecioUnitarioGastosGenerales =>
        GastosGenerales.Sum(g => g.PrecioUnitario);
}

public class GastoGeneral
{
    public int Id { get; set; }
    public int CentroEsteticaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 10560m; // 8 * 60 * 22
    public string UnidadMedida { get; set; } = "minutos/mes";
    public decimal CosteEmpresa { get; set; }

    // =E2/C2
    public decimal PrecioUnitario => Cantidad > 0 ? CosteEmpresa / Cantidad : 0m;
}

public class CategoriaPersonal
{
    public int Id { get; set; }
    public int CentroEsteticaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 10560m; // 8 * 60 * 22
    public string UnidadMedida { get; set; } = "minutos/mes";
    public decimal Neto { get; set; }
    public decimal Retenciones { get; set; } = 0.20m;

    // =((E2*F2)+E2)*1.5
    public decimal CosteEmpresa => ((Neto * Retenciones) + Neto) * 1.5m;

    // =G2/C2
    public decimal PrecioUnitario => Cantidad > 0 ? CosteEmpresa / Cantidad : 0m;
}

public class Producto
{
    public int Id { get; set; }
    public int CentroEsteticaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "ml";
    public decimal Precio { get; set; }

    // =E2/C2
    public decimal PrecioUnitario => Cantidad > 0 ? Precio / Cantidad : 0m;
}

public class Servicio
{
    public int Id { get; set; }
    public int CentroEsteticaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Minutos { get; set; }
    public int? CategoriaPersonalId { get; set; }
    public CategoriaPersonal? CategoriaPersonal { get; set; }

    // Hoja "PVR" columna F
    public decimal? Pvr { get; set; }

    public List<ServicioProducto> LineasProducto { get; set; } = new();

    // =Minutos * Tabla13[[#Totals],[Precio Unitario]]
    public decimal CalcularGastosGenerales(decimal totalUnitarioGastosGenerales) =>
        Minutos * totalUnitarioGastosGenerales;

    // =VLOOKUP(Persona, Tabla1, 7, FALSE) * Minutos
    public decimal CostePersona =>
        (CategoriaPersonal?.PrecioUnitario ?? 0m) * Minutos;

    // Suma de Coste 1..N
    public decimal CosteProductos =>
        LineasProducto.Sum(l => l.Coste);

    // =Gastos Generales + Coste Persona + Suma(Coste 1..N)
    public decimal CalcularEscandalloTotal(decimal totalUnitarioGastosGenerales) =>
        CalcularGastosGenerales(totalUnitarioGastosGenerales) + CostePersona + CosteProductos;

    // Hoja PVR Columna D ("Beneficio"): =Escandallo Total * Datos!$B$2 + Escandallo Total
    public decimal CalcularPrecioConBeneficio(decimal totalUnitarioGastosGenerales, decimal porcentajeBeneficio)
    {
        var escandallo = CalcularEscandalloTotal(totalUnitarioGastosGenerales);
        return (escandallo * porcentajeBeneficio) + escandallo;
    }

    // Hoja PVR Columna E ("IVA"): =Beneficio * Datos!$B$1 + Beneficio
    public decimal CalcularPrecioConIva(decimal totalUnitarioGastosGenerales, decimal porcentajeBeneficio, decimal porcentajeIva)
    {
        var conBeneficio = CalcularPrecioConBeneficio(totalUnitarioGastosGenerales, porcentajeBeneficio);
        return (conBeneficio * porcentajeIva) + conBeneficio;
    }
}

public class ServicioProducto
{
    public int Id { get; set; }
    public int ServicioId { get; set; }
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal Cantidad { get; set; }

    // =VLOOKUP(Producto N, Tabla3, 5, FALSE) * Cantidad N
    public decimal Coste => (Producto?.PrecioUnitario ?? 0m) * Cantidad;
}

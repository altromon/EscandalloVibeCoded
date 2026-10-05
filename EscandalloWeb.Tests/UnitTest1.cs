using EscandalloWeb.Data;
using EscandalloWeb.Models;
using EscandalloWeb.Services;
using Microsoft.EntityFrameworkCore;

namespace EscandalloWeb.Tests;

public class EscandalloExcelFormulaTests
{
    [Fact]
    public void FormulasExcel_CoincidenExactamenteConCalculadoraEscandalloXlsx()
    {
        var centro = ExcelTemplateSeeder.CrearCentroDesdeExcel(1, "Centro Test", "Calle Test");

        // 1. Gastos Generales: SUBTOTAL = (335 + 100 + 440) / 10560 = 0.08285984848484848...
        var totalGastosUnitario = centro.TotalPrecioUnitarioGastosGenerales;
        Assert.Equal(0.0828598484848485m, Math.Round(totalGastosUnitario, 16));

        // 2. Personal: Coste para empresa = ((Neto * Retenciones) + Neto) * 1.5
        var tecnico = centro.Personal.Single(p => p.Nombre == "Técnico");
        Assert.Equal(1980m, tecnico.CosteEmpresa);
        Assert.Equal(0.1875m, tecnico.PrecioUnitario);

        var encargado = centro.Personal.Single(p => p.Nombre == "Encargado");
        Assert.Equal(2880m, encargado.CosteEmpresa);
        Assert.Equal(0.2727272727272727m, Math.Round(encargado.PrecioUnitario, 16));

        var doctor = centro.Personal.Single(p => p.Nombre == "Doctor");
        Assert.Equal(5625m, doctor.CosteEmpresa);
        Assert.Equal(0.5326704545454545m, Math.Round(doctor.PrecioUnitario, 16));

        // 3. Servicio "Mesodermolifting" (Fila 3 de Servicios y PVR)
        var mesodermo = centro.Servicios.Single(s => s.Nombre == "Mesodermolifting");
        Assert.Equal(4.97159090909091m, Math.Round(mesodermo.CalcularGastosGenerales(totalGastosUnitario), 14));
        Assert.Equal(16.36363636363636m, Math.Round(mesodermo.CostePersona, 14));
        Assert.Equal(34.71245727272727m, Math.Round(mesodermo.CalcularEscandalloTotal(totalGastosUnitario), 14));
        Assert.Equal(52.06868590909091m, Math.Round(mesodermo.CalcularPrecioConBeneficio(totalGastosUnitario, centro.Beneficio), 14));
        Assert.Equal(63.00310995000000m, Math.Round(mesodermo.CalcularPrecioConIva(totalGastosUnitario, centro.Beneficio, centro.Iva), 14));
        Assert.Equal(100m, mesodermo.Pvr);

        // 4. Servicio "Prueba" (Fila 2 de Servicios y PVR)
        var prueba = centro.Servicios.Single(s => s.Nombre == "Prueba");
        Assert.Equal(85.72473090909091m, Math.Round(prueba.CalcularEscandalloTotal(totalGastosUnitario), 14));
        Assert.Equal(128.58709636363636m, Math.Round(prueba.CalcularPrecioConBeneficio(totalGastosUnitario, centro.Beneficio), 14));
        Assert.Equal(155.59038660000000m, Math.Round(prueba.CalcularPrecioConIva(totalGastosUnitario, centro.Beneficio, centro.Iva), 14));
    }

    [Fact]
    public async Task FlujoMultiTenantYMultiCentro_ConSuscripcionSimulada_FuncionaCorrectamente()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var factory = new TestDbContextFactory(options);
        var sesion = new SesionUsuarioService(factory);

        // Registro de nuevo cliente con primer centro
        var reg = await sesion.RegistrarUsuarioAsync("Clínica Sol", "sol@clinica.es", "secret123", "Sol Centro 1", cargarPlantillaExcel: true);
        Assert.True(reg.Exito);

        var usuario = await sesion.ObtenerUsuarioActualAsync();
        Assert.NotNull(usuario);
        Assert.False(usuario.SuscripcionActiva);
        Assert.Single(usuario.Centros);

        // Sin suscripción activa no puede añadir un segundo centro
        var intentoSinSub = await sesion.CrearCentroAsync("Sol Centro 2", "Av. Libertad 10", false);
        Assert.False(intentoSinSub.Exito);

        // Activar plan ProfesionalMensual (límite 1 centro)
        var pagoMensual = await sesion.ProcesarPagoSuscripcionSimuladaAsync(PlanSuscripcion.ProfesionalMensual, "4111222233334444", "Clínica Sol");
        Assert.True(pagoMensual.Exito);

        var intentoConPlanBasico = await sesion.CrearCentroAsync("Sol Centro 2", "Av. Libertad 10", false);
        Assert.False(intentoConPlanBasico.Exito);

        // Actualizar a Plan MulticentroAnual y crear segundo centro
        var pagoMulti = await sesion.ProcesarPagoSuscripcionSimuladaAsync(PlanSuscripcion.MulticentroAnual, "4111222233339999", "Clínica Sol");
        Assert.True(pagoMulti.Exito);

        var creacionCentro2 = await sesion.CrearCentroAsync("Sol Centro 2", "Av. Libertad 10", cargarPlantillaExcel: false);
        Assert.True(creacionCentro2.Exito);

        usuario = await sesion.ObtenerUsuarioActualAsync();
        Assert.Equal(2, usuario!.Centros.Count);
        Assert.Equal(2, usuario.Pagos.Count);

        // Con sesión iniciada, el cliente no puede crear cuentas para otros clientes
        var intentoOtroClienteConLogin = await sesion.RegistrarUsuarioAsync("Otro Cliente", "otro@clinica.es", "secret123", "Otro Centro", false);
        Assert.False(intentoOtroClienteConLogin.Exito);

        // Tras cerrar sesión, queda sin usuario activo y puede iniciarse o crearse otra cuenta
        sesion.CerrarSesion();
        Assert.Null(await sesion.ObtenerUsuarioActualAsync());
    }

    [Fact]
    public void ExportadorEscandallo_GeneraExcelCompatibleYPdfValido()
    {
        var centro = ExcelTemplateSeeder.CrearCentroDesdeExcel(1, "Centro Export", "Calle Mayor 1");

        // 1. Verificar exportación Excel (.xlsx) compatible con CalculadoraEscandallo
        var excelBytes = ExportadorEscandallo.GenerarExcel(centro);
        Assert.NotEmpty(excelBytes);

        using var ms = new MemoryStream(excelBytes);
        using var wb = new ClosedXML.Excel.XLWorkbook(ms);

        string[] hojasEsperadas = ["PVR", "Servicios", "Productos", "Personal", "Gastos Generales", "Datos"];
        Assert.Equal(hojasEsperadas, wb.Worksheets.Select(w => w.Name).ToArray());

        // Verificar que las fórmulas en Servicios y PVR evalúan al mismo resultado que CalculadoraEscandallo.xlsx
        var wsPvr = wb.Worksheet("PVR");
        Assert.Equal("Mesodermolifting", wsPvr.Cell(2, 2).GetString());
        Assert.Equal(34.712457m, Math.Round((decimal)wsPvr.Cell(2, 3).GetDouble(), 6));
        Assert.Equal(52.068686m, Math.Round((decimal)wsPvr.Cell(2, 4).GetDouble(), 6));
        Assert.Equal(63.003110m, Math.Round((decimal)wsPvr.Cell(2, 5).GetDouble(), 6));
        Assert.Equal(100m, (decimal)wsPvr.Cell(2, 6).GetDouble());

        // 2. Verificar exportación PDF con cabecera %PDF-
        var pdfBytes = ExportadorEscandallo.GenerarPdf(centro);
        Assert.True(pdfBytes.Length > 1000);
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 5);
        Assert.Equal("%PDF-", header);
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
    }
}
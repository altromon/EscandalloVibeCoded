using EscandalloWeb.Models;

namespace EscandalloWeb.Data;

public static class ExcelTemplateSeeder
{
    public static CentroEstetica CrearCentroDesdeExcel(int usuarioId, string nombreCentro, string direccion)
    {
        var centro = new CentroEstetica
        {
            UsuarioId = usuarioId,
            Nombre = nombreCentro,
            Direccion = direccion,
            Iva = 0.21m,
            Beneficio = 0.50m
        };

        PoblarDatosExcelEnCentro(centro);
        return centro;
    }

    public static void PoblarDatosExcelEnCentro(CentroEstetica centro)
    {
        centro.Iva = 0.21m;
        centro.Beneficio = 0.50m;

        // Hoja "Gastos Generales" (Tabla13)
        centro.GastosGenerales =
        [
            new GastoGeneral { Nombre = "Local", Cantidad = 10560m, UnidadMedida = "minutos/mes", CosteEmpresa = 335m },
            new GastoGeneral { Nombre = "Consumos", Cantidad = 10560m, UnidadMedida = "minutos/mes", CosteEmpresa = 100m },
            new GastoGeneral { Nombre = "Consumibles", Cantidad = 10560m, UnidadMedida = "minutos/mes", CosteEmpresa = 440m } // 20*22
        ];

        // Hoja "Personal" (Tabla1)
        var tecnico = new CategoriaPersonal { Nombre = "Técnico", Cantidad = 10560m, UnidadMedida = "minutos/mes", Neto = 1100m, Retenciones = 0.20m };
        var tecnicoSup = new CategoriaPersonal { Nombre = "Técnico Superior", Cantidad = 10560m, UnidadMedida = "minutos/mes", Neto = 1300m, Retenciones = 0.20m };
        var encargado = new CategoriaPersonal { Nombre = "Encargado", Cantidad = 10560m, UnidadMedida = "minutos/mes", Neto = 1600m, Retenciones = 0.20m };
        var enfermero = new CategoriaPersonal { Nombre = "Enfermero", Cantidad = 10560m, UnidadMedida = "minutos/mes", Neto = 1800m, Retenciones = 0.22m };
        var doctor = new CategoriaPersonal { Nombre = "Doctor", Cantidad = 10560m, UnidadMedida = "minutos/mes", Neto = 3000m, Retenciones = 0.25m };

        centro.Personal = [tecnico, tecnicoSup, encargado, enfermero, doctor];

        // Hoja "Productos" (Tabla3)
        var pCleasingMilk = new Producto { Nombre = "Cleasing Milk", Cantidad = 500m, UnidadMedida = "ml", Precio = 34.51m };
        var pFacialTonic = new Producto { Nombre = "Facial Tonic", Cantidad = 500m, UnidadMedida = "ml", Precio = 38.50m };
        var pSalycili = new Producto { Nombre = "Salycili Removing", Cantidad = 50m, UnidadMedida = "ml", Precio = 35.29m };
        var pLiftingPeeling = new Producto { Nombre = "Lifting Peeling", Cantidad = 100m, UnidadMedida = "ml", Precio = 111.78m };
        var pNeutralizante = new Producto { Nombre = "Neutralizante", Cantidad = 200m, UnidadMedida = "ml", Precio = 27.28m };
        var pAceiteArgan = new Producto { Nombre = "Aceite de Argán", Cantidad = 20m, UnidadMedida = "ml", Precio = 60.29m };
        var pColagenMask = new Producto { Nombre = "Colagen Mask", Cantidad = 12m, UnidadMedida = "ud", Precio = 86.52m };
        var pHydraFace = new Producto { Nombre = "HydraFace", Cantidad = 500m, UnidadMedida = "ml", Precio = 109.10m };
        var pProteccionSolar = new Producto { Nombre = "Protección Solar", Cantidad = 200m, UnidadMedida = "ml", Precio = 66.07m };
        var pOxigel = new Producto { Nombre = "Oxigel", Cantidad = 125m, UnidadMedida = "ml", Precio = 22.95m };
        var pGammaLift = new Producto { Nombre = "Gamma lift", Cantidad = 50m, UnidadMedida = "ml", Precio = 190.00m };
        var pChloroAntiox = new Producto { Nombre = "Chloro Antiox", Cantidad = 50m, UnidadMedida = "ml", Precio = 190.00m };
        var pSiliconOrganic = new Producto { Nombre = "Silicon organic", Cantidad = 100m, UnidadMedida = "ml", Precio = 29.52m };
        var pDmae = new Producto { Nombre = "DMAE", Cantidad = 100m, UnidadMedida = "ml", Precio = 42.20m };
        var pLaureth = new Producto { Nombre = "Laureth", Cantidad = 40m, UnidadMedida = "ml", Precio = 51.33m };
        var pCollagenPyruvate = new Producto { Nombre = "Collagen Pyruvate", Cantidad = 40m, UnidadMedida = "ml", Precio = 47.82m };
        var pDexpanthenol = new Producto { Nombre = "Dexpanthenol", Cantidad = 100m, UnidadMedida = "ml", Precio = 51.66m };
        var pAdn = new Producto { Nombre = "ADN", Cantidad = 100m, UnidadMedida = "ml", Precio = 111.63m };
        var pRecambiosPen = new Producto { Nombre = "Recambios pen", Cantidad = 1m, UnidadMedida = "ud", Precio = 8.00m };

        centro.Productos =
        [
            pCleasingMilk, pFacialTonic, pSalycili, pLiftingPeeling, pNeutralizante,
            pAceiteArgan, pColagenMask, pHydraFace, pProteccionSolar, pOxigel,
            pGammaLift, pChloroAntiox, pSiliconOrganic, pDmae, pLaureth,
            pCollagenPyruvate, pDexpanthenol, pAdn, pRecambiosPen
        ];

        // Hoja "Servicios" + "PVR": Servicio 1 = Mesodermolifting
        var mesodermolifting = new Servicio
        {
            Nombre = "Mesodermolifting",
            Minutos = 60m,
            CategoriaPersonal = encargado,
            Pvr = 100m,
            LineasProducto =
            [
                new ServicioProducto { Producto = pCleasingMilk, Cantidad = 4m },
                new ServicioProducto { Producto = pFacialTonic, Cantidad = 2m },
                new ServicioProducto { Producto = pSalycili, Cantidad = 1m },
                new ServicioProducto { Producto = pGammaLift, Cantidad = 2m },
                new ServicioProducto { Producto = pDexpanthenol, Cantidad = 2.5m },
                new ServicioProducto { Producto = pDmae, Cantidad = 2.5m },
                new ServicioProducto { Producto = pCollagenPyruvate, Cantidad = 1m },
                new ServicioProducto { Producto = pHydraFace, Cantidad = 1m },
                new ServicioProducto { Producto = pProteccionSolar, Cantidad = 1m },
                new ServicioProducto { Producto = pOxigel, Cantidad = 3m }
            ]
        };

        // Hoja "Servicios" + "PVR": Servicio 0 = Prueba (30 líneas de Cleasing Milk)
        var cantidadesPrueba = new decimal[]
        {
            20m, 21m, 21m, 22m, 23m, 24m, 25m, 26m, 27m, 28m,
            29m, 30m, 31m, 32m, 33m, 34m, 35m, 36m, 37m, 38m,
            39m, 40m, 41m, 42m, 43m, 44m, 45m, 46m, 47m, 48m
        };

        var prueba = new Servicio
        {
            Nombre = "Prueba",
            Minutos = 60m,
            CategoriaPersonal = tecnico,
            Pvr = null,
            LineasProducto = cantidadesPrueba
                .Select(cant => new ServicioProducto { Producto = pCleasingMilk, Cantidad = cant })
                .ToList()
        };

        centro.Servicios = [mesodermolifting, prueba];
    }

    public static void InicializarBaseDeDatos(AppDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Usuarios.Any())
        {
            return;
        }

        var demoUser = new Usuario
        {
            Nombre = "Clínicas Estéticas Demo S.L.",
            Email = "demo@clinicaestetica.es",
            PasswordHash = "demo123",
            Plan = PlanSuscripcion.MulticentroAnual,
            SuscripcionActiva = true,
            FechaExpiracionSuscripcion = DateTime.UtcNow.AddYears(1),
            MetodoPagoUltimos4 = "4242",
            Pagos =
            [
                new RegistroPago
                {
                    Fecha = DateTime.UtcNow.AddDays(-5),
                    Plan = PlanSuscripcion.MulticentroAnual,
                    Importe = 290.00m,
                    ReferenciaTransaccion = "MOCK-SUB-0001",
                    Estado = "Completado"
                }
            ]
        };

        db.Usuarios.Add(demoUser);
        db.SaveChanges();

        var centroPrincipal = CrearCentroDesdeExcel(demoUser.Id, "Centro Principal - Madrid", "Calle Serrano 45, Madrid");
        var centroSecundario = CrearCentroDesdeExcel(demoUser.Id, "Sucursal Norte - Barcelona", "Passeig de Gràcia 88, Barcelona");

        db.Centros.AddRange(centroPrincipal, centroSecundario);
        db.SaveChanges();
    }
}

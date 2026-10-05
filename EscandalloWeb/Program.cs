using EscandalloWeb.Components;
using EscandalloWeb.Data;
using EscandalloWeb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=escandallo.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<SesionUsuarioService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = dbFactory.CreateDbContext();
    ExcelTemplateSeeder.InicializarBaseDeDatos(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGet("/api/export/excel/{centroId:int}", async (int centroId, IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var centro = await db.Centros
        .Include(c => c.GastosGenerales)
        .Include(c => c.Personal)
        .Include(c => c.Productos)
        .Include(c => c.Servicios)
            .ThenInclude(s => s.CategoriaPersonal)
        .Include(c => c.Servicios)
            .ThenInclude(s => s.LineasProducto)
                .ThenInclude(lp => lp.Producto)
        .FirstOrDefaultAsync(c => c.Id == centroId);

    if (centro is null)
        return Results.NotFound();

    var bytes = ExportadorEscandallo.GenerarExcel(centro);
    return Results.File(
        bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"CalculadoraEscandallo_{centro.Nombre.Replace(' ', '_')}.xlsx");
});

app.MapGet("/api/export/pdf/{centroId:int}", async (int centroId, IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var centro = await db.Centros
        .Include(c => c.GastosGenerales)
        .Include(c => c.Personal)
        .Include(c => c.Productos)
        .Include(c => c.Servicios)
            .ThenInclude(s => s.CategoriaPersonal)
        .Include(c => c.Servicios)
            .ThenInclude(s => s.LineasProducto)
                .ThenInclude(lp => lp.Producto)
        .FirstOrDefaultAsync(c => c.Id == centroId);

    if (centro is null)
        return Results.NotFound();

    var bytes = ExportadorEscandallo.GenerarPdf(centro);
    return Results.File(
        bytes,
        "application/pdf",
        $"InformeEscandallo_{centro.Nombre.Replace(' ', '_')}.pdf");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

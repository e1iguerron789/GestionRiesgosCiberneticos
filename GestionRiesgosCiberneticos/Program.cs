using CyberRiskManager.Data;
using CyberRiskManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios  
builder.Services.AddSingleton<IAService>();
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<MongoService>();

var app = builder.Build();

// Mover esta línea después de la creación de 'app'  
var mongoTest = app.Services.GetRequiredService<MongoService>();
Console.WriteLine($"Activos encontrados: {mongoTest.GetAll().Count}");

builder.Services.AddLogging();

// Pipeline  
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Activos}/{action=Index}/{id?}");

app.Run();

using CyberRiskManager.Data;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoService>();
builder.Services.AddLogging();

var app = builder.Build();

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

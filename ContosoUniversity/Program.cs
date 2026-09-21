using ContosoUniversity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSystemWebAdapters();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseSystemWebAdapters();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

// Uploads/TeachingMaterials lives at the app root, not wwwroot, so it needs its own static file mapping
// (parity with the classic Server.MapPath("~/Uploads/...") used to save/serve teaching material images).
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/Uploads"
});

app.UseRouting();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

InitializeDatabase();

app.Run();

void InitializeDatabase()
{
    using var context = SchoolContextFactory.Create();
    DbInitializer.Initialize(context);
}

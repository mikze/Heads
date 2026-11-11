using GameKit.Hosting;
using Glowki;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Każda gra to osobna biblioteka. Nową dodaje się tutaj (pierwsza jest domyślna).
builder.Services.AddGameKit()
    .AddGame<GlowkiGame>();

// Front w trybie deweloperskim (next dev) działa na innym porcie, więc musi móc się połączyć.
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var app = builder.Build();

app.UseCors();

// Zbudowany front (npm run build w Web/) serwujemy z tego samego adresu co serwer gry.
var webRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, app.Configuration["WebRoot"] ?? "../Web/out"));
if (Directory.Exists(webRoot))
{
    var files = new PhysicalFileProvider(webRoot);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
    Console.WriteLine($"Front serwowany z {webRoot}");
}
else
{
    Console.WriteLine($"Brak zbudowanego frontu w {webRoot} — uruchom 'npm run dev' w katalogu Web.");
}

app.MapGameKit("/game");

app.Run();

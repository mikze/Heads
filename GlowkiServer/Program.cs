using GlowkiServer.Hubs;
using GlowkiServer.Lobby;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<RoomManager>();

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

app.MapHub<GameHub>("/game");

app.Run();

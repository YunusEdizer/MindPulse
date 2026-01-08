using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using MindPulse.API.Data;

var builder = WebApplication.CreateBuilder(args);

// CORS servisini ekle (Her yerden gelen iste�i kabul et - Geli�tirme a�amas� i�in)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

// Veritaban� Servisini ekliyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Gerekli k�t�phaneyi eklemeyi unutma: 
// using Microsoft.AspNetCore.StaticFiles; (En tepeye ekle)
app.UseBlazorFrameworkFiles();

var provider = new FileExtensionContentTypeProvider();
// Bilinmeyen dosya t�rlerini elle tan�t�yoruz
provider.Mappings[".blat"] = "application/octet-stream";
provider.Mappings[".dat"] = "application/octet-stream";
provider.Mappings[".dll"] = "application/octet-stream";
provider.Mappings[".json"] = "application/json";
provider.Mappings[".wasm"] = "application/wasm";
provider.Mappings[".woff"] = "application/font-woff";
provider.Mappings[".woff2"] = "application/font-woff2";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider,
    ServeUnknownFileTypes = true, // G�venlik duvar�n� delip ge�er
    DefaultContentType = "application/octet-stream" // Ne oldu�unu anlamazsa dosya olarak ver
});
app.MapFallbackToFile("index.html"); // 2. Siteye gireni ana sayfaya y�nlendirir

app.Run();

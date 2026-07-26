using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MindPulse.UI;
using MindPulse.UI.Services; // AppState'in bulundu�u namespace'i ekleyin

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient kaydı — API adresi wwwroot/appsettings.json içindeki "ApiBaseUrl"den okunur.
// Ayar yoksa yerel geliştirme adresine düşer (localhost).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    apiBaseUrl = "https://localhost:7132/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// --- YEN� EKLENEN SATIR ---
// AppState'i buraya kaydediyoruz
// AppState Singleton olarak kaydediliyor (Tüm sayfalar aynı instance'ı kullanır)
builder.Services.AddSingleton<AppState>();
// --------------------------

await builder.Build().RunAsync();
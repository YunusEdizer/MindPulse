using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MindPulse.UI;
using MindPulse.UI.Services; // AppState'in bulundu�u namespace'i ekleyin

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient Kayd�
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// --- YEN� EKLENEN SATIR ---
// AppState'i buraya kaydediyoruz
// AppState Singleton olarak kaydediliyor (Tüm sayfalar aynı instance'ı kullanır)
builder.Services.AddSingleton<AppState>();
// --------------------------

await builder.Build().RunAsync();
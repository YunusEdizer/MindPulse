using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MindPulse.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// API Adresimiz burasý (Swagger'daki adresin aynýsý)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7132/") });
await builder.Build().RunAsync();

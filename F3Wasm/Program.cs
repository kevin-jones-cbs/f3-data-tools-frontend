using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using F3Wasm;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Blazorise.Charts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

var lambdaUrl = builder.Configuration["LambdaUrl"] ?? "https://s6oww3m3a5svbuxq5pf35pjigu0xxaqk.lambda-url.us-west-1.on.aws/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(lambdaUrl) });

await builder.Build().RunAsync();

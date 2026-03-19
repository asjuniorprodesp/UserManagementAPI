using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagementUI.Models;
using UserManagementUI.Services;
using UserManagementUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7133/";
var apiToken = builder.Configuration["ApiSettings:Token"] ?? "techhive-api-key-2026";

builder.Services.AddScoped(_ =>
{
	var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
	client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);
	return client;
});
builder.Services.AddScoped<UserApiClient>();

await builder.Build().RunAsync();

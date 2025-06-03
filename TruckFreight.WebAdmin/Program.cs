using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TruckFreight.WebAdmin.Configuration;
using TruckFreight.WebAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Add Neshan Map Service
builder.Services.AddHttpClient<INeshanMapService, NeshanMapService>(client =>
{
    var neshanSettings = builder.Configuration.GetSection("AppSettings:Neshan").Get<NeshanSettings>();
    client.BaseAddress = new Uri(neshanSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(neshanSettings.Timeout);
});

// Add other services
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

using SENSORE_APP.Services;
using SENSORE_APP.Hubs;
using SENSORE_APP.Services.Factories;
using SENSORE_APP.Services.Strategies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HTTP Context Accessor for session access
builder.Services.AddHttpContextAccessor();

// Singleton Services
builder.Services.AddSingleton<SENSORE_APP.Services.PressureDataService>();
builder.Services.AddSingleton<SENSORE_APP.Services.HeatmapBroadcastService>();
builder.Services.AddSingleton<SENSORE_APP.Services.HeatmapCsvLoader>();

// Message Storage Service
builder.Services.AddScoped<SENSORE_APP.Services.MessageStorageService>();

// Factory Pattern (Creational)
builder.Services.AddSingleton<IMessageFactory, MessageFactory>();
builder.Services.AddSingleton<AlertMessageFactory>();

// Strategy Pattern (Behavioral)
builder.Services.AddSingleton<StrategyBasedRiskAnalyzer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Use Session middleware (must be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Patient}/{action=Index}/{id?}")
    .WithStaticAssets();

// Map SignalR hub
app.MapHub<SENSORE_APP.Hubs.PressureHub>("/pressureHub");

// Start pressure data service
var pressureService = app.Services.GetRequiredService<SENSORE_APP.Services.PressureDataService>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var cts = new CancellationTokenSource();

lifetime.ApplicationStarted.Register(() =>
{
    pressureService.StartLiveUpdates(cts.Token);
    // Force creation of broadcast service to activate event subscriptions
    _ = app.Services.GetRequiredService<SENSORE_APP.Services.HeatmapBroadcastService>();
});

lifetime.ApplicationStopping.Register(() =>
{
    cts.Cancel();
});

app.Run();

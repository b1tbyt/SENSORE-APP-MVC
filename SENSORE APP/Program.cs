var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SENSORE_APP.Services.PressureDataService>();
builder.Services.AddSingleton<SENSORE_APP.Services.HeatmapBroadcastService>();

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

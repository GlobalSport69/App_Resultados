using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using System.Reflection.PortableExecutable;
using LotteryResult.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LotteryResult;
using Serilog;
using Serilog.Events;
using LotteryResult.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Filters;
using LotteryResult.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add the processing server as IHostedService
//builder.Services.AddHangfireServer();
builder.Services.AddHangfireServer(x => {
    x.ServerName = string.Format("{0}:notify_premier", Environment.MachineName);
    x.Queues = new[] { "notify_premier" };
    x.WorkerCount = 1;
});
builder.Services.AddHangfireServer(x => {
    x.ServerName = string.Format("{0}:default", Environment.MachineName);
    x.Queues = new[] { "default" };
});


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/auth/login";
});

builder.Services.AddData(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
//builder.Logging.AddSerilog();


// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetSection("DB:Hangfire").Value)));


builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration
        .WriteTo.File(builder.Configuration.GetSection("Serilog:LogPath").Value, 
            outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] [{ThreadId}] {Message}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day)
        .WriteTo.File(builder.Configuration.GetSection("Serilog:ErrorLogPath").Value,
            outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{ThreadId}] {Message}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day);

    //configuration.WriteTo.Console(outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] [{ThreadId}] {Message}{NewLine}{Exception}");

    configuration
    .WriteTo.GrafanaLoki(builder.Configuration.GetSection("Serilog:LokiUrl").Value,
                new List<LokiLabel> { 
                    new() { Key = "app", Value = "info" } 
                },
            restrictedToMinimumLevel: LogEventLevel.Information)
    .WriteTo.GrafanaLoki(builder.Configuration.GetSection("Serilog:LokiUrl").Value,
             new List<LokiLabel> {
                new() { Key = "app", Value = "error" }
             },
             restrictedToMinimumLevel: LogEventLevel.Error)
    .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.FromSource<NotifyPremierService>())
            .WriteTo.GrafanaLoki(builder.Configuration.GetSection("Serilog:LokiUrl").Value,
            new List<LokiLabel> {
                new() { Key = "app", Value = "premiacion_log" }
            }));
});

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

//app.UseHangfireDashboard();
var options = new DashboardOptions()
{
    Authorization = new[] { new MyHangfireAuthorizationFilter() }
};

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire/index", options);

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

//app.MapControllerRoute(
//    name: "login",
//    pattern: "login",
//    defaults: new { controller = "Auth", action = "Login" });

app.Run();

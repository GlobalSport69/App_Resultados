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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddData(builder.Configuration);
builder.Services.AddServices(builder.Configuration);
builder.Services.AddLogging(builder => builder.AddSerilog());


// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetSection("DB:Hangfire").Value)));


builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration
        .WriteTo.File(builder.Configuration.GetSection("Serilog:LogPath").Value, rollingInterval: RollingInterval.Day)
        .WriteTo.File(builder.Configuration.GetSection("Serilog:ErrorLogPath").Value,
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day);
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

app.UseHangfireDashboard();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using System.Reflection.PortableExecutable;
using LotteryResult.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LotteryResult;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

builder.Services.AddData(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(builder.Configuration.GetSection("DB:Hangfire").Value)));


var app = builder.Build();



//using (var serviceScope = app.Services.CreateScope())
//{
//    var services = serviceScope.ServiceProvider;
//    var jobs = services.GetRequiredService<IBootstrapJobs>();
//    jobs.Init();
//}



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

using LotteryResult.Data.Abstractions;
using LotteryResult.Data.Context;
using LotteryResult.Data.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotteryResult.Data
{
    public static class DataExtension
    {
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IResultRepository, ResultRepository>();

            ConnectionConfiguration(services, configuration);
            return services;
        }

        private static void ConnectionConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            string connection = configuration.GetSection("DB:Data").Value;

            var builder = new NpgsqlDataSourceBuilder(connection).Build();
            services.AddDbContext<PostgresDbContext>(options =>
            {
                options.UseNpgsql(builder);
            });
        }
    }
}

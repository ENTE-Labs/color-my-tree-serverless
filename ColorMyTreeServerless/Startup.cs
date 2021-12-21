using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMyTree.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(ColorMyTree.Startup))]
namespace ColorMyTree
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseCosmos(builder.GetContext().Configuration.GetConnectionString("DefaultConnection"), builder.GetContext().Configuration["DatabaseName"]);
            });
            builder.Services.AddScoped<Storage>();

            builder.Services.AddSingleton<CacheConnectionService>();
            builder.Services.AddSingleton<CacheService>();
        }
    }
}

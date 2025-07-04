using System;
using Microsoft.Extensions.Configuration;

namespace OrchidsShop.DAL;

public static class Utils
{
    public static string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:DBDefault"];
    }
}

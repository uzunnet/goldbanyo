using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Desadoor.Api.AraYazilimlar;

public static class RateLimitingYapilandirmasi
{
    public static IServiceCollection RateLimitingEkle(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("Genel", opt =>
            {
                opt.PermitLimit = 1000;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });

            options.AddFixedWindowLimiter("Giris", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            options.RejectionStatusCode = 429;
        });

        return services;
    }
}

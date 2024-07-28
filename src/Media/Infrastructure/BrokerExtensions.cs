using MassTransit;

namespace Media.Infrastructure;

public static class BrokerExtensions
{
    public static void BrokerConfigure(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(configure =>
        {

        });
    }
}

﻿using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Consumer;
using Crm.Link.RabbitMq.Producer;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

namespace Crm.Link.RabbitMq.Configuration
{
    public static class ConsumerConfiguration
    {
        public static IServiceCollection StartConsumers(this IServiceCollection services, string connectionstring)
        {
            services.AddSingleton<IConnectionFactory>(serviceProvider =>
            {
                var uri = new Uri(connectionstring);
                return new ConnectionFactory
                {
                    //Uri = uri,
                    DispatchConsumersAsync = true,
                    HostName = "rabbitmq"
                };
            });

            services.AddSingleton<ConnectionProvider>();

            services.AddHostedService<PlanningAttendeeConsumer>();
            services.AddHostedService<PlanningSessionAttendeeConsumer>();
            services.AddHostedService<PlanningSessionConsumer>();

            return services;
        }

        public static IServiceCollection AddPublishers(this IServiceCollection services)
        {
            services.AddSingleton<PlanningAttendeePublisher>();
            services.AddSingleton<PlanningSessionAttendeePublisher>();
            services.AddSingleton<PlanningSessionPublisher>();

            return services;
        }
    }
}

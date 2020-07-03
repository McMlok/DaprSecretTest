using System;
using System.Collections.Generic;
using System.Text.Json;
using Dapr.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DaprK8sSecretsTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // Create Dapr Client
            var client = new DaprClientBuilder().Build();

            // Add the DaprClient to DI.
            services.AddSingleton(client);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DaprClient client)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    var secretValues = await client.GetSecretAsync(
                        "kubernetes", // Name of the Dapr Secret Store
                        "super-secret", // Name of the k8s secret
                        new Dictionary<string, string>()
                        {
                            {"namespace", "default"}
                        }); // Namespace where the k8s secret is deployed

                    // Get the secret value
                    var secretValue = secretValues["super-secret"];

                    context.Response.ContentType = "application/json";
                    await JsonSerializer.SerializeAsync(context.Response.Body, secretValue);
                });
            });
        }
    }
}

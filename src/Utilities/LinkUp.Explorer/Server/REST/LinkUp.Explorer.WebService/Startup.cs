﻿using LinkUp.Explorer.WebService.Repositories;
using LinkUp.Node;
using LinkUp.Raw;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LinkUp.Explorer.WebService
{
    public class Startup
    {
        private LinkUpNode _Node = new LinkUpNode();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            _Node.Name = "api";
            _Node.AddSubNode(new LinkUpUdpConnector(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1"), 2000, 1000));

            services.AddSingleton<IConnectorRepository>(new ConnectorRepository(_Node));
            services.AddSingleton<INodeRepository>(new NodeRepository(_Node));
            services.AddSingleton<ILabelRepository>(new LabelRepository(_Node));
        }
    }
}
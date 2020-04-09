using Autofac;
using Autofac.Extensions.DependencyInjection;
using Covid.Api.Swagger;
using Covid.Common.Mapper;
using Covid.Repository;
using Covid.Repository.Facades;
using Covid.Repository.Factories;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Covid.Api
{
    public class Startup
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(Startup));

        // IContainer instance in the Startup class 
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                    .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver()) // ensures responses are PascalCase remove to camelCase
                    .AddControllersAsServices();

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.Conventions.Add(new VersionByNamespaceConvention());
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("v"));
            });
            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {

                    // note: need a temporary service provider here because one has not been created yet
                    var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                    //add a swagger document for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc($"/swagger/{description.GroupName}/swagger.json", new OpenApiInfo { Title = "My API", Version = description.ApiVersion.ToString() });
                    }

                    // add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    // integrate xml comments
                    options.IncludeXmlComments(XmlCommentsFilePath);
                });

            ConfigureModelBindingExceptionHandling(services);

            // get the connection string to the database
            var connectionString = Configuration.GetConnectionString("CovidDatabase");

            // load the assembly containing the mappers
            var executingAssembly = Assembly.Load("Covid.Api");

            // create a Autofac container builder
            var builder = new ContainerBuilder();

            // read service collection to Autofac
            builder.Populate(services);

            // configure the database options
            var optionsBuilder = new DbContextOptionsBuilder<CovidDbContext>();
            optionsBuilder
                .UseSqlServer(connectionString, providerOptions => providerOptions.CommandTimeout(60));

            // use and configure Autofac with custom registrations
            builder.Register(ctx => { return new CovidDbContextFactory(optionsBuilder.Options); }).As<ICovidDbContextFactory>().SingleInstance();
            builder.RegisterType<CovidRepositoryFacade>().As<ICovidRepositoryFacade>().SingleInstance();
            builder.Register(ctx => { return new Mapper(executingAssembly); }).As<IMapper>().SingleInstance();

            // build the Autofac container
            ApplicationContainer = builder.Build();

            // creating the IServiceProvider out of the Autofac container
            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var log4netConfig = Configuration.GetSection("log4net");
            loggerFactory.AddLog4Net(log4netConfig["ConfigFile"]);

            _logger.Info("test");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            // configure swagger
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    options.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
                });
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                return Path.Combine(AppContext.BaseDirectory, xmlFile);
            }
        }

        private void ConfigureModelBindingExceptionHandling(IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    ValidationProblemDetails error = actionContext.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .Select(e => new ValidationProblemDetails(actionContext.ModelState)).FirstOrDefault();

                    // Here you can add logging to you log file or to your Application Insights.
                    // For example, using Serilog:
                    // Log.Error($"{{@RequestPath}} received invalid message format: {{@Exception}}", 
                    //   actionContext.HttpContext.Request.Path.Value, 
                    //   error.Errors.Values);
                    return new BadRequestObjectResult(error);
                };
            });
        }
    }
}

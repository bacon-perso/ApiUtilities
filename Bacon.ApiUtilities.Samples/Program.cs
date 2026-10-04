using Bacon.ApiUtilities.Extensions;
using Bacon.ApiUtilities.Models.Apis.Documentations;
using Bacon.ApiUtilities.Samples.ProjectConfigs;
using Bacon.ApiUtilities.Samples.Resources;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console(Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Warning()
                    .CreateLogger();

ILoggerFactory loggerFactory = new LoggerFactory();
loggerFactory.AddSerilog();

bool isCrashed = false;
const string applicationName = "ApiUtilities Sample";

try
{
    #region Configure Services

    #region Cors

    List<string> internalUrl = [];
    if (webApplicationBuilder.Environment.IsDevelopment())
    {
        internalUrl.Add("https://localhost:7133");
        internalUrl.Add("http://localhost:5109");
    }

    //Add generic bluenext cors policy
    webApplicationBuilder.Services.AddCors(o =>
    {
        o.AddDefaultPolicy(
            builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
            });
        o.AddPolicy(
            name: "Internal",
            builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins([.. internalUrl]);
            }
            );
        o.AddPolicy(
            name: "CloudFlare",
            builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins(["https://www.cloudflare.com"]);
            }
            );
        o.AddPolicy(
            name: "Google",
            builder =>
            {
                builder.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
            }
            );
    });

    #endregion Cors

    #region Security

    webApplicationBuilder.Services.AddHsts(o =>
    {
        o.Preload = true;
        o.IncludeSubDomains = true;
        o.MaxAge = TimeSpan.FromSeconds(31536000);
    });

    webApplicationBuilder.Services.AddHttpsRedirection(o =>
    {
        o.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
    });

    #endregion Security

    #region Localization

    webApplicationBuilder.Services.Configure<RequestLocalizationOptions>(o =>
    {
        o.DefaultRequestCulture = new(culture: "en-US", uiCulture: "en-US");
        o.SupportedCultures = [new CultureInfo("en-US")];
        o.SupportedUICultures = [new CultureInfo("en-US")];
    });

    #endregion Localization

    #region Controllers

    //Add api versionning
    webApplicationBuilder.Services.AddApiVersioning(o =>
    {
        o.ReportApiVersions = true;
    })
        .AddMvc();

    webApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProblemDetailsWriter, ApiVersioningErrorResponseProvider>());
    webApplicationBuilder.Services.AddProblemDetails();

    webApplicationBuilder.Services.Configure<JsonOptions>(o =>
    {
        o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

    webApplicationBuilder.Services.AddControllers(o =>
    {
        o.RespectBrowserAcceptHeader = true;
        o.ReturnHttpNotAcceptable = true;
    })
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.CamelCase));
        })
        .AddApiExceptionHandler<ErrorCodeResources>(o =>
        {
            o.InternalServerErrorInternalErrorCode = 500000;
            o.UnhandledValidationInternalErrorCode = 412000;

            o.BuiltInValidationAttributeSettings = ApiExceptionHandlerSettings.GetBuiltInValidationAttributeSettings();
            o.CustomValidationAttributeSettings = ApiExceptionHandlerSettings.GetCustomValidationattributeSettings();
        })
        .AddApiRateLimiting(o =>
        {
            o.RateLimitInternalErrorCode = 429000;
        })
        .AddOpenApiDocumentation(o =>
        {
            ICollection<OpenApiDocumentInfo> openApiDocumentInfos =
            [
                new()
                {
                    Name = "sample1",
                    Title = "Sample API 1",
                    Version = "1",
                    License = new()
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://opensource.org/license/apache-2.0")
                    }
                }
            ];

            #if DEBUG
            openApiDocumentInfos.Add(new()
            {
                Name = "sample2",
                Title = "Sample API 2",
                Version = "1"
            });

            o.DisplayHiddenEndpoints = true;
            #else
            o.DisplayHiddenEndpoints = false;
            #endif

            o.OpenApiDocumentInfos = openApiDocumentInfos;
            o.OutputCacheDuration = TimeSpan.FromSeconds(0);
            o.UiConfigs.UiType = Bacon.ApiUtilities.Models.UiTypes.Swagger;
            o.UiConfigs.VirtualPath = string.Empty;
            o.UiConfigs.CorsPolicyCss = new()
            {
                { "Internal", "#FFAC1C" },
                { "CloudFlare", "#F38020"},
                { "Google", "#4285F4"}
            };
        });

    #endregion Controllers

    #region Authorization

    //Authentication provider setup
    webApplicationBuilder.Services.AddAuthorization();

    //JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
    //webApplicationBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //.AddJwtBearer(o =>
    //{
    //    o.Authority = webApplicationBuilder.Configuration.GetSection("InstancesSettings")["AuthorityUrl"]!.TrimEnd('/');
    //    o.RequireHttpsMetadata = true;
    //    o.TokenValidationParameters.ValidateAudience = false;
    //    o.TokenValidationParameters.RequireAudience = false;
    //    o.TokenValidationParameters.ValidateIssuerSigningKey = true;
    //});

    #endregion Authorization

    #endregion Configure Services

    #region Configure WebApp

    WebApplication webApplication = webApplicationBuilder.Build();

    if (webApplicationBuilder.Environment.IsDevelopment())
    {
        webApplication.UseDeveloperExceptionPage();
    }

    #region Security

    webApplication.UseHsts();

    webApplication.UseHttpsRedirection();

    webApplication.Use((context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        return next(context);
    });

    #endregion Security

    #region Static Files

    webApplication.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static files for 30 days
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
            ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(30).ToString("R", CultureInfo.InvariantCulture));
        }
    });

    #endregion Static Files

    webApplication.UseRouting();
    webApplication.UseCors();

    #region Localization

    webApplication.UseRequestLocalization();

    #endregion Localization

    webApplication.UseOutputCache(); //THIS MUST BE ADDED

    #region Exception handler

    webApplication.UseApiExceptionHandler();
    webApplication.UseStatusCodePages();

    #endregion Exception handler

    #region Swagger

    webApplication.UseOpenApiDocumentation();

    #endregion Swagger

    #region Controllers

    webApplication.UseAuthentication();
    webApplication.UseAuthorization();

    webApplication.UseRateLimiter(); //needed

    webApplication.MapControllers();

    #endregion Controllers

    //This will not be documented
    webApplication.MapGet("/test", () =>
    {
        return "test";
    })
        .WithName("Test")
        .WithTags("toto");


    await webApplication.RunAsync();

    #endregion Configure WebApp
}
catch (Exception ex)
{
    Log.Fatal(ex, $"{applicationName} : start-up failed");
    isCrashed = true;
}
finally
{
    Log.CloseAndFlush();

    if (isCrashed)
    {
        Environment.Exit(1);
    }
}

//https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-10.0
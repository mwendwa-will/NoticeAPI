
using Asp.Versioning;
using Microsoft.OpenApi.Models;
using NoticeAPI.Middleware;
using NoticeAPI.Repositories;
using NoticeAPI.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddScoped<IDbConnectionFactory, MariaDbConnectionFactory>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddMemoryCache();

// Configure API versioning with Asp.Versioning.Mvc
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V"; // 'v1' for version 1.0
    options.SubstituteApiVersionInUrl = true;
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ECommerce API",
        Version = "v1",
        Description = "API for managing an e-commerce product catalog"
    });

    // Ensure versioned endpoints are included
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out _)) return false;

        var versions = apiDesc.ActionDescriptor.EndpointMetadata
            .OfType<ApiVersionAttribute>()
            .SelectMany(a => a.Versions);

        return versions.Any(v => $"v{v.MajorVersion}" == docName);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API v1");
        c.RoutePrefix = "swagger"; // Explicitly set to /swagger (default)
    });
}

app.UseHttpsRedirection();
app.UseExceptionMiddleware();
app.UseApiKey();
app.MapControllers();

app.Run();
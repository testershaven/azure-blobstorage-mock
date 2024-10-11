using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Blobstorage.Mock.Models;
using Blobstorage.Mock.Services;
using Serilog;
using Serilog.Formatting.Compact;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();

builder.Host.UseSerilog((hostContext, loggingBuilder) =>
{
    loggingBuilder.Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "blobstorage-mock")
        .Enrich.WithProperty("ApplicationVersion", Assembly.GetExecutingAssembly()?.GetName()?.Version)
        .ReadFrom.Configuration(hostContext.Configuration)
        .WriteTo.Console(new CompactJsonFormatter());
});

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })

    .AddXmlSerializerFormatters()
    .AddXmlDataContractSerializerFormatters();

IConfigurationSection azureBlobStorageOptionsConfigSection = builder.Configuration.GetRequiredSection(AzureBlobStorageOptions.ConfigPath);
builder.Services.Configure<AzureBlobStorageOptions>(azureBlobStorageOptionsConfigSection);
AzureBlobStorageOptions azureBlobStorageOptions = azureBlobStorageOptionsConfigSection.Get<AzureBlobStorageOptions>()
    ?? throw new ArgumentNullException(nameof(AzureBlobStorageOptions));

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IPathBuilderService, PathBuilderService>();
builder.Services.AddTransient<IMockService, MockService>();

if (azureBlobStorageOptions.PerfTestModeEnabled)
{
    builder.Services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>(sp =>
    {
        BlobServiceClient serviceClient = new(azureBlobStorageOptions.ConnectionString);
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(azureBlobStorageOptions.ContainerName);

        return new InMemoryBlobStorageService(containerClient);
    });
}
else
{
    builder.Services.AddTransient<IBlobStorageService, AzureBlobStorageService>(sp =>
    {
        BlobServiceClient serviceClient = new(azureBlobStorageOptions.ConnectionString);
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(azureBlobStorageOptions.ContainerName);

        return new AzureBlobStorageService(containerClient);
    });
}

WebApplication app = builder.Build();

app.MapHealthChecks("/health");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

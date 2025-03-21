using CloudStore.BL;
using CloudStore.BL.BL.Validation;
using CloudStore.DAL;
using CloudStore.Hubs;
using CloudStore.WebApi.apiKeyValidation;
using CloudStore.WebApi.Helpers;

namespace CloudStore;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddTransient<CSFilesDbHelper>();
        builder.Services.AddTransient<CSUsersDbHelper>();
        builder.Services.AddTransient<CloudValidation>();
        builder.Services.AddTransient<HashHelper>();
        builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
        builder.Services.AddSignalR(hubOptions =>
        {
            hubOptions.EnableDetailedErrors = true;
            hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(1);
            hubOptions.ClientTimeoutInterval = TimeSpan.FromHours(1);
            hubOptions.HandshakeTimeout = TimeSpan.FromMinutes(30);
            hubOptions.MaximumReceiveMessageSize = int.MaxValue;
        });
        builder.Services.AddMemoryCache();

        var app = builder.Build();

        app.UseCors(builder =>
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    );
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<LargeFileHub>("cloud-store-api/large-file-hub");

        app.Run();
    }
}
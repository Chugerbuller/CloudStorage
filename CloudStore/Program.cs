using CloudStore.BL;
using CloudStore.BL.BL.Validation;
using CloudStore.DAL;
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

        app.Run();
    }
}
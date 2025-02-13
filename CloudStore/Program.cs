using CloudStore.BL;
using CloudStore.BL.BL.Validation;
using CloudStore.DAL;
using CloudStore.WebApi.Helpers;
using System.Security.Cryptography;

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
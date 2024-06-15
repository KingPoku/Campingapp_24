
using Campingapp_24.Data;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;


namespace Campingapp_24
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();


            // Register your database service
            builder.Services.AddSingleton<Database>(_ =>
            {
                // Connection string for MySQL
                string connectionString = "Server=127.0.0.1;Port=9419;Database=campingapp_24;Uid=root;Pwd=;";
                return new Database(connectionString);
            });


           


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use CORS middleware
            app.UseCors("AllowAll");

            // Enable static file serving
            app.UseStaticFiles();

            app.UseAuthorization();


            app.MapControllers();


            app.Run();

        }
    }
}

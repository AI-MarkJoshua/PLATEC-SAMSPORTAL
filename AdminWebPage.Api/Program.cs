using AdminWebPage.Shared.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AdminWebPageContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AdminWebPageContext")));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.WithOrigins("https://10.0.2.2:7298", "http://10.0.2.2:7298", "http://localhost:5156", "https://localhost:5156")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowMobileApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

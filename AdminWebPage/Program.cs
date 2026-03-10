using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AdminWebPage.Shared.Data;
using AdminWebPage.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AdminWebPageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AdminWebPageContext") ?? throw new InvalidOperationException("Connection string 'AdminWebPageContext' not found."), 
    b => b.MigrationsAssembly("AdminWebPage")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpClient(); // Add HttpClient for DI


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdminWebPageContext>();
    await DbInitializer.Initialize(context);
}

app.Run();

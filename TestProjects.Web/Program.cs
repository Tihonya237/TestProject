using Microsoft.EntityFrameworkCore;
using TestProjects.BL.Services;
using TestProjects.DAL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<EmployeeService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=project.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Projects}/{action=List}/{id?}")
    .WithStaticAssets();


app.Run();

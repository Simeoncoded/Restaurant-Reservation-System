using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReservationSystem.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("RestaurantReservationSystemContext") ?? throw new InvalidOperationException("Connection string 'RestaurantReservationSystemContext' not found.");

//var apiKey = Environment.GetEnvironmentVariable("OpenAI__ApiKey");
//if (string.IsNullOrEmpty(apiKey))
//{
//    throw new InvalidOperationException("OpenAI API Key is not set.");
//}

//builder.Services.AddSingleton(new OpenAI.OpenAIClient(Environment.GetEnvironmentVariable("OpenAI__ApiKey")));



builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDbContext<RestaurantReservationSystemContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ReservationRepository>(); // Register the repository for DI

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

//To prepare the database and seed data.  Can comment this out some of the time.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    RestaurantReservationSystemInitializer.Initialize(serviceProvider: services);
}


app.Run();

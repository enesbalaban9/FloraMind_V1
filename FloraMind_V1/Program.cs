using FloraMind_V1.Data;
using FloraMind_V1.Models;
using FloraMind_V1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<FloraMindDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.AccessDeniedPath = "/Account/AccessDenied"; 
    });




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseStaticFiles();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


//Seeding Admin Users

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FloraMindDbContext>();

    
    context.Database.EnsureCreated();

    string hashedPassword;

    
    string admin1Email = "e76443628@gmail.com";
    if (!context.Users.Any(u => u.Email == admin1Email))
    {
        string defaultPassword = "EnesAdmin!123";
        hashedPassword = FloraMind_V1.Helpers.SecurityHelper.HashPassword(defaultPassword);

        var EnesAdmin = new User
        {
            Name = "Enes Balaban",
            Email = admin1Email,
            PasswordHash = hashedPassword,
            Role = "Admin", 
            RegistrationDate = DateTime.UtcNow,
            IsBanned = false
        };

        context.Users.Add(EnesAdmin);
        Console.WriteLine($"Admin kullanýcýsý ({admin1Email}) oluþturuldu.");
    }

    
    string admin2Email = "ysnaydn4294@gmail.com";
    if (!context.Users.Any(u => u.Email == admin2Email))
    {
        string defaultPassword = "YasinAdmin!123";
        hashedPassword = FloraMind_V1.Helpers.SecurityHelper.HashPassword(defaultPassword);

        var testAdmin = new User
        {
            Name = "Yasin Aydýn",
            Email = admin2Email,
            PasswordHash = hashedPassword,
            Role = "Admin", 
            RegistrationDate = DateTime.UtcNow,
            IsBanned = false
        };

        context.Users.Add(testAdmin);
        Console.WriteLine($"Admin kullanýcýsý ({admin2Email}) oluþturuldu.");
    }

    context.SaveChanges();
}



// app.Run();


app.Run();

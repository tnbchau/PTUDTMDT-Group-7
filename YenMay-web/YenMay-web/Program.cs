using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories;
using YenMay_web.Repositories.Implementations;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Services;
using YenMay_web.Services.Interfaces;
using YenMay_web.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Session 7 ngày
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".YenMay.Session";
});



builder.Services.AddDbContext<YMDbContext>(options =>
    options.UseSqlServer(builder.Configuration.
    GetConnectionString("YenMayConnectionString")));

builder.Services
    .AddIdentity<User, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<YMDbContext>()
    .AddRoles<IdentityRole<int>>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); 

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        var googleAuthSection = builder.Configuration.GetSection("Authentication:Google");
        googleOptions.ClientId = googleAuthSection["ClientId"];
        googleOptions.ClientSecret = googleAuthSection["ClientSecret"];
    })
    .AddFacebook(facebookOptions =>
    {
        var facebookAuthSection = builder.Configuration.GetSection("Authentication:Facebook");
        facebookOptions.AppId = facebookAuthSection["AppId"];
        facebookOptions.AppSecret = facebookAuthSection["AppSecret"];
    });
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShippingRepository, ShippingRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

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
app.UseSession();
app.Use(async (context, next) =>
{
    await context.Session.LoadAsync();
    context.GetOrCreateCartSessionId();
    await next();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "article_details",
        pattern: "tin-tuc/{categorySlug}/{articleSlug}",
        defaults: new { controller = "Article", action = "Details" });

    endpoints.MapControllerRoute(
        name: "article_category",
        pattern: "tin-tuc/{categorySlug}",
        defaults: new { controller = "Article", action = "Index" });

    endpoints.MapControllerRoute(
        name: "article_index",
        pattern: "tin-tuc",
        defaults: new { controller = "Article", action = "Index" });

    endpoints.MapControllerRoute(
        name: "product-detail",
        pattern: "san-pham/{categorySlug}/{productSlug}",
        defaults: new { controller = "Product", action = "Detail" });

    endpoints.MapControllerRoute(
        name: "product-category",
        pattern: "san-pham/{categorySlug}",
        defaults: new { controller = "Product", action = "Index" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapControllerRoute(
    name: "about",
    pattern: "ve-chung-toi",
    defaults: new { controller = "About", action = "Index" }
);
});
await IdentitySeedData.SeedAsync(app.Services);
app.Run();

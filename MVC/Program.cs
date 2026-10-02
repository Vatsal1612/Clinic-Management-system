using MVC.BAL;
using LoginReg.BAL;
// using MVC.BAL;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<AuthHelper>();
builder.Services.AddScoped<UserHelper>();
builder.Services.AddScoped<ProfileHelper>();
builder.Services.AddScoped<AdminHelper>();

builder.Services.AddScoped<NpgsqlConnection>(opt =>
{
    var conn = opt.GetRequiredService<IConfiguration>()
                  .GetConnectionString("DefaultConnection");

    return new NpgsqlConnection(conn);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

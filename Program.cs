using EMS.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =============================
// Services
// =============================

builder.Services.AddSingleton<MongoDbService>();

builder.Services.AddControllersWithViews();

builder.Services.AddSession();

// =============================
// QuestPDF License
// =============================

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// =============================
// Configure HTTP Request Pipeline
// =============================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
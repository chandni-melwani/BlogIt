using BlogApp.Components;
using BlogApp.Data;
using BlogApp.Hubs;
using BlogApp.Services;
using Microsoft.AspNetCore.DataProtection;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddControllers();

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        builder.Configuration["Supabase:Url"]!,
        builder.Configuration["Supabase:AnonKey"]!,
        new Supabase.SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        }
    ));

builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddScoped<BlogRepository>();
builder.Services.AddScoped<ConnectionRepository>();
builder.Services.AddScoped<UserProfileRepository>();
builder.Services.AddScoped<EngagementRepository>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddScoped<SearchStateService>();

builder.Services.AddHttpClient<AiSummaryService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(
        new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
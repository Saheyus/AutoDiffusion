using Autodiffusion.Services;
using AutoDiffusion.Data;
using AutoDiffusion.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<RefreshService>();
builder.Services.AddSingleton<GptServiceClient>();
builder.Services.AddScoped<RandomWordService>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddScoped<INameService, NameService>();
builder.Services.AddScoped<ProbabilityService>();
builder.Services.AddScoped<FullNameService>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

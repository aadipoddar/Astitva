using Astitva.Shared.Services;
using Astitva.Web.Components;
using Astitva.Web.Services;

using AstitvaLibrary.DataAccess;

using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Secrets.SyncfusionLicense);

// Add services to the container.
builder.Services.AddSyncfusionBlazor()
	.AddRazorComponents()
	.AddInteractiveServerComponents();

// Add device-specific services used by the Astitva.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IUpdateService, UpdateService>();
builder.Services.AddSingleton<IVibrationService, VibrationService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddScoped<ISaveAndViewService, SaveAndViewService>();
builder.Services.AddScoped<ISoundService, SoundService>();
builder.Services.AddScoped<IDataStorageService, DataStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode()
	.AddAdditionalAssemblies(typeof(Astitva.Shared._Imports).Assembly);

app.Run();

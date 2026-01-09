using Herramientas;
using Microsoft.AspNetCore.ResponseCompression;
using pepeizqs_apps_blazor_web.Componentes;
using System.Globalization;
using System.IO.Compression;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var conexionTexto = builder.Configuration.GetConnectionString("pepeizqs_apps_webContextConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
Herramientas.BaseDatos.cadenaConexion = conexionTexto;

#region Compresion (Primero)

builder.Services.AddResponseCompression(options =>
{
	//options.Providers.Add<BrotliCompressionProvider>();
	options.Providers.Add<GzipCompressionProvider>();
	options.EnableForHttps = true;
});

//builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
//{
//	options.Level = CompressionLevel.Optimal;
//});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
	options.Level = CompressionLevel.Optimal;
});

#endregion

#region Optimizador

builder.Services.AddWebOptimizer(opciones =>
{
	opciones.AddCssBundle("/css/bundle.css", new NUglify.Css.CssSettings
	{
		CommentMode = NUglify.Css.CssComment.None,

	}, "lib/bootstrap/dist/css/bootstrap.min.css", "css/colores_texto.css", "css/principal.css", "css/site.css", "lib/font-awesome/css/all.css");

	opciones.AddJavaScriptBundle("/superjs.js", "lib/jquery/dist/jquery.min.js", "lib/bootstrap/dist/js/bootstrap.bundle.min.js", "js/site.js");
});

#endregion

#region Blazor

builder.Services.AddRazorComponents().AddInteractiveServerComponents(opciones =>
{
	opciones.DetailedErrors = true;
});

#endregion

#region Redireccionador

builder.Services.AddControllersWithViews();

#endregion

#region Decompilador

builder.Services.AddHttpClient<IDecompiladores, Decompiladores2>()
	.ConfigurePrimaryHttpMessageHandler(() =>
		new HttpClientHandler
		{
			AutomaticDecompression = System.Net.DecompressionMethods.GZip,
			MaxConnectionsPerServer = 2
		});

builder.Services.AddSingleton<IDecompiladores, Decompiladores2>();

#endregion

#region Tareas

builder.Services.Configure<HostOptions>(hostOptions =>
{
	hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddSingleton<Tareas.TareaGithub>();
builder.Services.AddSingleton<Tareas.TareaIndexNow>();

builder.Services.AddHostedService(provider => provider.GetRequiredService<Tareas.TareaGithub>());
builder.Services.AddHostedService(provider => provider.GetRequiredService<Tareas.TareaIndexNow>());

#endregion

#region Contexto para Idioma y User-Agent

builder.Services.AddHttpContextAccessor();

#endregion

#region Mejora velocidad carga

builder.Services.AddHsts(opciones =>
{
	opciones.Preload = true;
	opciones.IncludeSubDomains = true;
	opciones.MaxAge = TimeSpan.FromDays(730);
});

#endregion

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	//app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseDeveloperExceptionPage();
	app.UseHsts();
}

#region Compresion (Primero)

app.UseResponseCompression();

#endregion

#region Optimizador (Despues Compresion)

app.UseWebOptimizer();

#endregion

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode(opciones =>
{
	opciones.DisableWebSocketCompression = true;
});

app.UseAuthorization();

#region Redireccionador

app.MapControllers();

#endregion

app.UseStatusCodePagesWithRedirects("/error");

app.Run();

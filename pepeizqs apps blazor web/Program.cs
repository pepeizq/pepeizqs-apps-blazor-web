using Herramientas;
using Herramientas.Conexiones;
using Microsoft.AspNetCore.ResponseCompression;
using pepeizqs_apps_blazor_web.Componentes;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

Pepeizqapps.cadenaConexion = builder.Configuration.GetConnectionString("pepeizqs_apps_webContextConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
Pepedeals.cadenaConexion = builder.Configuration.GetConnectionString("pepeizqs_deals_webContextConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

#region Compresion (Primero)

builder.Services.AddResponseCompression(opciones =>
{
	opciones.Providers.Add<BrotliCompressionProvider>();
	opciones.Providers.Add<GzipCompressionProvider>();
	opciones.EnableForHttps = true;
	opciones.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
				new[] { "application/octet-stream", "application/rss+xml", "text/html", "text/css", "image/png", "image/x-icon", "text/javascript" });
});

builder.Services.Configure<GzipCompressionProviderOptions>(opciones =>
{
	opciones.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(opciones =>
{
	opciones.Level = CompressionLevel.Optimal;
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

#region Decir a Google que deje de indexar http

builder.Services.AddHttpsRedirection(opciones =>
{
	opciones.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
});

#endregion

var app = builder.Build();

#region Control inicial de peticiones

app.Use(async (contexto, siguiente) =>
{
	// Redireccionar www a no-www
	string host = contexto.Request.Host.Host;

	if (host.StartsWith("www."))
	{
		string nuevoHost = host.Substring(4);
		string scheme = contexto.Request.Scheme;
		PathString rutaBase = contexto.Request.PathBase;
		PathString rutaSlug = contexto.Request.Path;
		QueryString queryTexto = contexto.Request.QueryString;

		string nuevoEnlace = $"{scheme}://{nuevoHost}{rutaBase}{rutaSlug}{queryTexto}";

		contexto.Response.StatusCode = StatusCodes.Status301MovedPermanently;
		contexto.Response.Headers.Location = nuevoEnlace;
		return;
	}

	try
	{
		await siguiente();

		// Evitar peticiones a recursos que no existen despues de enviar una peticion
		if (contexto.Response.StatusCode == 404 && contexto.Response.HasStarted == false)
		{
			contexto.Response.StatusCode = StatusCodes.Status301MovedPermanently;
			contexto.Response.Headers.Location = "/";
		}
	}
	catch
	{
		if (contexto.Response.HasStarted == false)
		{
			contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
		}
	}
});

#endregion

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

app.Run();

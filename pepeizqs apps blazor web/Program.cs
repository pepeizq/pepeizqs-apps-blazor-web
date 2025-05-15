using Herramientas;
using pepeizqs_apps_blazor_web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebOptimizer(opciones =>
{
	opciones.AddCssBundle("/css/bundle.css", new NUglify.Css.CssSettings
	{
		CommentMode = NUglify.Css.CssComment.None,

	}, "lib/bootstrap/dist/css/bootstrap.min.css", "css/colores_texto.css", "css/principal.css", "css/site.css", "lib/font-awesome/css/all.css");

	opciones.AddJavaScriptBundle("/superjs.js", "lib/jquery/dist/jquery.min.js", "lib/bootstrap/dist/js/bootstrap.bundle.min.js", "js/site.js");
});

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

builder.Services.AddHostedService(provider => provider.GetRequiredService<Tareas.TareaGithub>());

#endregion

#region Contexto para Idioma y User-Agent

builder.Services.AddHttpContextAccessor();

#endregion

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

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

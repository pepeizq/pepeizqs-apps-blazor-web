#nullable disable

using Listados;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace Tareas
{

	public class TareaIndexNow : BackgroundService
	{
		private readonly ILogger<TareaIndexNow> _logger;
		private readonly IServiceScopeFactory _factoria;
		private readonly IConfiguration _configuracion;

		public TareaIndexNow(ILogger<TareaIndexNow> logger, IServiceScopeFactory factory, IConfiguration configuracion)
		{
			_logger = logger;
			_factoria = factory;
			_configuracion = configuracion;
		}

		protected override async Task ExecuteAsync(CancellationToken tokenParar)
		{
			using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(20));

			while (await timer.WaitForNextTickAsync(tokenParar))
			{
				try
				{
					using SqlConnection conexion = Herramientas.BaseDatos.Conectar();

					TimeSpan tiempoSiguiente = TimeSpan.FromHours(1);

					if (BaseDatos.Tareas.ComprobarTareaUso(conexion, "indexNow", tiempoSiguiente) == true)
					{
						BaseDatos.Tareas.ActualizarTareaUso(conexion, "indexNow", DateTime.Now);

						List<Proyecto> proyectos = Listados.Proyectos.Generar();

						var handler = new HttpClientHandler()
						{
							ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
							{
								return true;
							}
						};

						using (handler)
						{
							using (HttpClient cliente = new HttpClient(handler) { })
							{
								cliente.DefaultRequestHeaders.Add("User-Agent", "IndexNowBot");
								cliente.Timeout = TimeSpan.FromSeconds(30);

								List<string> listaEnlaces = new List<string>();

								foreach (var proyecto in proyectos)
								{
									if (string.IsNullOrEmpty(proyecto.Ubicacion) == false)
									{
										listaEnlaces.Add("https://pepeizqapps.com" + proyecto.Ubicacion);
									}
								}

								if (listaEnlaces?.Count > 0)
								{
									var indexNowConfig = _configuracion.GetSection("IndexNow");
									string host = indexNowConfig["Host"];
									string key = indexNowConfig["Key"];
									string keyLocation = indexNowConfig["KeyLocation"];

									var payload = new
									{
										host,
										key,
										keyLocation,
										urlList = listaEnlaces
									};

									string json = JsonSerializer.Serialize(payload);
									StringContent contenido = new StringContent(json, Encoding.UTF8, "application/json");
									await cliente.PostAsync("https://www.bing.com/indexnow", contenido);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					
				}
			}
		}
	}
}

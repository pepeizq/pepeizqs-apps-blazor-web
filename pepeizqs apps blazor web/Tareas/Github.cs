#nullable disable

using Herramientas;
using Herramientas.Conexiones;
using Microsoft.Data.SqlClient;

namespace Tareas
{
	public class TareaGithub : BackgroundService
	{
		private readonly ILogger<TareaGithub> _logger;
		private readonly IServiceScopeFactory _factoria;
		private readonly IDecompiladores _decompilador;

		public TareaGithub(ILogger<TareaGithub> logger, IServiceScopeFactory factory, IDecompiladores decompilador)
		{
			_logger = logger;
			_factoria = factory;
			_decompilador = decompilador;
		}

		protected override async Task ExecuteAsync(CancellationToken tokenParar)
		{
			using PeriodicTimer timer = new(TimeSpan.FromSeconds(60));

			while (await timer.WaitForNextTickAsync(tokenParar))
			{
				try
				{
					using (AsyncServiceScope scope = _factoria.CreateAsyncScope())
					{
						using SqlConnection conexion = Pepeizqapps.Conectar();

						TimeSpan tiempoSiguiente = TimeSpan.FromMinutes(120);

						if (BaseDatos.Tareas.ComprobarTareaUso(conexion, "github", tiempoSiguiente) == true)
						{
							BaseDatos.Tareas.ActualizarTareaUso(conexion, "github", DateTime.Now);

							foreach (var proyecto in BaseDatos.Github.Todo(conexion))
							{
								GithubAPI api = await Github.CargarAPI(proyecto.Id);

								if (string.IsNullOrEmpty(api.UltimaModificacion) == false && api.Estrellas > -1 && api.Forks > -1)
								{
									BaseDatos.Github.Actualizar(conexion, proyecto.Id, api.UltimaModificacion, api.Estrellas, api.Forks, api.Suscriptores);
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					BaseDatos.Errores.Mensaje("TareaGithub", ex.Message);
				}
			}
		}

		public override async Task StopAsync(CancellationToken stoppingToken)
		{
			await base.StopAsync(stoppingToken);
		}
	}
}

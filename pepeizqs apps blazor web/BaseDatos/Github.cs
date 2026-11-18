#nullable disable

using Dapper;
using Herramientas.Conexiones;
using Microsoft.Data.SqlClient;

namespace BaseDatos
{
	public static class Github
	{
		public static GithubBaseDatos Buscar(SqlConnection conexion, string id)
		{
			string seleccionarDato = "SELECT * FROM proyectosGithub WHERE id=@id";

			return conexion.QueryFirstOrDefault<GithubBaseDatos>(seleccionarDato, new { id });
		}

		public static async Task<List<GithubBaseDatos>> Todo()
		{
			string sql = "SELECT * FROM proyectosGithub";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return (await conexion.QueryAsync<GithubBaseDatos>(sql)).ToList();
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Github Todo - pepeizq's apps", ex.Message);
			}

			return null;
		}

		public static void Actualizar(SqlConnection conexion, string id, string fecha, int estrellas, int forks, int suscriptores)
		{
			string sqlActualizar = "UPDATE proyectosGithub " +
						"SET fecha=@fecha, estrellas=@estrellas, forks=@forks, suscriptores=@suscriptores WHERE id=@id";

			try
			{
				conexion.Execute(sqlActualizar, new { id, fecha, estrellas, forks, suscriptores });
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Actualizar Github Proyecto", ex.Message);
			}
		}
	}

	public class GithubBaseDatos
	{
		public string Id { get; set; }
		public string Fecha { get; set; }
		public int Estrellas { get; set; }
		public int Forks { get; set; }
		public int Suscriptores { get; set; }
	}
}
#nullable disable

using Dapper;
using Herramientas.Conexiones;

namespace BaseDatos
{

	public static class Noticias
	{
		public static async Task<Noticia> UnaNoticia(int id)
		{
			string sql = "SELECT * FROM noticias WHERE id = @id";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return await conexion.QueryFirstOrDefaultAsync<Noticia>(sql, new { id });
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Noticia Una - pepeizq's apps", ex.Message);
			}

			return null;
		}

		public static async Task<List<Noticia>> UltimasReducida(int cantidad)
		{
			string busqueda = "SELECT TOP " + cantidad.ToString() + " id, titulo, fecha, imagen FROM noticias ORDER BY id DESC";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return (await conexion.QueryAsync<Noticia>(busqueda)).ToList();
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Noticias Ultimas Reducida - pepeizq's apps", ex.Message);
			}

			return null;
		}

		public static async Task<List<Noticia>> Ultimas(int cantidad)
		{
			string busqueda = "SELECT TOP " + cantidad.ToString() + " * FROM noticias ORDER BY id DESC";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return (await conexion.QueryAsync<Noticia>(busqueda)).ToList();
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Noticias Ultimas - pepeizq's apps", ex.Message);
			}

			return null;
		}
	}

	public class Noticia
	{
		public int Id { get; set; }
		public string Titulo { get; set; }
		public string Contenido { get; set; }
		public DateTime Fecha { get; set; }
		public string Imagen { get; set; }
	}
}

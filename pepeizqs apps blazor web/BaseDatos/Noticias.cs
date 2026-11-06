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

			return await Pepeizqapps.Select(async conexion =>
			{
				return await conexion.QueryFirstOrDefaultAsync<Noticia>(sql, new { id });
			});
		}
	}

	public class Noticia
	{
		public string Titulo { get; set; }
		public string Contenido { get; set; }
		public DateTime Fecha { get; set; }
	}
}

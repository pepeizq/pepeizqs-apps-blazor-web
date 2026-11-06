#nullable disable

using Dapper;
using Herramientas.Conexiones;

namespace BaseDatos
{

	public static class Errores
	{
		public static async void Mensaje(string seccion, string mensaje, string enlace = null)
		{
			string sql = @"
				INSERT INTO errores
				(seccion, mensaje, stacktrace, fecha, enlace)
				VALUES (@seccion, @mensaje, @stacktrace, @fecha, @enlace);
			";

			await Pepedeals.RestoOperaciones(async (conexion, sentencia) =>
			{
				return await conexion.ExecuteAsync(sql, new
				{
					seccion,
					mensaje,
					stacktrace = "",
					fecha = DateTime.Now,
					enlace = (object)enlace ?? DBNull.Value
				}, transaction: sentencia);
			});
		}
	}
}

using Dapper;

namespace BaseDatos
{

	public static class Errores
	{
		public static async void Mensaje(string mensaje)
		{
			string sql = @"
                INSERT INTO errores
                (mensaje)
                VALUES (@mensaje)
            ";

			await Herramientas.BaseDatos.RestoOperaciones(async (conexion, sentencia) =>
			{
				return await conexion.ExecuteAsync(sql, new
				{
					mensaje = mensaje
				}, transaction: sentencia);
			});
		}
	}
}

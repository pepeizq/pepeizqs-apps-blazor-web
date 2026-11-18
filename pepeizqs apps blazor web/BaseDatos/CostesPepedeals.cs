#nullable disable

using Dapper;
using Herramientas.Conexiones;

namespace BaseDatos
{
	public static class CostesPepedeals
	{
		public static async Task<decimal> Saldo()
		{
			string sql = "SELECT ISNULL(SUM(importe), 0) FROM costespepedeals";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return await conexion.ExecuteScalarAsync<decimal>(sql);
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Saldo pepe's deals - pepeizq's apps", ex.Message);
			}

			return 0;
		}

		public static async Task<List<CostePepedeals>> Todo()
		{
			string sql = "SELECT * FROM costespepedeals";

			try
			{
				return await Pepeizqapps.Select(async conexion =>
				{
					return (await conexion.QueryAsync<CostePepedeals>(sql)).ToList();
				});
			}
			catch (Exception ex)
			{
				BaseDatos.Errores.Mensaje("Todo pepe's deals - pepeizq's apps", ex.Message);
			}

			return null;
		}
	}

	public class CostePepedeals
	{
		public string Concepto { get; set; }
		public decimal Importe { get; set; }
		public DateTime Fecha { get; set; }
	}
}

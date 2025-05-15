#nullable disable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Herramientas
{
	public static class Github
	{
		public static async Task<GithubAPI> CargarAPI(string proyecto)
		{
			string html = await Decompiladores.Estandar("https://api.github.com/repos/pepeizq/" + proyecto);

			if (string.IsNullOrEmpty(html) == false)
			{
				GithubAPI api = JsonSerializer.Deserialize<GithubAPI>(html);

				if (api != null) 
				{
					return api;
				}
			}

			return null;
		}
	}

	public class GithubAPI
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("pushed_at")]
		public string UltimaModificacion { get; set; }

		[JsonPropertyName("stargazers_count")]
		public string Estrellas { get; set; }

		[JsonPropertyName("forks_count")]
		public string Forks { get; set; }

        [JsonPropertyName("subscribers_count")]
        public string Suscriptores { get; set; }
    }
}

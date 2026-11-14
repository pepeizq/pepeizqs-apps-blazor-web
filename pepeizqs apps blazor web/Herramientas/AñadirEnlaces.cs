using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace Herramientas
{

	public static class AñadirEnlaces
	{
		static readonly Regex regexDominio = new(
			@"(?<![@\w./-])(?:https?://)?(?:[a-z0-9-]+\.)+(?:com|net|org|es|io|app|deals)\b(?:/[^\s<]*)?",
			RegexOptions.IgnoreCase);

		public static MarkupString Enlazar(string texto)
		{
			string codificado = System.Net.WebUtility.HtmlEncode(texto);

			string resultado = regexDominio.Replace(codificado, m =>
			{
				string url = m.Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
					? m.Value
					: "https://" + m.Value;

				return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\" style=\"text-decoration: none\">{m.Value}</a>";
			});

			return new MarkupString(resultado);
		}
	}
}

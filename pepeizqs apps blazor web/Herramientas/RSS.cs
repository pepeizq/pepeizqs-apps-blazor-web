#nullable disable

using Microsoft.AspNetCore.Mvc;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace Herramientas
{

	public class RSS : Controller
	{
		[ResponseCache(Duration = 300)]
		[HttpGet("rss/updates.xml")]
		public async Task<IActionResult> GenerarActualizaciones()
		{
			string dominio = "https://" + HttpContext.Request.Host.Value;

			SyndicationFeed feed = new SyndicationFeed("pepeizq's apps", "RSS de las actualizaciones que hago en mi código", new Uri(dominio), "RSSUrl", DateTime.Now)
			{
				Copyright = new TextSyndicationContent($"{DateTime.Now.Year}")
			};

			List<SyndicationItem> items = new List<SyndicationItem>();

			List<BaseDatos.Noticia> noticias = await BaseDatos.Noticias.Ultimas(10);

			if (noticias?.Count > 0)
			{
				foreach (BaseDatos.Noticia noticia in noticias)
				{
					string enlace = dominio + "/updates/" + noticia.Id.ToString() + "/" + Herramientas.EnlaceAdaptador.Nombre(noticia.Titulo) + "/";

					string titulo = noticia.Titulo;
					string contenido = noticia.Contenido;
					Uri enlaceUri = null;

					if (enlace != null)
					{
						enlaceUri = new Uri(enlace);
					}

					SyndicationItem item = new SyndicationItem(titulo, contenido, enlaceUri, noticia.Id.ToString(), noticia.Fecha);

					items.Add(item);
				}

				feed.Items = items;
			}

			XmlWriterSettings opciones = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = true,
				Indent = true
			};

			using (MemoryStream stream = new MemoryStream())
			{
				using (XmlWriter xmlEscritor = XmlWriter.Create(stream, opciones))
				{
					Rss20FeedFormatter rssFormateador = new Rss20FeedFormatter(feed, false);
					rssFormateador.WriteTo(xmlEscritor);
					xmlEscritor.Flush();
				}

				return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
			}
		}
	}
}

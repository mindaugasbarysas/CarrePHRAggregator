using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Vulsk.CarrePhrAggregator.DataSpecification;

namespace Vulsk.CarrePhrAggregator.ResourceOutput
{
	public class ResourceOutput
	{
		public static XmlDocument Output(XmlDocument input)
		{

			var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "")), "output.xslt"));
			var xt = new XslCompiledTransform();
			var output = new MemoryStream();
			var xw = XmlWriter.Create(output);
			xt.Load(path);
			xt.Transform(input, xw);
			output.Position = 0L;
			var xdoc = new XmlDocument();
			xdoc.Load(output);
			return xdoc;
		}

		public IGraph ToRdf(List<PhrData> input)
		{
			IGraph g = new Graph();
			var carreNode = g.CreateUriNode(UriFactory.Create("http://carre-project.eu"));
			foreach (var i in input)
			{
				foreach (var d in i.Data) {
					var predicate = g.CreateUriNode(UriFactory.Create(string.Format("http://carre-project.eu/{0}", d.Name)));
					var value = g.CreateLiteralNode(d.Value.ToString());
					g.Assert(new Triple(carreNode, predicate, value));
				}
			}

			return g;
		}

		public void WriteRdf(IGraph graph, string fileName)
		{
			var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", ""))));
			var rdfxmlwriter = new RdfXmlWriter();
			rdfxmlwriter.Save(graph, path + "/" + fileName + ".rdf");
		}

		public IGraph ReadRdf(string pathToFile)
		{
			IGraph g = new Graph();
			FileLoader.Load(g, pathToFile);

			return g;
		}
	}
}

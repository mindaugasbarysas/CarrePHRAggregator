using System;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;

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
	}
}

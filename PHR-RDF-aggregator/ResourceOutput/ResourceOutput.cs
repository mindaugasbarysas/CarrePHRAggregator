using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;

namespace VULSK.CarrePHRAggregator.ResourceOutput
{
	public class ResourceOutput
	{
		public XmlDocument Output(XmlDocument input)
		{

			string path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "")), "output.xslt" ));
			XslCompiledTransform xt=new XslCompiledTransform();
			System.IO.MemoryStream output = new System.IO.MemoryStream();
			XmlWriter xw= XmlWriter.Create(output);
			xt.Load(path);
			xt.Transform(input, xw);
			output.Position=0L;
			XmlDocument xdoc=new XmlDocument();
			Byte[] buffer = null;
			output.Read(buffer,0, Convert.ToInt32(output.Length));
			xdoc.LoadXml(Encoding.UTF8.GetString(buffer));
			return xdoc;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VULSK.CarrePHRAggregator.DataSpecification;
using VULSK.CarrePHRAggregator.PHRInput;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace VULSK.CarrePHRAggregator.RUM
{


	public class RUM
	{
		private List<IPHRInput> _availablePhrs=new List<IPHRInput>();

		public RUM()
		{
			string path = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "")));
			string[] pluginFiles = Directory.GetFiles(path, "*.dll");

			var ipi = (
				from file in pluginFiles
				let asm = Assembly.LoadFile(file)
				from type in asm.GetExportedTypes()
				where typeof(IPHRInput).IsAssignableFrom(type) && type!=typeof(IPHRInput)
				select Activator.CreateInstance(type)
			).ToArray();
			_availablePhrs.AddRange(_availablePhrs);
		}

		public List<PHRData> GetPatientData(PatientIdentifier p)
		{
			List<PHRData> retList = new List<PHRData>();
			foreach (IPHRInput phrSource in _availablePhrs)
			{
				if (phrSource.Source!=null)
				{ 
					retList.Add(phrSource.GetData(p));
					//weighting/deduplication logic could go here
				}
			}
			return retList;
		}

		public XmlDocument ToXML(List<PHRData> input)
		{
			XmlSerializer xs=new XmlSerializer(typeof(List<PHRData>));
			MemoryStream ms=new MemoryStream();
			xs.Serialize(ms, input);
			XmlDocument xd=new XmlDocument();
			ms.Position=0L;
			xd.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
			return xd;
		}

		public PHRData GetPatientData(PatientIdentifier p, IPHRInput phrInput)
		{
			return phrInput.GetData(p);
		}


	}
}

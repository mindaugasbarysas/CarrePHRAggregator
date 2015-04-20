using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Vulsk.CarrePhrAggregator.Rum
{
	using DataSpecification;

	public class Rum
	{
		private readonly List<IPhrInput> _availablePhrs = new List<IPhrInput>();

		public Rum()
		{
			var path = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "")));
			var pluginFiles = Directory.GetFiles(path, "*.dll");

			var ipi = (
				from file in pluginFiles
				let asm = Assembly.LoadFile(file)
				from type in asm.GetExportedTypes()
				where typeof(IPhrInput).IsAssignableFrom(type) && type != typeof(IPhrInput)
				select Activator.CreateInstance(type)
			).ToArray();
			_availablePhrs.AddRange(ipi.Select(i => (IPhrInput)i));
		}

		public List<PhrData> GetPatientData(PatientIdentifier p)
		{
			var retList = new List<PhrData>();
			foreach (var phrSource in _availablePhrs)
			{
				if (phrSource.Source != null)
				{
					retList.Add(phrSource.GetData(p));
					//weighting/deduplication logic could go here
				}
			}
			return retList;
		}

		public static XmlDocument ToXml(List<PhrData> input)
		{
			var xs = new XmlSerializer(typeof(List<PhrData>));
			var ms = new MemoryStream();
			xs.Serialize(ms, input);
			var xd = new XmlDocument();
			ms.Position = 0L;
			xd.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
			return xd;
		}

		public PhrData GetPatientData(PatientIdentifier p, IPhrInput phrInput)
		{
			return phrInput.GetData(p);
		}
	}
}

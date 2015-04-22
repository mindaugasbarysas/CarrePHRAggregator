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
    using Vulsk.CarrePhrAggregator.ResourceConfiguration;

	public class Rum
	{
		private readonly List<IPhrInput> _availablePhrs = new List<IPhrInput>();

        private readonly List<IConfiguration> _availableConfigurators = new List<IConfiguration>();

        /// <summary>
        /// Production constructor of RUM, loading fake configurators disabled.
        /// </summary>
        public Rum()
        {
            this.initRum(false);
        }

        /// <summary>
        /// Constructor which lets end-user tell RUM to load fake configurators.
        /// </summary>
        /// <param name="loadFakes">Should RUM load fake configurators?</param>
        public Rum(bool loadFakes)
        {
            this.initRum(loadFakes);
        }

        /// <summary>
        /// Initializes RUM instance.
        /// </summary>
        /// <param name="loadFakes">Should RUM load fake configurators?</param>
		public void initRum(bool loadFakes)
		{
			var path = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", "")));
			var pluginFiles = Directory.GetFiles(path, "*.dll");

            var configurators = (
                from file in pluginFiles
                let asm = Assembly.LoadFile(file)
                from type in asm.GetExportedTypes()
                where typeof(IConfiguration).IsAssignableFrom(type) && type != typeof(IConfiguration)
                select Activator.CreateInstance(type)
            ).ToArray();

            _availableConfigurators.AddRange(configurators.Select(i => (IConfiguration)i).Where(i => i.fake == loadFakes));

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
            foreach (var configurator in _availableConfigurators)
            {
                var configuration = configurator.GetConfiguration();
                foreach (var phrSource in _availablePhrs)
                {
                    if (phrSource.Source == null)
                    {
                        continue;
                    }
                    if (configuration.Sources.Select(s => s.InternalId).Contains(phrSource.Source.InternalId)) //not the best thing, but at this stage it will do...
                    {
                        var phrdata = phrSource.GetData(p, configuration);
                        if (phrdata == null)
                        {
                            // no data to add and/or process.
                            continue;
                        }
                        // filter
                        List<DataUnit> filteredData = new List<DataUnit>();
                        filteredData.AddRange(phrdata.Data.Where(pd => configuration.DesiredData.Select(s => s.OntologicName).Contains(pd.OntologicName)));
                        phrdata.Data = filteredData;
                        retList.Add(phrdata);
                    }
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

        public List<IPhrInput> getPhrList()
        {
            return _availablePhrs;
        }

        public List<IConfiguration> getConfigurators()
        {
            return _availableConfigurators;
        }
	}
}

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
    using Vulsk.CarrePhrAggregator.ResourceOutput;
	public class Rum
	{
		private readonly List<IPhrInput> _availablePhrs = new List<IPhrInput>();

        private readonly List<IConfiguration> _availableConfigurators = new List<IConfiguration>();

        private readonly List<IOutput> _availableOutputs = new List<IOutput>();

        private string patientPath = "../configuration/vivaportPatientMap.xml";

        public string PatientListLocation { get { return patientPath; } set { patientPath = value; } }

        public SourceIdentifier Source { get { return _sourceId; } }
        private readonly SourceIdentifier _sourceId = new SourceIdentifier
        {
            InternalId = typeof(Rum).GUID,
            SourceName = "RUM"
        };

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

            var outputs = (
                from file in pluginFiles
                let asm = Assembly.LoadFile(file)
                from type in asm.GetExportedTypes()
                where typeof(IOutput).IsAssignableFrom(type) && type != typeof(IOutput)
                select Activator.CreateInstance(type)
                ).ToArray();
            
            _availableOutputs.AddRange(outputs.Select(i => (IOutput)i));
		}

        private List<PatientIdentifier> GetPatients()
        {
            List<PatientIdentifier> patients = new List<PatientIdentifier>();
            if (File.Exists(this.PatientListLocation))
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(this.PatientListLocation);
                XmlNodeList nodes = xdoc.SelectNodes("//Patient");
                foreach (XmlNode node in nodes)
                {
                    patients.Add(new PatientIdentifier() { InternalId = new Guid(node.SelectSingleNode("Internal").InnerText) });
                }
            }

            return patients;
        }

        public void Run()
        {
            foreach (PatientIdentifier p in GetPatients())
            {
                foreach (IOutput o in _availableOutputs)
                {
                    o.Output(GetPatientData(p));
                    // Future improvement: logging.
                }
            }
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

        public List<IOutput> getOutputs()
        {
            return _availableOutputs;
        }
	}
}

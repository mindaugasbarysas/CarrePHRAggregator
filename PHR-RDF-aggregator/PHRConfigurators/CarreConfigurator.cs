using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulsk.CarrePhrAggregator.ResourceConfiguration;
using Vulsk.CarrePhrAggregator.DataSpecification;
using Vulsk.CarrePhrAggregator.PhrConfigurators.CarreRestObjects;
using System.IO;
using System.Reflection;
using Pareto.Rest.Client.Http;
using Newtonsoft.Json;

namespace Vulsk.CarrePhrAggregator.PhrConfigurators
{
    public class CarreConfigurator : IConfiguration
    {
        /// <summary>
        /// Production Carre configurator.
        /// </summary>
        public bool fake { get { return false; } }

        private string api_endpoint = "https://carre.kmi.open.ac.uk:443/ws/";

        private List<string> types = new List<string>() { "observable", "clinical_observable", "personal_observable" };

        private readonly SourceIdentifier _sourceId = new SourceIdentifier
        {
            InternalId = typeof(CarreConfigurator).GUID,
            SourceName = "CARRE Configurator"
        };
        public SourceIdentifier Source { get { return _sourceId; } }

        public Configuration GetConfiguration()
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

            List<DataUnit> knownUnits = new List<DataUnit>();
            foreach (var type in this.types)
            {
                HttpClient Client = new HttpClient(api_endpoint);
                var rawRepoData = JsonConvert.DeserializeObject<List<CarreInstance>>(Client.Get(new HttpRequestData(string.Format("instances?type={0}", type))).Data);
                var subjects = rawRepoData.Select(p => p.subject).Where(p=>!knownUnits.Select(ku=>ku.OntologicName).Contains(p)).ToList().Distinct();
                foreach (var p in subjects)
                {
                    string name = rawRepoData.Where(e => (e.predicate == "http://carre.kmi.open.ac.uk/ontology/risk.owl#observable_name" && e.subject == p)).Select(e => e.obj).FirstOrDefault();
                    string identifier = rawRepoData.Where(e => (e.predicate == "http://carre.kmi.open.ac.uk/ontology/risk.owl#has_risk_element_identifier" && e.subject == p)).Select(e => e.obj).FirstOrDefault();
                    
                    if (knownUnits.Select(pp => pp.Identifier).Contains(identifier))
                    {
                        identifier = string.Format("{0}#duplicated#{1}#", identifier, Guid.NewGuid().ToString());
                    }

                    if (name == null && identifier == null) // it can't be helped. Skip.
                    {
                        continue;
                    }
                    if (name == null) name = p;
                    if (identifier == null) identifier = p;
                    knownUnits.Add(
                        new DataUnit()
                        {
                            Name = name,
                            OntologicName = p,
                            Identifier = identifier
                        }
                   );
                }
            }
            return new Configuration()
            {
                Sources = ipi.Select(i => (IPhrInput)i).Where(i=>i.Source.SourceName == "Vivaport.eu" || i.Source.SourceName == "Microsoft HealthVault").Select(i => i.Source).ToList(),
                DesiredData = knownUnits
            };
        }
    }
}

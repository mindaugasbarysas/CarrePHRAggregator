using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulsk.CarrePhrAggregator.ResourceConfiguration;
using Vulsk.CarrePhrAggregator.DataSpecification;
using System.IO;
using System.Reflection;

namespace Vulsk.CarrePhrAggregator.PhrConfigurators
{
    public class EmptyConfigurator : IConfiguration
    {
        public bool fake { get { return true; } }

        private readonly SourceIdentifier _sourceId = new SourceIdentifier
        {
            InternalId = typeof(EmptyConfigurator).GUID,
            SourceName = "Empty configurator for testing"
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

            var sources = new List<SourceIdentifier>();

            foreach (IPhrInput source in ipi)
            {
                if (source.Source.SourceName == "FakeTestSource")
                    sources.Add(source.Source);
            }

            return new Configuration()
            {
                Sources = sources,
                DesiredData = new List<DataUnit>() { 
                    new DataUnit() { 
                        Name = "TestEntry",
                        OntologicName = "rdf:TestEntry" 
                    } 
                }
            };
        }
    }
}

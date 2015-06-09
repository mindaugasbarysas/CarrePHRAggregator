using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vulsk.CarrePhrAggregator.PhrPlugins
{
    using DataSpecification;
    using PatientSummary;

    /// <summary>
    /// Test interface for testing.
    /// </summary>
	public class PhrPluginEmpty : IPhrInput
	{
		private readonly SourceIdentifier _sourceId = new SourceIdentifier {
            InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e565"),
			SourceName = "FakeTestSource"
		};
		public SourceIdentifier Source { get { return _sourceId; } }

		public PhrData GetData(PatientIdentifier p, Configuration config)
		{
            return new PhrData()
            {
                Source = this.Source,
                Patient = p,
                Data = new List<DataUnit>()
                { 
                    new DataUnit() 
                    { 
                        OntologicName = "rdf:TestEntry", 
                        Name = "TestEntry"
                    },
                    new DataUnit() 
                    {
                        OntologicName = "rdf:DifferentTestEntry",
                        Name = "DifferentTestEntry"
                    }
                }
            };
		}
    }
}

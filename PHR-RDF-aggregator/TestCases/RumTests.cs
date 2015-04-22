using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCases
{
	using Vulsk.CarrePhrAggregator.PhrPlugins;
	using Vulsk.CarrePhrAggregator.DataSpecification;
	using Vulsk.CarrePhrAggregator.Rum;
	using Vulsk.CarrePhrAggregator.ResourceOutput;
    using Vulsk.CarrePhrAggregator.PhrConfigurators;
    using Microsoft.Health;


	[TestClass]
	public class RumTests
	{
		private const string PGuid = "5c30ee9a-2e63-42c7-b418-ef4fe2f3e565";

		[TestMethod]
		public void TestRum()
		{
			var rum = new Rum(true);
			var pd = rum.GetPatientData(new PatientIdentifier { InternalId = new Guid(PGuid) });

			var output = ResourceOutput.Output(Rum.ToXml(pd));
			output.Save("RumOutputTest.xml");
            Assert.IsTrue(output.HasChildNodes);
		}

        [TestMethod]
        public void TestRumInitialization()
        {
            var rum = new Rum(true);
            Assert.IsTrue(rum.getPhrList().Count > 0, "There should be at least one PHR module loaded!");
            Assert.IsTrue(rum.getConfigurators().Count > 0, "There should be at least one Configurator module loaded!");
            Assert.IsTrue(rum.getPhrList().Exists(p=>p.Source.InternalId == new PhrPluginEmpty().Source.InternalId), "Empty test source should be loaded");
            Assert.IsTrue(rum.getConfigurators().Exists(p => p.Source.InternalId == new EmptyConfigurator().Source.InternalId), "Empty configurator should be loaded");
        }

        [TestMethod]
        //[ExpectedException(typeof(HealthServiceAccessDeniedException))]
        public void TestRumRun()
        {
            var rum = new Rum(true);
            var patient = new PatientIdentifier() { InternalId = new Guid(PGuid) };
            var patData = rum.GetPatientData(patient);
            Assert.IsTrue(patData.FindAll(p => p.Patient.InternalId == patient.InternalId).Count == 1, "There are no entries for test patient, why?");
            Assert.IsTrue(patData.FindAll(p => p.Data.FindAll(d => d.OntologicName == "rdf:TestEntry").Count > 0).Count == 1, "PhrPluginEmpty should have been returned DataUnit with a TestEntry.");
            Assert.IsTrue(patData.FindAll(p => p.Data.FindAll(d => d.OntologicName == "rdf:DifferentTestEntry").Count > 0).Count == 0, "DifferentTestEntry should have been filtered out by RUMs' config.");
        }
	}
}



using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TestCases
{
	using Vulsk.CarrePhrAggregator.PhrPlugins;
	using Vulsk.CarrePhrAggregator.DataSpecification;
	using Vulsk.CarrePhrAggregator.Rum;
    using Vulsk.CarrePhrAggregator.Rum.Unification;
	using Vulsk.CarrePhrAggregator.ResourceOutput;
    using Vulsk.CarrePhrAggregator.PhrConfigurators;
    using Microsoft.Health;


	[TestClass]
	public class RumTests
	{
		private const string PGuid = "5c30ee9a-2e63-42c7-b418-ef4fe2f3e565";

        [TestMethod]
        public void TestRumInitialization()
        {
            var rum = new Rum(true);
            Assert.IsTrue(rum.getPhrList().Count > 0, "There should be at least one PHR module loaded!");
            Assert.IsTrue(rum.getConfigurators().Count > 0, "There should be at least one Configurator module loaded!");
            Assert.IsTrue(rum.getOutputs().Count > 0, "There should be at least one Output module loaded!");
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

        [TestMethod]
        [TestCategory("Unificator")]
        public void TestUnificationConfiguration()
        {
            var unificator = new Unificator();
            unificator.LoadUnificationRules("../../Fixtures/SourcePriorities.xml");
            Assert.IsTrue(unificator.GetUnificationRules().SourcePriority.Exists(p => p.Source.InternalId == new PhrPluginEmpty().Source.InternalId && p.Priority == 10 && p.NewerRecordPriority == 10), "Empty test source should be loaded with priority 10");
            Assert.IsTrue(unificator.GetUnificationRules().SourcePriority.Exists(p => p.Source.InternalId == new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e566") && p.Priority == 20 && p.NewerRecordPriority == 10), "Fake test source should be loaded with priority 20");
        }

        [TestMethod]
        [TestCategory("Unificator")]
        public void TestUnification()
        {
            var unificator = new Unificator();
            unificator.LoadUnificationRules("../../Fixtures/SourcePriorities.xml");

            PatientIdentifier patient1 = new PatientIdentifier() {
                          InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e565")
                     };
            PatientIdentifier patient2 = new PatientIdentifier() {
                          InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e566") // should be untouched
                     };

            SourceIdentifier source1 = new SourceIdentifier() {
                          InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e565"), // has priority 10
                          SourceName = "FAKE"
                     };
            SourceIdentifier source2 = new SourceIdentifier() {
                          InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e566"), // has priority 20
                          SourceName = "FAKE"
                     };

            List<PhrData> TestData = new List<PhrData>() {
                new PhrData() {
                     Patient = patient1,
                     Source = source1,
                     Data = new List<DataUnit>() {
                         new DataUnit() {
                             OntologicName = "test1",
                             Datetime = DateTime.Now.Date.AddDays(-2), //overrides value-1
                             Identifier = "identifier-1",
                             Name = "name-1",
                             Value = "value-1"
                         },
                         new DataUnit() {
                             OntologicName = "test2",
                             Datetime = DateTime.Now.Date.AddDays(-2),
                             Identifier = "identifier-2",
                             Name = "name-2",
                             Value = "value-1-1"
                         }
                     }
                },
                new PhrData() {
                     Patient = patient1,
                     Source = source2, // priority of 20, should be overriden
                     Data = new List<DataUnit>() {
                         new DataUnit() {
                             OntologicName = "test1",
                             Datetime = DateTime.Now.Date.AddDays(-2),
                             Identifier = "identifier-1",
                             Name = "name-1",
                             Value = "value-1-3"
                         },
                         new DataUnit() {
                             OntologicName = "test2",
                             Datetime = DateTime.Now.Date.AddDays(-1), //newer, should override
                             Identifier = "identifier-2",
                             Name = "name-2",
                             Value = "value-1-4"
                         }
                     }
                },
                new PhrData() {
                     Patient = patient2,
                     Source = source2,
                     Data = new List<DataUnit>() {
                         new DataUnit() {
                             OntologicName = "test1",
                             Datetime = DateTime.Now.AddDays(-2),
                             Identifier = "identifier-1",
                             Name = "name-1",
                             Value = "value-1"
                         },
                         new DataUnit() {
                             OntologicName = "test2",
                             Datetime = DateTime.Now.AddDays(-1),
                             Identifier = "identifier-2",
                             Name = "name-2",
                             Value = "value-1-1"
                         }
                     }
                },
            };

            List<PhrData> result = unificator.Unify(TestData);

            Assert.IsTrue(result.Exists(phd => phd.Patient == patient1), "patient1 should exist in list.");
            Assert.IsTrue(result.Exists(phd => phd.Patient == patient2), "patient2 should exist in list.");
            
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-1")), "patient1 data missing");
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-2")), "patient1 data missing");

            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient2).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-1")), "patient2 data missing");
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient2).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-2")), "patient2 data missing");
            

            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient2).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-1" && dat.Value == "value-1")), "patient2 data touched!");
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient2).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-2" && dat.Value == "value-1-1")), "patient2 data touched!");

            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).FindAll(pd => pd.Data.Exists(dat => dat.Name == "name-1")).Count == 1);
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).FindAll(pd => pd.Data.Exists(dat => dat.Name == "name-2")).Count == 1);

            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-1" && dat.Value == "value-1")), "patient1 data identifier-1 is not value-1!");
            Assert.IsTrue(result.FindAll(phd => phd.Patient == patient1).Exists(pd => pd.Data.Exists(dat => dat.Name == "name-2" && dat.Value == "value-1-4")), "partient1 data name-2 is not value-1-4!");
        }
	}
}



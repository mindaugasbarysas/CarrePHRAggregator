using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vulsk.CarrePhrAggregator.PhrPlugins;
using Vulsk.CarrePhrAggregator.DataSpecification;
using System.Collections.Generic;
using System.Xml;

namespace TestCases
{
    [TestClass]
    public class VivaportPluginTest
    {
        private const string PGuid = "5c30ee9a-2e63-42c7-b418-ef4fe2f3e565";

        [TestMethod]
        public void TestMappingLoader()
        {
            PhrPluginVivaport ppV = new PhrPluginVivaport("../../Fixtures/mapping.xml", "../../Fixtures/Documents");
            Assert.IsTrue(ppV.GetMap().ContainsKey("http://localhost/test#TestName"));
            Assert.IsTrue(ppV.GetMap().ContainsKey("http://localhost/test#TestSurname"));
            Assert.AreEqual(ppV.GetMap()["http://localhost/test#TestName"], "//TestDataUnit/TestPatientInformation/TestPatientName/text()");
            Assert.IsTrue(ppV.GetTypeMap().ContainsKey("http://localhost/test#TestSurname"));
            Assert.AreEqual(ppV.GetTypeMap()["http://localhost/test#TestSurname"], "strong");
        }
        [TestMethod]
        public void TestDocumentLoader()
        {
            PhrPluginVivaport ppV = new PhrPluginVivaport("../../Fixtures/mapping.xml", "../../Fixtures/Documents");
            Assert.IsTrue(ppV.GetDocuments().ContainsKey(PGuid));
            Assert.IsTrue(ppV.GetDocuments()[PGuid].GetType() == typeof(List<XmlDocument>));
            Assert.IsTrue(((List<XmlDocument>)ppV.GetDocuments()[PGuid]).Count == 2);
        }
        [TestMethod]
        public void TestPatientMap()
        {
            PhrPluginVivaport ppV = new PhrPluginVivaport("../../Fixtures/mapping.xml", "../../Fixtures/Documents", "../../Fixtures/Patients.xml");
            Assert.IsTrue(ppV.GetPatientMap().ContainsKey(PGuid));
            Assert.IsTrue(ppV.GetPatientMap()[PGuid].ToString() == PGuid);
        }

        [TestMethod]
        public void TestGetData()
        {
            Configuration config = new Configuration()
            {
                DesiredData = new List<DataUnit>() {
                    new DataUnit() { Name = "name", OntologicName = "http://localhost/test#TestName" },
                    new DataUnit() { Name = "surname", OntologicName = "http://localhost/test#TestSurname" }
                },
                Sources = new List<SourceIdentifier>() {
                    new SourceIdentifier() { SourceName = "vivaport", InternalId = new Guid()}
                }
            };
            PhrPluginVivaport ppV = new PhrPluginVivaport("../../Fixtures/mapping.xml", "../../Fixtures/Documents", "../../Fixtures/Patients.xml");
            PhrData data = ppV.GetData(new PatientIdentifier(){ InternalId = new Guid(PGuid) }, config);
            Assert.IsTrue((string)data.Data.Find(du => du.Name == "name").Value == "TestName");
            Assert.IsTrue((DateTime)data.Data.Find(du => du.Name == "name").Datetime == DateTime.Parse("1901-01-01"));
            Assert.IsTrue((string)data.Data.Find(du => du.Name == "surname").Value == "TestSurname");
            Assert.IsTrue((DateTime)data.Data.Find(du => du.Name == "surname").Datetime == DateTime.Parse("1900-01-01"));
            Assert.IsTrue(data.Data.Find(du => du.Name == "surname").OntologicType == "strong");
        }
    }
}

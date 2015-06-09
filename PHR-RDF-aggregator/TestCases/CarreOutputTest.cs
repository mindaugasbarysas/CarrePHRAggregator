using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vulsk.CarrePhrAggregator.ResourceOutput;
using Vulsk.CarrePhrAggregator.DataSpecification;
using System.Collections.Generic;
using Pareto.Rest.Client.Http;
using Newtonsoft.Json;
using Moq;
using Moq.AutoMock;
using System.Linq;

namespace TestCases
{
    [TestClass]
    public class CarreOutputTest
    {
        public string guid = "5c30ee9a-2e63-42c7-b418-ef4fe2f3e565";
        public List<PhrData> GetMockData()
        {
            PatientIdentifier patient1 = new PatientIdentifier()
            {
                InternalId = new Guid(guid)
            };

            SourceIdentifier source1 = new SourceIdentifier()
            {
                InternalId = new Guid("5c30ee9a-2e63-42c7-b418-ef4fe2f3e565"),
                SourceName = "vivaport"
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
                             Value = "value-1",
                             Source = source1,
                             OntologicType = "string",
                         },
                         new DataUnit() {
                             OntologicName = "test2",
                             Datetime = DateTime.Now.Date.AddDays(-2),
                             Identifier = "identifier-2",
                             Name = "name-2",
                             Value = "value-1-1",
                             Source = source1,
                             OntologicType = "string",
                         }
                     }
                },
            };

            return TestData;
        }

        private HttpResponseData CheckRequestFailInsert(HttpRequestData r)
        {
            if (r.Path != "/query") { throw new Exception("Wrong path!"); }
            var response = new HttpResponseData();
            
            if (r.Data == "{ token: \"77a5e3fad4d2646594fc12e67d2b6590e30df81f\", query: \"SELECT ?val FROM <https://carre.kmi.open.ac.uk/users/MindaugasB> WHERE { <https://carre.kmi.open.ac.uk/users/MindaugasB/measurements/vivaport_635688864000000000> <https://carre.kmi.open.ac.uk/users/MindaugasB/measurements/vivaport_635688864000000000_has_test1> ?val . }\" }") 
            {
                response.StatusCode = System.Net.HttpStatusCode.OK;
                response.Data = "OK";
                response.ContentLength = 1;
            } else {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return response;
        }

        private HttpResponseData CheckRequestSuccessInsert(HttpRequestData r)
        {
            if (r.Path != "/query") { throw new Exception("Wrong path!"); }
            var response = new HttpResponseData();

            if (r.Data.Contains("token: \"77a5e3fad4d2646594fc12e67d2b6590e30df81f\""))
            {
                if (r.Data.Contains("query: \"SELECT ?val FROM <https://carre.kmi.open.ac.uk/users/MindaugasB> WHERE { <https://carre.kmi.open.ac.uk/users/MindaugasB/measurements/vivaport_635692320000000000>"))
                {
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                }
                if (r.Data.Contains("query: \"INSERT DATA { GRAPH <https://carre.kmi.open.ac.uk/users/MindaugasB> { <https://carre.kmi.open.ac.uk/users/MindaugasB/measurements/vivaport_635692320000000000> <http://www.w3.org/2000/01/rdf-schema#type> :individual_test1_measurement .\n"))
                {
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    response.Data = "OK";
                    response.ContentLength = 1;
                }
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return response;
        }

        [TestMethod]
        public void TestFailInsert()
        {
            var mockClient = new Mock<IHttpClient>();
            mockClient.Setup(m => m.Post(It.IsAny<HttpRequestData>())).Returns<HttpRequestData>((i) => CheckRequestFailInsert(i) );
            CarreAuthProvider pr = new CarreAuthProvider();
            pr.ConfigurationLocation = "../../Fixtures/authTokens.xml";
            CarreOutput co = new CarreOutput();
            co.AuthProvider = pr;
            co.Client = mockClient.Object;
            List<OutputResult> results = co.Output(this.GetMockData());
            Assert.IsTrue(results.Where(o => o.Success == false).Count() == 2);
            Assert.IsTrue(results.Count == this.GetMockData().Select(p => p.Data).First().Count);
            
        }

        [TestMethod]
        public void TestSuccessInsert()
        {
            var mockClient = new Mock<IHttpClient>();
            mockClient.Setup(m => m.Post(It.IsAny<HttpRequestData>())).Returns<HttpRequestData>((i) => CheckRequestSuccessInsert(i));
            CarreAuthProvider pr = new CarreAuthProvider();
            pr.ConfigurationLocation = "../../Fixtures/authTokens.xml";
            CarreOutput co = new CarreOutput();
            co.AuthProvider = pr;
            co.Client = mockClient.Object;
            List<OutputResult> results = co.Output(this.GetMockData());
            Assert.IsTrue(results.Where(o => o.Success == false).Count() == 1);
            Assert.IsTrue(results.Count == this.GetMockData().Select(p => p.Data).First().Count);
            
        }
    }
}

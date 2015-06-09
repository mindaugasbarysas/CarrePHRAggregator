using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Vulsk.CarrePhrAggregator.DataSpecification;
using Pareto.Rest.Client.Http;
using Newtonsoft.Json;

namespace Vulsk.CarrePhrAggregator.ResourceOutput
{
    public class CarreOutput:IOutput
    {
        public IAuthProvider AuthProvider { get { return authProvider; } set { authProvider = value; } }
        public IHttpClient Client { get { return client; } set { client = value; } }
        private string _endpoint = "https://carre.kmi.open.ac.uk/users/"; //make this configurable
        private string _owl = "http://carre.kmi.open.ac.uk/ontology/risk.owl";
        private string _urlPrivateRDF = "https://carre.kmi.open.ac.uk/sparql-auth";
        private string _sPrefix = "http://carre.kmi.open.ac.uk/ontology/sensors.owl#";
        private IAuthProvider authProvider = new CarreAuthProvider();
        private IHttpClient client = new HttpClient("https://carre.kmi.open.ac.uk/ws/");
        public SourceIdentifier OutputIdentifier
        {
            get
            {
                return new SourceIdentifier() { InternalId = this.GetType().GUID, SourceName = "CarreOutput" };
            }
        }

        public List<OutputResult> Output(List<PhrData> outData)
        {
            List<OutputResult> output = new List<OutputResult>();
            

            var patients = outData.Select(p=>p.Patient).Distinct();
            foreach (var pat in patients) 
            {

                var authData = authProvider.GetAuthToken(pat);
                if (authData == null)
                {
                    continue;
                }

                foreach (PhrData p in outData.Where(i=>i.Patient.InternalId == pat.InternalId))
                {
                    foreach (DataUnit du in p.Data)
                    {

                        string provider = p.Source.InternalId.ToString();
                        string owl = string.Format(this._owl, du.OntologicClass);
                        string userUri = string.Format("{0}{1}", this._endpoint, authProvider.GetUserName(p.Patient));
                        string userMeasureUri = string.Format("{0}{1}/measurements/{2}_{3}", this._endpoint, authProvider.GetUserName(p.Patient), du.Source.SourceName, du.Datetime.Ticks.ToString());
                        string measurementType = string.Format("<http://www.w3.org/2000/01/rdf-schema#type> :individual_{0}_measurement", du.OntologicName);
                        string measuredBy = string.Format("<http://carre.kmi.open.ac.uk/ontology/sensors.owl#is_measured_by> <http://carre.kmi.open.ac.uk/manufacturers/{0}>", du.Source.SourceName);
                        string dateUri = string.Format("{0}_date", userMeasureUri);
                        string valueUri = string.Format("{0}_has_{1}", userMeasureUri, du.OntologicName);
                        string dateString = string.Format("<http://carre.kmi.open.ac.uk/ontology/sensors.owl#has_value> \"{0}\"^^<http://www.w3.org/2001/XMLSchema#datetime>", du.Datetime.ToUniversalTime().ToString("o"));
                        string valueString = string.Format("<http://carre.kmi.open.ac.uk/ontology/sensors.owl#has_value> \"{0}\"^^<http://www.w3.org/2001/XMLSchema#{1}>", du.Value, du.OntologicType);
                        string requestTriples = string.Format("<{0}> {1} .\n", userMeasureUri, measurementType);
                        requestTriples = string.Format("{0} <{1}> {2} .\n", requestTriples, userMeasureUri, measuredBy);
                        requestTriples = string.Format("{0} <{1}> {2} .\n", requestTriples, userMeasureUri, dateUri);
                        requestTriples = string.Format("{0} <{1}> <{2}> .\n", requestTriples, userMeasureUri, dateUri);
                        requestTriples = string.Format("{0} <{1}> <{1}_{2}> .\n", requestTriples, userMeasureUri, du.OntologicName);
                        requestTriples = string.Format("{0} <{1}> {2} .\n", requestTriples, dateUri, dateString);
                        requestTriples = string.Format("{0} <{1}_{2}> {3} .\n", requestTriples, userMeasureUri, valueUri, valueString);
                        string requestCheck = string.Format("SELECT ?val FROM <{0}> WHERE {{ <{1}> <{2}> ?val . }}", userUri, userMeasureUri, valueUri);
                        string requestInsert = string.Format("INSERT DATA {{ GRAPH <{0}> {{ {1} }} }}", userUri, requestTriples);

                        // check if the measureURI exists
                        var result = client.Post(new HttpRequestData("/query",null,"application/json","POST", string.Format("{{ token: \"{0}\", query: \"{1}\" }}", authData.Token, requestCheck)));
                        if (result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            output.Add(new OutputResult()
                            {
                                Data = p.Data,
                                Patient = p.Patient,
                                Source = p.Source,
                                Message = string.Format("ERROR:{0}", result.StatusCode.ToString()),
                                Success = false
                            });

                            continue;
                        }

                        if (result.Data == null || result.ContentLength == 0) {
                            // insert data.
                            result = client.Post(new HttpRequestData("/query",null,"application/json","POST", string.Format("{{ token: \"{0}\", query: \"{1}\" }}", authData.Token, requestInsert)));
                            if (result.StatusCode != System.Net.HttpStatusCode.OK || result.ContentLength == 0)
                            {
                                output.Add(new OutputResult()
                                {
                                    Data = p.Data,
                                    Patient = p.Patient,
                                    Source = p.Source,
                                    Message = result.StatusCode == System.Net.HttpStatusCode.OK ? string.Format("ERROR (BAD QUERY):{0}", requestInsert) : string.Format("ERROR:{0}", result.StatusCode.ToString()),
                                    Success = false
                                });

                                continue;
                            }
                            output.Add(new OutputResult()
                            {
                                Data = p.Data,
                                Patient = p.Patient,
                                Source = p.Source,
                                Message = "",
                                Success = true
                            });
                        }
                        
                        
                    }
                }
            }

            return output;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;

namespace Vulsk.CarrePhrAggregator.PhrPlugins
{
	using DataSpecification;
	using PatientSummary;

	public class PhrPluginVivaport : IPhrInput
	{
		public SourceIdentifier Source { get { return _sourceId; } }
        private readonly SourceIdentifier _sourceId = new SourceIdentifier
        {
            InternalId = typeof(PhrPluginVivaport).GUID,
            SourceName = "vivaport"
        };

        private Hashtable Documents = new Hashtable();
        private Hashtable DataMap = new Hashtable();
        private Hashtable DateMap = new Hashtable();
        private Hashtable TypeMap = new Hashtable();
        private Hashtable PatientMap = new Hashtable();

        /// <summary>
        /// Constructor of Vivaport PHR plugin
        /// </summary>
        public PhrPluginVivaport()
        {
            LoadMap();
            LoadDocuments();
            LoadPatientMap();
        }

        /// <summary>
        /// Constructor if Vivaport PHR plugin
        /// </summary>
        /// <param name="mapPath">value mapping configuration xml file</param>
        /// <param name="dataPath">patient data storage path</param>
        /// <param name="patientMapPath">optional patient mapping configuration xml</param>
        public PhrPluginVivaport(string mapPath, string dataPath, string patientMapPath = null)
        {
            LoadMap(mapPath);
            LoadDocuments(dataPath);
            if (patientMapPath != null)
            {
                LoadPatientMap(patientMapPath);
            }
            else
            {
                LoadPatientMap();
            }
        }

        /// <summary>
        /// Implementation of GetData method outlined in interface
        /// </summary>
        /// <param name="p">patient identifier</param>
        /// <param name="config">configuration parameters</param>
        /// <returns>PHR data</returns>
		public PhrData GetData(PatientIdentifier p, Configuration config)
		{
            PhrData retData = new PhrData();
            retData.Patient = p;
            retData.Source = this._sourceId;
            retData.Data = new List<DataUnit>();
            if (getPatientId(p.InternalId) == null)
            {
                return retData;
            }

            if (!Documents.ContainsKey(getPatientId(p.InternalId)))
            {
                return retData;
            }

            foreach (DataUnit du in config.DesiredData)
            {
                if (!this.DataMap.ContainsKey(du.OntologicName))
                {
                    continue;
                }
                
                foreach (XmlDocument x in (List<XmlDocument>)Documents[getPatientId(p.InternalId)])
                {
                    XDocument xd = XDocument.Parse(x.OuterXml);
                    var val = (IEnumerable)xd.XPathEvaluate(this.DataMap[du.OntologicName].ToString());
                    DateTime dt = new DateTime();
                    if (DateMap.ContainsKey(du.OntologicName))
                    {
                        var dateVal = (IEnumerable)xd.XPathEvaluate(this.DateMap[du.OntologicName].ToString());
                        if (dateVal.Cast<XText>().FirstOrDefault() != null)
                        {
                            var date = dateVal.Cast<XText>().FirstOrDefault().Value;
                            if (!DateTime.TryParse(date, out dt))
                            {
                                dt = DateTime.Parse("1900-01-01");
                            }
                        }
                        else 
                        { 
                            dt = DateTime.Parse("1900-01-01"); 
                        }
                    }
                    string type = "string";
                    if (TypeMap.ContainsKey(du.OntologicName))
                    {
                        type = TypeMap[du.OntologicName].ToString();
                    }
                    if (val.Cast<XText>().FirstOrDefault() == null) { continue; }
                    var value = val.Cast<XText>().FirstOrDefault().Value;
                    retData.Data.Add(new DataUnit() { OntologicName = du.OntologicName, Value = value, Name = du.Name, Identifier = this._sourceId.SourceName, Datetime = dt, OntologicType = type });
                }
            }
            return retData;
		}

        /// <summary>
        /// Gets patient mapping configuration
        /// </summary>
        /// <returns>patient map</returns>
        public Hashtable GetPatientMap()
        {
            return this.PatientMap;
        }
        /// <summary>
        /// Gets data map.
        /// </summary>
        /// <returns>data map</returns>
        public Hashtable GetMap()
        {
            return this.DataMap;
        }
        /// <summary>
        /// Gets date map.
        /// </summary>
        /// <returns>data map</returns>
        public Hashtable GetDateMap()
        {
            return this.DateMap;
        }
        /// <summary>
        /// Gets date map.
        /// </summary>
        /// <returns>data map</returns>
        public Hashtable GetTypeMap()
        {
            return this.TypeMap;
        }
        /// <summary>
        /// Gets documents loaded.
        /// </summary>
        /// <returns>documents loaded.</returns>
        public Hashtable GetDocuments()
        {
            return this.Documents;
        }

        /// <summary>
        /// loads patient map if it exists;
        /// </summary>
        protected void LoadPatientMap()
        {
            // TODO: better loading of xml...
            if (System.IO.File.Exists("../configuration/vivaportPatientMap.xml"))
            {
                LoadPatientMap("../configuration/vivaportPatientMap.xml");
            }
        }
        /// <summary>
        /// actual implementation of patient map loader.
        /// </summary>
        /// <param name="path">where it is to be loaded from</param>
        protected void LoadPatientMap(string path)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNodeList nodes = xdoc.SelectNodes("//Patient");
            foreach (XmlNode node in nodes)
            {
                if (!this.PatientMap.ContainsKey(node.SelectSingleNode("Internal").InnerText))
                {
                    this.PatientMap.Add(node.SelectSingleNode("Internal").InnerText, node.SelectSingleNode("Vivaport").InnerText);
                }
            }
        }

        /// <summary>
        /// Loads data map.
        /// </summary>
        protected void LoadMap()
        {
            // TODO: better loading of xml...
            if (System.IO.File.Exists("../configuration/vivaportMap.xml"))
            {
                LoadMap("../configuration/vivaportMap.xml");
            }
        }

        /// <summary>
        /// Actual implementation of data map loader.
        /// </summary>
        /// <param name="path">path of xml</param>
        protected void LoadMap(string path)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNodeList nodes = xdoc.SelectNodes("//DataEntry");
            foreach (XmlNode node in nodes)
            {
                if (!this.DataMap.ContainsKey(node.SelectSingleNode("OntologicName").InnerText))
                {
                    this.DataMap.Add(node.SelectSingleNode("OntologicName").InnerText, node.SelectSingleNode("XPath").InnerText);
                }
                if (!this.DateMap.ContainsKey(node.SelectSingleNode("OntologicName").InnerText) && node.SelectSingleNode("DateXPath") != null)
                {
                    this.DateMap.Add(node.SelectSingleNode("OntologicName").InnerText, node.SelectSingleNode("DateXPath").InnerText);
                }
                if (!this.TypeMap.ContainsKey(node.SelectSingleNode("OntologicName").InnerText) && node.SelectSingleNode("Type") != null)
                {
                    this.TypeMap.Add(node.SelectSingleNode("OntologicName").InnerText, node.SelectSingleNode("Type").InnerText);
                }
            }
        }

        /// <summary>
        /// Loads everything that vivaport export service exports...
        /// </summary>
        protected void LoadDocuments()
        {
            // TODO: better loading of xml...
            if (System.IO.Directory.Exists(@"C:\vivaport_data\"))
            {
                LoadDocuments(@"C:\vivaport_data\");
            }
        }

        /// <summary>
        /// Loads all (patient) documents in the path.
        /// </summary>
        /// <param name="path">base path of loading</param>
        /// <param name="patient">is the path a "patient" path?</param>
        protected void LoadDocuments(string path, bool patient = false)
        {
            if (!patient) {
                foreach (string s in System.IO.Directory.GetDirectories(path))
                {
                    LoadDocuments(s, true);
                }

                return;
            }

            foreach (string s in System.IO.Directory.GetFiles(path))
            {
                XmlDocument x = new XmlDocument();
                x.Load(s);
                LoadDocument(new Guid(System.IO.Directory.GetParent(s).Name), x);
            }
        }

        /// <summary>
        /// Loads a single xml document and assigns it to patient guid.
        /// </summary>
        /// <param name="patientID"> patient guid </param>
        /// <param name="document"> Xml document </param>
        protected void LoadDocument(Guid patientID, XmlDocument document)
        {
            if (this.Documents.ContainsKey(patientID.ToString()))
            {
                ((List<XmlDocument>)this.Documents[patientID.ToString()]).Add(document);
            }
            else
            {
                this.Documents.Add(patientID.ToString(), new List<XmlDocument>() { document }); 
            }
        }

        /// <summary>
        /// Patient mapping helper
        /// </summary>
        /// <param name="externalId">external patient guid</param>
        /// <returns>patient guid in vivaport</returns>
        protected string getPatientId(Guid externalId){
            if (this.PatientMap.Contains(externalId.ToString()))
                return (string)this.PatientMap[externalId.ToString()];
            return null;
        }
	}
}

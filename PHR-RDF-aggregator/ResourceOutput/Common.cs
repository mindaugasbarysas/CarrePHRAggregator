using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Reflection;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Vulsk.CarrePhrAggregator.DataSpecification;
using Pareto.Rest.Client.Http;
using Newtonsoft.Json;

namespace Vulsk.CarrePhrAggregator.ResourceOutput
{
    public class OutputResult: PhrData
    {
        public bool Success {get;set;}
        public string Message {get;set;}
    }
    
    public interface IOutput
    {
        SourceIdentifier OutputIdentifier { get; }

        List<OutputResult> Output(List<PhrData> outData);
    }

    public class CarreAuthToken
    {
        private string token { get; set; }
        public string Token { get { return token; } set { token = value; } }

        public string Username { get; set; }
    }

    public interface IAuthProvider
    {
        CarreAuthToken GetAuthToken(PatientIdentifier p);
        string GetUserName(PatientIdentifier p);
    }

    public class CarreAuthProvider : IAuthProvider
    {
        public string ConfigurationLocation { get { return configurationLocation; } set { configurationLocation = value; } }
        private string configurationLocation = "../configuration/authTokens.xml";
        private Hashtable authMap = null;
        /// <summary>
        /// actual implementation of patient map loader.
        /// </summary>
        /// <param name="path">where it is to be loaded from</param>
        protected void LoadAuthTokens()
        {
            this.authMap = new Hashtable();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(this.configurationLocation);
            XmlNodeList nodes = xdoc.SelectNodes("//User");
            foreach (XmlNode node in nodes)
            {
                if (!this.authMap.ContainsKey(node.SelectSingleNode("PatientId").InnerText))
                {
                    this.authMap.Add(
                        node.SelectSingleNode("PatientId").InnerText,
                        new CarreAuthToken()
                        {
                            Token = node.SelectSingleNode("AuthToken").InnerText,
                            Username = node.SelectSingleNode("Username").InnerText
                        }
                     );
                }
            }
        }
        public CarreAuthToken GetAuthToken(PatientIdentifier p)
        {
            CarreAuthToken t = new CarreAuthToken();
            if (authMap == null)
            {
                LoadAuthTokens();
            }
            if (this.authMap.ContainsKey(p.InternalId.ToString())) return (CarreAuthToken)authMap[p.InternalId.ToString()];
            
            return null;
        }

        public string GetUserName(PatientIdentifier p)
        {
            if (authMap == null)
            {
                LoadAuthTokens();
            }
            if (this.authMap.ContainsKey(p.InternalId.ToString())) return ((CarreAuthToken)authMap[p.InternalId.ToString()]).Username;
            
            return null;
        }

    }

}

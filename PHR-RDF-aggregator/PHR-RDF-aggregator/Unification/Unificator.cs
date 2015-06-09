using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;


namespace Vulsk.CarrePhrAggregator.Rum.Unification
{
    using DataSpecification;
    public class Unificator
    {
        public Unificator()
        {

        }

        private readonly SourceIdentifier _sourceId = new SourceIdentifier
        {
            InternalId = typeof(Unificator).GUID,
            SourceName = "Unificator"
        };

        private readonly UnificationRules _unificationRules = new UnificationRules() { SourcePriority = new List<SourcePriority>() };

        /// <summary>
        /// Gets current unification rules.
        /// </summary>
        /// <returns></returns>
        public UnificationRules GetUnificationRules()
        {
            return this._unificationRules;
        }

        /// <summary>
        /// Loads unification configuration
        /// </summary>
        /// <param name="path">path to xml</param>
        public void LoadUnificationRules(string path)
        {
            if (path == null)
            {
                path = "../configuration/UnificationRules.xml";
            }
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNodeList nodes = xdoc.SelectNodes("//Source");
            foreach (XmlNode node in nodes)
            {
                this._unificationRules.SourcePriority.Add(
                    new SourcePriority()
                    {
                        Priority = Convert.ToInt32(node.SelectSingleNode("Priority").InnerText),
                        NewerRecordPriority = Convert.ToInt32(node.SelectSingleNode("NewerRecordPriority").InnerText),
                        Source = new SourceIdentifier()
                        {
                            SourceName = node.SelectSingleNode("SourceName").InnerText,
                            InternalId = new Guid(node.SelectSingleNode("SourceGuid").InnerText)
                        }
                    }
                );
            }
        }

        /// <summary>
        /// Unifies data
        /// </summary>
        /// <param name="datum">data to unify</param>
        /// <returns>Unified data</returns>
        public List<PhrData> Unify(List<PhrData> datum)
        {
            var patients = datum.Select(p => p.Patient).Distinct();
            var sources = datum.Select(s => s.Source).Distinct();


            List<PhrData> data = new List<PhrData>();

            foreach (PatientIdentifier patient in patients)
            {
                PhrData patientData = new PhrData()
                {
                    Patient = patient,
                    Source = this._sourceId,
                    Data = new List<DataUnit>()
                };
                foreach (SourceIdentifier source in sources)
                {
                    foreach (PhrData phrd in datum.Where(pd => pd.Source == source && pd.Patient == patient))
                    {
                        foreach (DataUnit du in phrd.Data)
                        {
                            if (patientData.Data.Exists(d => d.Identifier == du.Identifier))
                            {
                                //resolve conflict.
                                var existing = patientData.Data.Where(d => d.Identifier == du.Identifier).First();
                                var rule_conflicting = this.GetUnificationRules().SourcePriority.Where(sp => sp.Source.InternalId == source.InternalId).FirstOrDefault();
                                var rule_existing = this.GetUnificationRules().SourcePriority.Where(sp => sp.Source.InternalId == existing.Source.InternalId).FirstOrDefault();
                                if (existing.Value == du.Value)
                                {
                                    continue;
                                }

                                if (rule_conflicting == null || rule_existing == null)
                                {
                                    if (existing.Datetime < du.Datetime)
                                    {
                                        existing.Value = du.Value;
                                        existing.Source = source;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (rule_conflicting == null)
                                    {
                                        continue; //not defined - no priority
                                    }

                                    if (rule_existing == null) // undefined priority for existing? overwrite!
                                    {
                                        existing.Value = du.Value;
                                        existing.Source = source;
                                        continue;
                                    }
                                    // existing record source priority is lower than conflicting and the existing record is not newer
                                    if (rule_existing.Priority >= rule_conflicting.Priority && existing.Datetime >= du.Datetime)
                                    {
                                        existing.Value = du.Value;
                                        existing.Source = source;
                                        continue;
                                    }

                                    // existing record is older and existing record source time priority is lower
                                    if (rule_existing.NewerRecordPriority >= rule_conflicting.NewerRecordPriority && existing.Datetime < du.Datetime)
                                    {
                                        existing.Value = du.Value;
                                        existing.Source = source;
                                        continue;
                                    }
                                }

                            }
                            else
                            {
                                du.Source = source;
                                patientData.Data.Add(du);
                            }
                        }
                    }
                }
                data.Add(patientData);
            }

            return data;
        }
    }
}

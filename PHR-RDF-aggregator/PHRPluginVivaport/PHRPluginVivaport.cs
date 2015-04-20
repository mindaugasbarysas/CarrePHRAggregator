using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Vulsk.CarrePhrAggregator.PhrPlugins
{
	using DataSpecification;
	using PatientSummary;

	public class PhrPluginVivaport : IPhrInput
	{
		private readonly SourceIdentifier _sourceId = new SourceIdentifier {
			InternalId = typeof(PhrPluginVivaport).GUID,
			SourceName = "Vivaport.eu"
		};
		public SourceIdentifier Source { get { return _sourceId; } }

		public PhrData GetData(PatientIdentifier p)
		{
			var ret = new PhrData { Data = new List<DataUnit>(), Patient = p, Source = Source };

			using (var client = new HttpClient()) {
				client.BaseAddress = new Uri(ConfigurationManager.AppSettings["VivaportApiUri"]);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				//get patient personal information
				var response = client.GetAsync(string.Format("api/PatientSummary/{0}", p.InternalId)).Result;
				if (response.IsSuccessStatusCode) {
					var persInfo = response.Content.ReadAsAsync<PersonalInformation>().Result;
					ret.Data.Add(new DataUnit {
						Datetime = DateTime.UtcNow,
						Name = "Name",
						OntologicName = "rdf:Name",
						Value = persInfo.contactInfo.sName
					});
					ret.Data.Add(new DataUnit {
						Datetime = persInfo.dtLastUpdate,
						Name = "Birth date",
						OntologicName = "rdf:BDate",
						Value = persInfo.dtDateofBirth
					});
					ret.Data.Add(new DataUnit {
						Datetime = persInfo.dtLastUpdate,
						Name = "Gender",
						OntologicName = "rdf:Gender",
						Value = persInfo.sGender
					});
					ret.Data.Add(new DataUnit {
						Datetime = persInfo.dtLastUpdate,
						Name = "Country",
						OntologicName = "rdf:Country",
						Value = persInfo.ciMyContacts.FirstOrDefault(c => c.adress.sCountry != null)
					});
				}

				//get diagnosis
				response = client.GetAsync(string.Format("api/Diagnosis/{0}", p.InternalId)).Result;
				if (response.IsSuccessStatusCode) {
					var diagnosis = response.Content.ReadAsAsync<ProblemRCI>().Result;
					ret.Data.Add(new DataUnit {
						Datetime = diagnosis.dtDate,
						Name = "ICD",
						OntologicName = "rdf:ICD",
						Value = diagnosis.sICD
					});
					ret.Data.Add(new DataUnit {
						Datetime = diagnosis.dtDate,
						Name = "Description",
						OntologicName = "rdf:Description",
						Value = diagnosis.sDiagnosisDescription
					});
					ret.Data.Add(new DataUnit {
						Datetime = diagnosis.dtDate,
						Name = "OnsetDate",
						OntologicName = "rdf:OnsetDate",
						Value = diagnosis.sOnsetDate
					});
					ret.Data.Add(new DataUnit {
						Datetime = diagnosis.dtDate,
						Name = "EndDate",
						OntologicName = "rdf:EndDate",
						Value = diagnosis.sendDate
					});
					ret.Data.Add(new DataUnit {
						Datetime = diagnosis.dtDate,
						Name = "ResolutionCircumstances",
						OntologicName = "rdf:ResolutionCircumstances",
						Value = diagnosis.sResolution
					});
				}

				//get medication
				response = client.GetAsync(string.Format("api/Diagnosis/{0}", p.InternalId)).Result;
				if (response.IsSuccessStatusCode) {
					var medication = response.Content.ReadAsAsync<Medication>().Result;
					foreach (var m in medication.medicineList)
					{
						ret.Data.Add(new DataUnit {
							Datetime = m.dtLastUpdate,
							Name = "Medication",
							OntologicName = "rdf:Medication",
							Value = m.sActiveIngredient
						});
						ret.Data.Add(new DataUnit {
							Datetime = m.dtLastUpdate,
							Name = "Dose",
							OntologicName = "rdf:Dose",
							Value = m.sDose
						});
						ret.Data.Add(new DataUnit {
							Datetime = m.dtLastUpdate,
							Name = "TimesPerDay",
							OntologicName = "rdf:TimesPerDay",
							Value = m.sFrequency
						});
						ret.Data.Add(new DataUnit {
							Datetime = m.dtLastUpdate,
							Name = "StartDate",
							OntologicName = "rdf:StartDate",
							Value = m.dtStarted
						});
						ret.Data.Add(new DataUnit {
							Datetime = m.dtLastUpdate,
							Name = "EndDate",
							OntologicName = "rdf:EndDate",
							Value = m.dtEnded
						});
					}
				}
			}

			return ret;
		}
	}
}

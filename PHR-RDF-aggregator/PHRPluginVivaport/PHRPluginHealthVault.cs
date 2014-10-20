using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Health;
using Microsoft.Health.Web;
using Microsoft.Health.PatientConnect;
using Microsoft.Health.Authentication;
using Microsoft.Health.Web.Authentication;
using Microsoft.Health.ItemTypes;
using VULSK.CarrePHRAggregator.DataSpecification;
using Limilabs.Mail;

namespace VULSK.CarrePHRAggregator.PHRInputHealthVault
{
	public class PHRPluginHealthVault
	{
		private Guid ClientID = new Guid("8AE962DB-D869-4E84-9B39-1FF38F235006");
		private Guid MasterID = new Guid("99C20788-9FE4-496E-A149-8BB210A01C66");
		private Uri ShellURI = new Uri("https://account.healthvault-ppe.co.uk/");
		private Uri PlatformURI = new Uri("https://platform.healthvault-ppe.co.uk/platform");

		private SourceIdentifier _sourceID = new SourceIdentifier()
		{
			InternalId = typeof(PHRPluginHealthVault).GetType().GUID,
			SourceName = "Microsoft HealthVault"
		};
		public SourceIdentifier Source { get { return _sourceID; } }


		public PHRData GetData(PatientIdentifier p)
		{
			HealthClientApplication clientApp = HealthClientApplication.Create(ClientID, MasterID, ShellURI, PlatformURI);

			if (clientApp.GetApplicationInfo() == null)
			{
				// Create a new client instance.                  
				clientApp.StartApplicationCreationProcess();
				return null;
			}

			PersonInfo pi = clientApp.ApplicationConnection.GetAuthorizedPeople().Where(k => k.PersonId == p.InternalId).FirstOrDefault();
			if (pi == null)
			{
				clientApp.StartUserAuthorizationProcess();
				return null; // not authorized;
			}

			HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(pi.PersonId);
			HealthRecordAccessor access = new HealthRecordAccessor(authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

			PHRData ret = new PHRData() { Data = new List<DataUnit>(), Patient = p, Source = this.Source };
			ret.Data.Add(new DataUnit() { Name = "Name", OntologicName = "rdf:Name", Value = pi.Name });
			Basic b = GetSingleValue<Basic>(Basic.TypeId, access);
			ret.Data.Add(new DataUnit()
			{
				Name = "Birth date",
				OntologicName = "rdf:BDate",
				Value = (b != null) ? b.BirthYear : -1
			});
			ret.Data.Add(new DataUnit()
			{
				Name = "Gender",
				OntologicName = "rdf:Gender",
				Value = (b != null) ?
				(b.Gender.Value == Gender.Male) ? "male" : (b.Gender.Value == Gender.Female) ? "female" : "unknown" : "unknown"
			}); // sorry about readability, really, this could have been much much much more elegant
			ret.Data.Add(new DataUnit()
			{
				Name = "Country",
				OntologicName = "rdf:Country",
				Value = (b != null) ? b.Country : "unknown"
			});
			List<Height> heights = GetValues<Height>(Height.TypeId, access);
			foreach (Height h in heights)
			{
				ret.Data.Add(new DataUnit()
				{
					Datetime = h.EffectiveDate,
					Name = "Height_meters",
					OntologicName = "rdf:Height",
					Value = h.Value.Meters
				});
			}

			List<Weight> weights = GetValues<Weight>(Weight.TypeId, access);
			foreach (Weight w in weights)
			{
				ret.Data.Add(new DataUnit()
				{
					Datetime = w.EffectiveDate,
					Name = "weight_kg",
					OntologicName = "rdf:Height",
					Value = w.Value.Kilograms
				});
			}


			return ret;
		}

		private T GetSingleValue<T>(Guid typeID, HealthRecordAccessor access) where T : class
		{
			HealthRecordSearcher searcher = access.CreateSearcher();

			HealthRecordFilter filter = new HealthRecordFilter(typeID);
			searcher.Filters.Add(filter);

			HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

			if (items != null && items.Count > 0)
			{
				return items[0] as T;
			}
			else
			{
				return null;
			}
		}

		private List<T> GetValues<T>(Guid typeID, HealthRecordAccessor access) where T : HealthRecordItem
		{
			HealthRecordSearcher searcher = access.CreateSearcher();

			HealthRecordFilter filter = new HealthRecordFilter(typeID);
			searcher.Filters.Add(filter);

			HealthRecordItemCollection items = searcher.GetMatchingItems()[0];

			List<T> typedList = new List<T>();

			foreach (HealthRecordItem item in items)
			{
				typedList.Add((T)item);
			}

			return typedList;
		}
	}
}

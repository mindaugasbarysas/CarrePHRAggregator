using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Health;
using Microsoft.Health.ItemTypes;

namespace Vulsk.CarrePhrAggregator.PhrPlugins
{
	using DataSpecification;

    public class PhrPluginHealthVault : IPhrInput
	{
		private readonly Guid _clientId = new Guid("10EA5F9A-7013-404F-9CFC-17332D724A57");
		private readonly Guid _masterId = new Guid("99C20788-9FE4-496E-A149-8BB210A01C66");
		private readonly Uri _shellUri = new Uri("https://account.healthvault-ppe.co.uk/");
		private readonly Uri _platformUri = new Uri("https://platform.healthvault-ppe.co.uk/platform");

		private readonly SourceIdentifier _sourceId = new SourceIdentifier {
			InternalId = typeof(PhrPluginHealthVault).GUID,
			SourceName = "Microsoft HealthVault"
		};
		public SourceIdentifier Source { get { return _sourceId; } }


		public PhrData GetData(PatientIdentifier p)
		{
			var clientApp = HealthClientApplication.Create(_clientId, _masterId, _shellUri, _platformUri);

			if (clientApp.GetApplicationInfo() == null)
			{
				// Create a new client instance.                  
				clientApp.StartApplicationCreationProcess();
				return null;
			}

			var ap = clientApp.ApplicationConnection.GetAuthorizedPeople().ToList();

            var pi = clientApp.ApplicationConnection.GetAuthorizedPeople().FirstOrDefault(k => k.PersonId == p.InternalId);
			if (pi == null)
			{
				clientApp.StartUserAuthorizationProcess();
				return null; // not authorized;
			}

			var authConnection = clientApp.CreateAuthorizedConnection(pi.PersonId);
			var access = new HealthRecordAccessor(authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

			var ret = new PhrData { Data = new List<DataUnit>(), Patient = p, Source = Source };
			ret.Data.Add(new DataUnit { Datetime = DateTime.UtcNow, Name = "Name", OntologicName = "rdf:Name", Value = pi.Name });
			var b = GetSingleValue<Basic>(Basic.TypeId, access);
			ret.Data.Add(new DataUnit {
				Datetime = b.EffectiveDate,
				Name = "Birth date",
				OntologicName = "rdf:BDate",
				Value = b.BirthYear
			});
			var gender = b.Gender ?? Gender.Unknown;
            ret.Data.Add(new DataUnit {
				Datetime = b.EffectiveDate,
				Name = "Gender",
				OntologicName = "rdf:Gender",
				Value = gender == Gender.Male ? "male" : gender == Gender.Female ? "female" : "unknown"
			});
			ret.Data.Add(new DataUnit {
				Datetime = b.EffectiveDate,
				Name = "Country",
				OntologicName = "rdf:Country",
				Value = b.Country
			});
			var heights = GetValues<Height>(Height.TypeId, access);
			foreach (var h in heights)
			{
				ret.Data.Add(new DataUnit {
					Datetime = h.EffectiveDate,
					Name = "Height_meters",
					OntologicName = "rdf:Height",
					Value = h.Value.Meters
				});
			}

			var weights = GetValues<Weight>(Weight.TypeId, access);
			foreach (var w in weights)
			{
				ret.Data.Add(new DataUnit {
					Datetime = w.EffectiveDate,
					Name = "weight_kg",
					OntologicName = "rdf:Height",
					Value = w.Value.Kilograms
				});
			}


			return ret;
		}

		private static T GetSingleValue<T>(Guid typeId, HealthRecordAccessor access) where T : class
		{
			var searcher = access.CreateSearcher();

			var filter = new HealthRecordFilter(typeId);
			searcher.Filters.Add(filter);

			var items = searcher.GetMatchingItems()[0];

			if (items != null && items.Count > 0)
			{
				return items[0] as T;
			}
			return null;
		}

		private static List<T> GetValues<T>(Guid typeId, HealthRecordAccessor access) where T : HealthRecordItem
		{
			var searcher = access.CreateSearcher();

			var filter = new HealthRecordFilter(typeId);
			searcher.Filters.Add(filter);

			var items = searcher.GetMatchingItems()[0];

			return items.Cast<T>().ToList();
		}
	}
}

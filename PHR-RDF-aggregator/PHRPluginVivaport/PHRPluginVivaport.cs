namespace Vulsk.CarrePhrAggregator.PhrPlugins
{
	using DataSpecification;

	public class PhrPluginVivaport : IPhrInput
	{

		private readonly SourceIdentifier _sourceId = new SourceIdentifier {
			InternalId = typeof(PhrPluginVivaport).GUID,
			SourceName = "Vivaport.eu"
		};
		public SourceIdentifier Source { get { return _sourceId; } }

		public PhrData GetData(PatientIdentifier p)
		{
			var retData = new PhrData();

			return retData;
		}
	}
}

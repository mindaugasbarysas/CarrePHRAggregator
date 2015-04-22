using System.Collections.Generic;

namespace Vulsk.CarrePhrAggregator
{
	using DataSpecification;

	/// <summary>
	/// Generic interface for PHR data input
	/// </summary>
	public interface IPhrInput
	{
		/// <summary>
		/// Must be set!
		/// </summary>
		SourceIdentifier Source {get;}
        PhrData GetData(PatientIdentifier p, Configuration configuration);
	}
}

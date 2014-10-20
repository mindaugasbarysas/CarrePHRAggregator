using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VULSK.CarrePHRAggregator.DataSpecification;

namespace VULSK.CarrePHRAggregator.PHRInput
{
	/// <summary>
	/// Generic interface for PHR data input
	/// </summary>
	public interface IPHRInput
	{
		/// <summary>
		/// Must be set!
		/// </summary>
		SourceIdentifier Source {get;}
		PHRData GetData(PatientIdentifier p);
	}
}

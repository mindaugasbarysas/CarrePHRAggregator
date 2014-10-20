using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VULSK.CarrePHRAggregator.DataSpecification
{
	/// <summary>
	/// Single data bit from any repo
	/// </summary>
	public class DataUnit
	{
		public DateTime Datetime { get;set;}
		public string Name { get; set; }
		public object Value { get; set; }
		public string OntologicName { get; set; }

	}

	/// <summary>
	/// Data collection with source and patient identification
	/// </summary>
	public class PHRData
	{
		public PatientIdentifier Patient { get; set; }
		public SourceIdentifier Source { get; set; }
		public List<DataUnit> Data { get; set; }
	}

	/// <summary>
	/// Identifies a patient (e.g. in MPI)
	/// </summary>
	public class PatientIdentifier
	{
		public Guid InternalId { get; set; }
		// could be expanded in the future
	}

	/// <summary>
	/// Identifies a data source
	/// </summary>
	public class SourceIdentifier
	{
		public Guid InternalId { get; set; }
		public string SourceName { get; set; }
	}
}

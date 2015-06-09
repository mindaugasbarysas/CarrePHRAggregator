using System;
using System.Collections.Generic;

namespace Vulsk.CarrePhrAggregator.DataSpecification
{
	/// <summary>
	/// Single data bit from any repo
	/// </summary>
	public class DataUnit
	{
		public DateTime Datetime { get; set; }
		public string Name { get; set; }
		public object Value { get; set; }
		public string OntologicName { get; set; }
        public string OntologicClass { get; set; }
        public string OntologicType { get; set; }
        public string Identifier { get; set; }
        public SourceIdentifier Source { get; set; }
	}

	/// <summary>
	/// Data collection with source and patient identification
	/// </summary>
	public class PhrData
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

    /// <summary>
    /// Configuration object passed to RUM modules.
    /// </summary>
    public class Configuration
    {
        public List<SourceIdentifier> Sources { get; set; }
        public List<DataUnit> DesiredData { get; set; }
    }

    /// <summary>
    /// Source priority rule item
    /// </summary>
    public class SourcePriority
    {
        public int Priority { get; set; }
        public int NewerRecordPriority { get; set; }
        public SourceIdentifier Source { get; set; }
    }
    /// <summary>
    /// Lists unification sources by priority
    /// </summary>
    public class UnificationRules
    {
        public List<SourcePriority> SourcePriority { get; set; }
        // Todo: add more unification rules
    }
}

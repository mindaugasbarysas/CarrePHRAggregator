using System.Collections.Generic;
using Vulsk.CarrePhrAggregator.DataSpecification;

namespace Vulsk.CarrePhrAggregator.ResourceConfiguration
{
    /// <summary>
    /// Interface defining the configuration plugin structure for the RUM.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Should this configuration run in production?
        /// </summary>
        bool fake { get; }
        /// <summary>
        /// Configuration source identifier.
        /// </summary>
        SourceIdentifier Source { get; }
        /// <summary>
        /// Method to get a configuration.
        /// </summary>
        /// <returns>Configuration</returns>
       Configuration GetConfiguration();
    }
}

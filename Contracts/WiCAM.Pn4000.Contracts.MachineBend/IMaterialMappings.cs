using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IMaterialMappings
{
	List<IMaterialMapping> Mappings { get; }
}

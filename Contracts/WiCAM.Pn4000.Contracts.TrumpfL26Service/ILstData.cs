using System.Collections;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface ILstData : IDictionary<int, object>, ICollection<KeyValuePair<int, object>>, IEnumerable<KeyValuePair<int, object>>, IEnumerable
{
	IEnumerable<string> Text { get; set; }
}

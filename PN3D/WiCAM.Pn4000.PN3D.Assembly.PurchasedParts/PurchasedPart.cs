using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiCAM.Pn4000.Contracts.Assembly;

namespace WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;

public class PurchasedPart : IPrefabricatedPart
{
	public string Name { get; set; }

	public int Type { get; set; } = 1;

	public bool IsMountedBeforeBending { get; set; }

	[JsonIgnore]
	public bool IgnoreAtCollision
	{
		get
		{
			return !this.IsMountedBeforeBending;
		}
		set
		{
			this.IsMountedBeforeBending = !value;
		}
	}

	public IEnumerable<(string propName, string propValue)> AdditionalProperties { get; set; }

	public IPrefabricatedPart.SearchTypes NameSearchType { get; set; }

	public string AdditionalInfos => string.Join(Environment.NewLine, this.AdditionalProperties?.Select(((string propName, string propValue) x) => x.propName + ": " + x.propValue) ?? new List<string>());

	public PurchasedPart Clone()
	{
		return new PurchasedPart
		{
			Name = this.Name,
			Type = this.Type,
			IsMountedBeforeBending = this.IsMountedBeforeBending,
			NameSearchType = this.NameSearchType
		};
	}
}

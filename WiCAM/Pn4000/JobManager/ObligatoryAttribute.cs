using System;

namespace WiCAM.Pn4000.JobManager;

public class ObligatoryAttribute : Attribute
{
	public bool IsObligatory { get; set; }

	public ObligatoryAttribute()
		: this(value: true)
	{
	}

	public ObligatoryAttribute(bool value)
	{
		IsObligatory = value;
	}
}

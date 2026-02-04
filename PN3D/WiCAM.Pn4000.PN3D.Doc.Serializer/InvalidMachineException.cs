using System;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

[Serializable]
public class InvalidMachineException : Exception
{
	public InvalidMachineException()
	{
	}

	public InvalidMachineException(string? message)
		: base(message)
	{
	}

	public InvalidMachineException(string? message, Exception? inner)
		: base(message, inner)
	{
	}
}

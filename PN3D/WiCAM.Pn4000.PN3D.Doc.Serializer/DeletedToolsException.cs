using System;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

public class DeletedToolsException : Exception
{
	public DeletedToolsException()
	{
	}

	public DeletedToolsException(string? message)
		: base(message)
	{
	}

	public DeletedToolsException(string? message, Exception? inner)
		: base(message, inner)
	{
	}
}

using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface IPnBndDocToDocConverter
{
	IDoc3d Convert(IPnBndDoc doc)
	{
		return doc as IDoc3d;
	}
}

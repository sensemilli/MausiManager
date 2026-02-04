using System;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.FingerCalculation;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.PN3D.FingerStop;

public class FingerBendInfoConverter : IFingerBendInfoConverter
{
	public IFingerBendInfo Convert(ICombinedBendDescriptorInternal bendDescriptor)
	{
		throw new NotImplementedException();
	}
}

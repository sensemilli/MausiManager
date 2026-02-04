using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.FingerCalculation;

public interface IFingerBendInfoConverter
{
	IFingerBendInfo Convert(ICombinedBendDescriptorInternal bendDescriptor);
}

using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class AsmComAdapter
{
	public static int AsmComCount
	{
		get
		{
			return ASMCOM.asmcom_get_iasakt();
		}
		set
		{
			ASMCOM.asmcom_set_iasakt(value);
		}
	}

	public static void SetElement(int idx, AsmComEntity entity)
	{
		int num = 80;
		int idx2 = idx * num;
		MarshalString.ToIntPtrAtFortranStyle(entity.PartName, ASMCOM.getcharaddr_asname(idx2), num);
		ASMCOM.set_iasarc(idx, entity.Iasarc);
		ASMCOM.set_xasref(idx, entity.LengthX);
		ASMCOM.set_yasref(idx, entity.LengthY);
		ASMCOM.set_zasref(idx, entity.LengthZ);
		ASMCOM.set_xasrot(idx, entity.PartId);
		ASMCOM.set_yasrot(idx, entity.InstanceNumber);
		ASMCOM.set_zasrot(idx, entity.Zasrot);
		ASMCOM.set_iasanz(idx, entity.Iasanz);
		MarshalString.ToIntPtrAtFortranStyle(entity.PartType, ASMCOM.getcharaddr_astamm(idx2), num);
		ASMCOM.set_rasdik(idx, entity.Thickness);
	}
}

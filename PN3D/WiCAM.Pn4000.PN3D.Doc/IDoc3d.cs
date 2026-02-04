using System;
using System.Collections.Generic;
using BendDataSourceModel;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.PN3D.Doc;

public interface IDoc3d : IPnBndDoc, IPnBndDocExt
{
	IDocMetadata MetaData { get; }

	[Obsolete]
	new IBendMachineSimulation? BendMachineConfig { get; set; }

	PnBndFile DiskFile { get; set; }

	BdsmDataModel PpModel { get; set; }

	int AmountInAssembly { get; set; }

	global::WiCAM.Pn4000.PN3D.Assembly.Assembly Assembly { get; set; }

	IFrontCalls FrontCalls { get; }

	List<UserProperty> UserProperties { get; set; }

	event Action<IDoc3d> FreezeCombinedBendDescriptorsChanged;

	event Action<IDoc3d> PnMaterialByUserChanged;

	event Action<IDoc3d, List<ValidationResult>> ValidationResultChanged;

	event Action<IDoc3d> UpdateBendDataInfoEvent;

	event Action<IDoc3d> DocUpdated;

	event Action<IDoc3d> UpdateGeneralInfoAutoEvent;

	event Action<IDoc3d, ModelViewMode> UpdateGeneralInfoEvent;

	event Action<IDoc3d> UpdateBendMachineInfoEvent;

	bool MoveBend(int bendIndexOld, int bendIndexNew);

	void SetUserDefinedTool(int bendIdx, int? punchId, int? dieId, IPreferredProfile newPP);

	void UpdateGeneralInfo(ModelViewMode viewMode);

	void ColorNoDeductionValueFoundBendZones(Model model, ModelColors3DConfig modelColors3D, UiModelType modelType);

	IDoc3d GetCloneForBendSequence();

	IDoc3d Copy(IPnBndDocCopyOptions options);

	void SetFingerPos(ICombinedBendDescriptorInternal? cbd, IFingerStopPointInternal leftFingerPos, IFingerStopPointInternal rightFingerPos, FingerPositioningMode mode);
}

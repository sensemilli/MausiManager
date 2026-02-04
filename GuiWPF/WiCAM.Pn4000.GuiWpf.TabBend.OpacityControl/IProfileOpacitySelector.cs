using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.PaintTools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.OpacityControl;

public interface IProfileOpacitySelector
{
	event Action<int, int, Dictionary<PartRole, double>> OpacityProfileChanged;

	void LoadOpacityProfileFromConfig(int? n = null);

	void InitOpacityProfiles();

	void ChangeOpacity(List<PartRole> affectedRoles, double opacity);

	void RemoveCurrentProfile();

	void AddAProfile();

	void SaveCameraState();

	void EraseSavedCameraState();

	bool CanSaveCameraState();

	bool CanEraseSavedCameraState();

	void ColorModelParts(IPaintTool paintTool);
}

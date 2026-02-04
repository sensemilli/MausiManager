using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.GuiWpf.TabBend.OpacityControl;

internal class ProfileOpacitySelector : IProfileOpacitySelector
{
	private Dictionary<PartRole, double> _partRoleOpacities = new Dictionary<PartRole, double>();

	private int _numOfProfiles = 1;

	private int _currentProfileNum;

	private OpacityProfile _currentProfileCached;

	private readonly IConfigProvider _configProvider;

	private readonly IScreen3DMain _screen3D;

	private readonly ICurrentDocProvider _docProvider;

	private readonly IMachinePainter _machinePainter;

	public event Action<int, int, Dictionary<PartRole, double>> OpacityProfileChanged;

	public ProfileOpacitySelector(IConfigProvider configProvider, IScreen3DMain screen3D, ICurrentDocProvider docProvider, IMachinePainter machinePainter)
	{
		_configProvider = configProvider;
		_screen3D = screen3D;
		_docProvider = docProvider;
		_machinePainter = machinePainter;
	}

	public void LoadOpacityProfileFromConfig(int? n = null)
	{
		GeneralUserSettingsConfig config = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		CheckIfValid(config);
		config = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		List<OpacityProfile> opacityProfiles = config.OpacityProfiles;
		_numOfProfiles = config.OpacityProfiles.Count;
		int valueOrDefault = n.GetValueOrDefault();
		if (!n.HasValue)
		{
			valueOrDefault = config.OpacityProfileSelected;
			n = valueOrDefault;
		}
		_currentProfileNum = Math.Clamp(n.Value, 0, _numOfProfiles - 1);
		_currentProfileCached = opacityProfiles[n.Value];
		Dictionary<PartRole, double> dictionary = new Dictionary<PartRole, double>();
		for (int i = 0; i < _currentProfileCached.Opacities.Length; i++)
		{
			double value = _currentProfileCached.Opacities[i];
			dictionary.Add((PartRole)i, value);
		}
		_partRoleOpacities = dictionary;
		config.OpacityProfileSelected = _currentProfileNum;
		if (_currentProfileCached.HasCameraSaved)
		{
			if (_currentProfileCached.CameraState2 is CameraState cameraState)
			{
				_screen3D.ScreenD3D.Renderer.ImportCameraState(cameraState);
			}
			_screen3D.ScreenD3D.ProjectionType = (ProjectionType)_currentProfileCached.ProjectionType;
			_screen3D.ScreenD3D.Render(skipQueuedFrames: false);
		}
		_configProvider.Push(config);
		_configProvider.Save<GeneralUserSettingsConfig>();
		this.OpacityProfileChanged(_currentProfileNum, _numOfProfiles, _partRoleOpacities);
	}

	public void InitOpacityProfiles()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.OpacityProfiles = new List<OpacityProfile>();
		generalUserSettingsConfig.OpacityProfileSelected = 0;
		_numOfProfiles = 1;
		_currentProfileNum = 0;
		for (int i = 0; i < _numOfProfiles; i++)
		{
			double[] array = new double[System.Enum.GetValues(typeof(PartRole)).Length];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = 1.0;
			}
			generalUserSettingsConfig.OpacityProfiles.Add(new OpacityProfile(array));
		}
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
	}

	public void ChangeOpacity(List<PartRole> affectedRoles, double opacity)
	{
		GeneralUserSettingsConfig config = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		CheckIfValid(config);
		config = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_currentProfileNum = config.OpacityProfileSelected;
		List<OpacityProfile> opacityProfiles = config.OpacityProfiles;
		_currentProfileCached = opacityProfiles[_currentProfileNum];
		foreach (PartRole affectedRole in affectedRoles)
		{
			_currentProfileCached.Opacities[(int)affectedRole] = opacity;
		}
		_configProvider.Push(config);
		_configProvider.Save<GeneralUserSettingsConfig>();
		Dictionary<PartRole, double> dictionary = new Dictionary<PartRole, double>();
		for (int i = 0; i < _currentProfileCached.Opacities.Length; i++)
		{
			dictionary.Add((PartRole)i, _currentProfileCached.Opacities[i]);
		}
		_partRoleOpacities = dictionary;
		_numOfProfiles = opacityProfiles.Count;
		this.OpacityProfileChanged(_currentProfileNum, _numOfProfiles, dictionary);
	}

	public void RemoveCurrentProfile()
	{
		if (_numOfProfiles - 1 >= 1)
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			List<OpacityProfile> opacityProfiles = generalUserSettingsConfig.OpacityProfiles;
			opacityProfiles.RemoveAt(_currentProfileNum);
			_configProvider.Push(generalUserSettingsConfig);
			_configProvider.Save<GeneralUserSettingsConfig>();
			_numOfProfiles = opacityProfiles.Count;
			LoadOpacityProfileFromConfig(Math.Min(_numOfProfiles - 1, _currentProfileNum));
		}
	}

	public void AddAProfile()
	{
		if (_numOfProfiles + 1 <= 16)
		{
			GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
			List<OpacityProfile> opacityProfiles = generalUserSettingsConfig.OpacityProfiles;
			double[] array = new double[System.Enum.GetValues(typeof(PartRole)).Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 1.0;
			}
			opacityProfiles.Insert(_currentProfileNum + 1, new OpacityProfile(array));
			_configProvider.Push(generalUserSettingsConfig);
			_configProvider.Save<GeneralUserSettingsConfig>();
			_numOfProfiles = opacityProfiles.Count;
			LoadOpacityProfileFromConfig(_currentProfileNum + 1);
		}
	}

	public void SaveCameraState()
	{
        try
        {
            // Sicherer Zugriff auf Screen3D
            if (_screen3D?.ScreenD3D?.Renderer == null)
            {
                return;
            }
            GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_currentProfileCached = generalUserSettingsConfig.OpacityProfiles[_currentProfileNum];
		_currentProfileCached.HasCameraSaved = true;
		_currentProfileCached.CameraState2 = _screen3D.ScreenD3D.Renderer.ExportCameraState();
		_currentProfileCached.ProjectionType = (int)_screen3D.ScreenD3D.Renderer.ProjectionType;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
        }
        catch (ObjectDisposedException)
        {
            // Screen wurde bereits disposed - Silent fail
        }
    }

	public void EraseSavedCameraState()
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_currentProfileCached = generalUserSettingsConfig.OpacityProfiles[_currentProfileNum];
		_currentProfileCached.HasCameraSaved = false;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
	}

	public bool CanSaveCameraState()
	{
        try
        {
            // Erst prüfen ob überhaupt ein Kamerastatus gespeichert ist
            if (!_currentProfileCached.HasCameraSaved)
            {
                return true;
            }

            // Sicherer Zugriff auf Screen3D
            if (_screen3D?.ScreenD3D?.Renderer == null)
            {
                return false; // Können nicht speichern wenn Screen nicht verfügbar
            }

            CameraState cameraState = _screen3D.ScreenD3D.Renderer.ExportCameraState();
		if (_currentProfileCached.CameraState2 is CameraState cameraState2)
		{
			if (cameraState != cameraState2)
			{
				return true;
			}
                return _currentProfileCached.ProjectionType != (int)_screen3D.ScreenD3D.ProjectionType;
            }
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false; // Screen wurde bereits disposed
        }    
	}

	public bool CanEraseSavedCameraState()
	{
		return _currentProfileCached.HasCameraSaved;
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
		IDoc3d currentDoc = _docProvider.CurrentDoc;
		if (_partRoleOpacities.TryGetValue(PartRole.BendModel, out var value))
		{
			Model bendModel3D = currentDoc.BendModel3D;
			paintTool.SetModelOpacity(bendModel3D, value, applyToSubModels: true);
		}
		IBendMachineGeometry machine = currentDoc.BendSimulation?.State.MachineConfig?.Geometry;
		_machinePainter.SetOpacities(paintTool, machine, _partRoleOpacities);
	}

	private void CheckIfValid(GeneralUserSettingsConfig config)
	{
		List<OpacityProfile> opacityProfiles = config.OpacityProfiles;
		if (opacityProfiles == null || opacityProfiles.Count == 0 || opacityProfiles.Any((OpacityProfile x) => x == null) || opacityProfiles.Any((OpacityProfile x) => x.Opacities == null) || opacityProfiles.Any((OpacityProfile x) => x.Opacities.Length != System.Enum.GetValues(typeof(PartRole)).Length))
		{
			InitOpacityProfiles();
		}
	}
}

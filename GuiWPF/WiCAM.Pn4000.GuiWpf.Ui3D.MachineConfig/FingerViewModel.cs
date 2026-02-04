using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine.FingerStop;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.SubViews;
using WiCAM.Pn4000.WpfControls.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;
using ViewModelBase = WiCAM.Pn4000.Common.ViewModelBase;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class FingerViewModel : ViewModelBase
{
	private Screen3D _screen3D;

	private Model _fingerModel;

	private readonly IConfigProvider _configProvider;

	private readonly IMessageLogGlobal _messageLog;

	private readonly IPaintTool _paintTool;

	private IStopCombinations _stopCombinations;

	private IBendMachine _bendMachine;

	private readonly Color _highLightColor = Color.FromRGBA(6553855u);

	private readonly Color _selectColorFlat = Color.FromRGBA(4294902015u);

	private readonly Color _highLightColor2 = Color.FromRGBA(1804477439u);

	private readonly Color _highLightColor3 = Color.FromRGBA(1804477439u);

	private readonly Color _highLightColor4 = Color.FromRGBA(4292280575u);

	private readonly Color _errorColor = Color.FromRGBA(4278190335u);

	private readonly Color _errorColorHover = Color.FromRGBA(3692313855u);

	public Action<ChangedConfigType> DataChanged;

	private StopCombinationViewModel? _selectedCombination;

	private StopCombinationViewModel? _hoveredCombination;

	private Face? _hoveredFace;

	private Face? _selectedFace;

	private double _alignY;

	public ObservableCollection<StopCombinationViewModel> Combinations { get; } = new ObservableCollection<StopCombinationViewModel>();

	private Dictionary<Face, string> FaceToName { get; } = new Dictionary<Face, string>();

	private Canvas ScreenOverlay => Screen3D.Overlay;

	public string Image
	{
		get
		{
			if (SelectedCombination == null)
			{
				return null;
			}
			string text = Path.Combine(_bendMachine.MachinePath, "FingerStops_Icons", SelectedCombination.ActualIconPath);
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}
	}

	public Screen3D Screen3D
	{
		get
		{
			return _screen3D;
		}
		set
		{
			_screen3D = value;
			NotifyPropertyChanged("Screen3D");
		}
	}

	public StopCombinationViewModel? SelectedCombination
	{
		get
		{
			return _selectedCombination;
		}
		set
		{
			_selectedCombination = value;
			RepaintModel();
			NotifyPropertyChanged("SelectedCombination");
			NotifyPropertyChanged("Image");
		}
	}

	public StopCombinationViewModel? HoveredCombination
	{
		get
		{
			return _hoveredCombination;
		}
		set
		{
			_hoveredCombination = value;
			RepaintModel();
			NotifyPropertyChanged("HoveredCombination");
		}
	}

	private ScreenD3D11 ScreenD3D => Screen3D.ScreenD3D;

	public Face? HoveredFace
	{
		get
		{
			return _hoveredFace;
		}
		set
		{
			_hoveredFace = value;
			NotifyPropertyChanged("HoveredFace");
			NotifyPropertyChanged("HoveredFaceName");
		}
	}

	public string HoveredFaceName
	{
		get
		{
			if (HoveredFace == null || !FaceToName.TryGetValue(HoveredFace, out string value))
			{
				return string.Empty;
			}
			return value;
		}
	}

	public Face? SelectedFace
	{
		get
		{
			return _selectedFace;
		}
		set
		{
			_selectedFace = value;
			NotifyPropertyChanged("SelectedFace");
		}
	}

	public double AlignY
	{
		get
		{
			return _alignY;
		}
		set
		{
			_alignY = value;
			NotifyPropertyChanged("AlignY");
		}
	}

	public FingerViewModel(IPaintTool paintTool, IConfigProvider configProvider, IMessageLogGlobal messageLog)
	{
		_paintTool = paintTool;
		_configProvider = configProvider;
		_messageLog = messageLog;
	}

	public FingerViewModel Init(IBendMachine bendMachine, FingerStopType fingerStop)
	{
		_bendMachine = bendMachine;
		IFingerStop fingerStop2 = fingerStop switch
		{
			FingerStopType.LeftFinger => _bendMachine.Geometry.LeftFinger, 
			FingerStopType.RightFinger => _bendMachine.Geometry.RightFinger, 
			_ => throw new ArgumentOutOfRangeException("fingerStop", fingerStop, null), 
		};
		_stopCombinations = fingerStop switch
		{
			FingerStopType.LeftFinger => _bendMachine.StopCombinationsLeftFinger, 
			FingerStopType.RightFinger => _bendMachine.StopCombinationsRightFinger, 
			_ => throw new ArgumentOutOfRangeException("fingerStop", fingerStop, null), 
		};
		HashSet<string> hashSet = _stopCombinations.FaceNames.ToHashSet();
		List<IFingerStopCombinationData> list = (from x in _stopCombinations.Combinations
			where x.Type.Any(StopCombinationType.FaceMask)
			orderby x.Type.Count(StopCombinationType.FaceMask), x.Type.ToFaceNames().FirstOrDefault() ?? string.Empty
			select x).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			IFingerStopCombinationData data = list[i];
			StopCombinationViewModel item = new StopCombinationViewModel(i, data);
			Combinations.Add(item);
		}
		_fingerModel = new Model
		{
			Shell = fingerStop2.FingerModel.Shell,
			FileName = fingerStop2.FilePath
		};
		FaceToName.Clear();
		foreach (Face allFace in _fingerModel.GetAllFaces())
		{
			if (hashSet.Contains(allFace.Name))
			{
				FaceToName[allFace] = allFace.Name;
			}
		}
		Screen3D = new Screen3D();
		Screen3D.Loaded += Screen3DAssembly3DOnLoaded;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		Screen3D.SetBackground(generalUserSettingsConfig.PreviewColor3D1.ToWpfColor(), generalUserSettingsConfig.PreviewColor3D2.ToWpfColor());
		Screen3D.MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
		return this;
	}

	private void Screen3DAssembly3DOnLoaded(object sender, RoutedEventArgs e)
	{
		ScreenD3D.RemoveModel(null);
		ScreenD3D.RemoveBillboard(null);
		ScreenD3D.Resize((int)Screen3D.ActualWidth, (int)Screen3D.ActualHeight);
		ScreenD3D.AddModel(_fingerModel, render: false);
		Sphere model = new Sphere(new Vector3d(0.0, 0.0, 0.0), 3.0, 20, 20, new Color(1f, 1f, 0f, 1f));
		ScreenD3D.AddModel(model);
		SetViewDirection();
		ScreenD3D.Render(skipQueuedFrames: false);
		Screen3D.MouseEnterTriangle += MouseEnterTriangle;
		Screen3D.TriangleSelected += TriangleSelected;
	}

	private void MouseEnterTriangle(IScreen3D sender, ITriangleEventArgs e)
	{
		Face face = e.Tri?.Face;
		if ((face != null && face.FaceType == FaceType.Flat) || (face != null && face.FaceType == FaceType.RoundCylindricalConvex))
		{
			HoveredFace = face;
		}
		else
		{
			HoveredFace = null;
		}
		RepaintModel();
	}

	private void TriangleSelected(IScreen3D sender, ITriangleEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		e.Handle();
		SelectedCombination = null;
		SelectedFace = e.Tri?.Face;
		RepaintModel();
		if (SelectedFace == null)
		{
			return;
		}
		AlignY = SelectedFace.SurfaceDerivatives.Keys.Average((Vertex x) => x.Pos.Y);
		FaceViewModel viewModel = new FaceViewModel(SelectedFace, _stopCombinations);
		ShowPopup(e.Args, viewModel, delegate(FaceNameViewModel x)
		{
			FaceToName[SelectedFace] = x.Name;
			if (IsNameNotValidForFace(SelectedFace, x.Name))
			{
				_messageLog.ShowErrorMessage("Selected face not parallel to lower beam!");
			}
		}, delegate
		{
		});
	}

	private void HidePopups()
	{
		Application.Current.Dispatcher.Invoke(ScreenOverlay.Children.Clear);
	}

	private void RepaintModel()
	{
		_paintTool.FrameStart();
		List<Face> list = new List<Face>();
		List<Face> list2 = new List<Face>();
		if (HoveredFace != null)
		{
			list2.Add(HoveredFace);
		}
		Dictionary<string, Face> dictionary = new Dictionary<string, Face>();
		List<Face> list3 = new List<Face>();
		foreach (var (face2, text2) in FaceToName)
		{
			if (IsNameNotValidForFace(face2, text2))
			{
				list3.Add(face2);
			}
			if (!string.IsNullOrEmpty(text2) && !dictionary.TryAdd(text2, face2))
			{
				list3.Add(face2);
				list3.Add(dictionary[text2]);
			}
		}
		if (SelectedCombination != null)
		{
			foreach (string faceName in SelectedCombination.FaceNames)
			{
				if (dictionary.TryGetValue(faceName, out var value))
				{
					list.Add(value);
				}
			}
		}
		else if (SelectedFace != null)
		{
			list.Add(SelectedFace);
		}
		if (HoveredCombination != null)
		{
			foreach (string faceName2 in HoveredCombination.FaceNames)
			{
				if (dictionary.TryGetValue(faceName2, out var value2))
				{
					list2.Add(value2);
				}
			}
		}
		foreach (Face item in list)
		{
			_paintTool.SetFaceColor(item, _fingerModel, _selectColorFlat);
		}
		foreach (Face item2 in list2)
		{
			Color value3 = (list.Contains(item2) ? _highLightColor4 : _highLightColor);
			_paintTool.SetFaceColor(item2, _fingerModel, value3);
		}
		foreach (Face item3 in list3)
		{
			Color value4 = (list2.Contains(item3) ? _errorColorHover : _errorColor);
			_paintTool.SetFaceColor(item3, _fingerModel, value4);
		}
		_paintTool.FrameApply(out HashSet<Model> modifiedModels, out HashSet<Shell> modifiedShells);
		ScreenD3D.UpdateModelAppearance(ref modifiedModels, ref modifiedShells);
	}

	private void SetViewDirection()
	{
		Matrix4d identity = Matrix4d.Identity;
		identity *= Matrix4d.RotationX(-0.6283185482025146);
		identity *= Matrix4d.RotationZ(3.1415927410125732);
		ScreenD3D.SetViewDirectionByMatrix4d(identity, render: false, delegate
		{
			Pair<Vector3d, Vector3d> boundary = _fingerModel.GetBoundary(_fingerModel.Transform.Inverted);
			ScreenD3D.ZoomExtend(render: true, boundary.Item1, boundary.Item2, 0.5, fitFront: false, null);
		});
	}

	private static bool IsNameNotValidForFace(Face face, string? name)
	{
		if (name == null || !name.Contains("flat"))
		{
			return false;
		}
		Plane flatFacePlane = face.FlatFacePlane;
		if (flatFacePlane != null && !(Math.Abs(flatFacePlane.Normal.X) > 1E-06) && !(Math.Abs(flatFacePlane.Normal.Y + 1.0) > 1E-06))
		{
			return Math.Abs(flatFacePlane.Normal.Z) > 1E-06;
		}
		return true;
	}

	public void AlignFaceY()
	{
		Face selectedFace = SelectedFace;
		if (selectedFace == null || selectedFace.FaceType != FaceType.Flat)
		{
			return;
		}
		Dictionary<Vector3d, Vertex> vertexCache = selectedFace.Shell.VertexCache;
		Vector3d pos;
		foreach (Vertex key in selectedFace.SurfaceDerivatives.Keys)
		{
			vertexCache.Remove(key.Pos);
			pos = key.Pos;
			pos.Y = AlignY;
			key.Pos = pos;
			while (!vertexCache.TryAdd(key.Pos, key))
			{
				pos = key.Pos;
				pos.Y = double.BitIncrement(key.Pos.Y);
				key.Pos = pos;
			}
		}
		foreach (KeyValuePair<Vertex, SurfaceDerivatives> surfaceDerivative in selectedFace.SurfaceDerivatives)
		{
			surfaceDerivative.Value.Normal = -Vector3d.UnitY;
		}
		Plane flatFacePlane = selectedFace.FlatFacePlane;
		pos = selectedFace.FlatFacePlane.Origin;
		pos.Y = AlignY;
		flatFacePlane.Origin = pos;
		selectedFace.FlatFacePlane.Normal = -Vector3d.UnitY;
		Screen3D.ScreenD3D.UpdateAllModelGeometry(render: false);
		RepaintModel();
	}

	public void Save()
	{
		foreach (KeyValuePair<Face, string> item in FaceToName)
		{
			item.Deconstruct(out var key, out var value);
			Face face = key;
			string name = value;
			face.Name = name;
		}
		ModelSerializer.Serialize(Path.Combine(_bendMachine.Geometry.BasePath, _fingerModel.FileName), _fingerModel);
		foreach (StopCombinationViewModel combination in Combinations)
		{
			combination.Save();
		}
	}

	private void ShowPopup<T>(IPnInputEventArgs args, ICustomAutoCompleteBoxViewModel viewModel, Action<T> addPiece, Action<T> visualize) where T : ICustomAutoCompleteBoxViewModel.IItem
	{
		HidePopups();
		if (args.MouseEventArgs == null)
		{
			return;
		}
		Point position = args.MouseEventArgs.GetPosition(ScreenOverlay);
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			CustomAutoCompleteBox<T> customAutoCompleteBox = new CustomAutoCompleteBox<T>(viewModel);
			RadAutoCompleteBox dropdown = customAutoCompleteBox.DropDown;
			dropdown.SelectionChanged += delegate
			{
				HidePopups();
				if (dropdown.SelectedItem is T obj)
				{
					addPiece(obj);
				}
			};
			customAutoCompleteBox.SelectedItemChanged += visualize;
			customAutoCompleteBox.Loaded += delegate
			{
				dropdown.Focus();
				dropdown.Populate(string.Empty);
				dropdown.LostMouseCapture += delegate
				{
					HidePopups();
					RepaintModel();
				};
			};
			ScreenOverlay.Children.Add(customAutoCompleteBox);
			Canvas.SetLeft(customAutoCompleteBox, position.X - dropdown.Width / 2.0);
			Canvas.SetTop(customAutoCompleteBox, position.Y);
		});
	}

	public void Dispose()
	{
		Screen3D?.Dispose();
		Screen3D = null;
	}
}

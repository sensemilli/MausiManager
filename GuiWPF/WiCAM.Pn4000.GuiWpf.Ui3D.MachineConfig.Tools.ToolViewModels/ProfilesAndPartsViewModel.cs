using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PnControl;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public abstract class ProfilesAndPartsViewModel<T, P> : ToolViewModelBase, IDeleteTab, IAddTab, IEditTab, IAnyTab where T : ToolViewModel where P : ToolPieceViewModel
{
    private IConfigProvider _configProvider;

    private IDrawToolProfiles _drawToolProfiles;

    private IGlobalToolStorage _toolStorage;

    private ICommand? _selectClick;

    private ICommand? _keyDownDelete;

    private FrameworkElement _editScreen;

    private Visibility _editScreenVisible;

    private bool _isCopyButtonEnabled;

    private bool _isDeleteButtonEnabled;

    private bool _isEditButtonEnabled;

    private bool _isAddButtonEnabled;

    private bool _isOkButtonEnabled;

    private bool _isCancelButtonEnabled;

    private bool _isSaveButtonEnabled;

    private bool _showProfilesWithoutParts;

    private bool _showAllParts;

    private ObservableCollection<T> _selectedProfiles;

    private ObservableCollection<P> _selectedParts;

    private RelayCommand<object>? _cmdMirrorGeometry;

    private RelayCommand<object>? _cmdEditGeometry;

    private RelayCommand<object>? _cmdSaveGeometry;

    private RelayCommand<object>? _cmdChooseGeometryFile;

    private RelayCommand<object>? _cmdRemoveGeometryFile;

    private Type _lastSelectedType;

    private Brush _profileBorderBrush;

    private Brush _pieceBorderBrush;

    private bool _dirtyFlagProfileView;

    private bool _dirtyFlagPartView;

    public T? SelectedProfile
    {
        get
        {
            return SelectedProfiles.FirstOrDefault();
        }
        set
        {
            SelectedProfiles.Clear();
            SelectedProfiles.Add(value);
            NotifyPropertyChanged("SelectedProfile");
        }
    }

    public P? SelectedPart
    {
        get
        {
            return SelectedParts.FirstOrDefault();
        }
        set
        {
            SelectedParts.Clear();
            SelectedParts.Add(value);
            NotifyPropertyChanged("SelectedPart");
        }
    }

    public ObservableCollection<T> SelectedProfiles
    {
        get
        {
            return _selectedProfiles;
        }
        internal set
        {
            _selectedProfiles = value;
            _selectedProfiles.CollectionChanged += delegate
            {
                ProfileSelectionChanged();
            };
            NotifyPropertyChanged("SelectedProfiles");
        }
    }

    public ObservableCollection<P> SelectedParts
    {
        get
        {
            return _selectedParts;
        }
        internal set
        {
            _selectedParts = value;
            _selectedParts.CollectionChanged += delegate
            {
                PartsSelectionChanged();
            };
            NotifyPropertyChanged("SelectedParts");
        }
    }

    public Type LastSelectedType
    {
        get
        {
            return _lastSelectedType;
        }
        set
        {
            if (_lastSelectedType != value)
            {
                _lastSelectedType = value;
                NotifyPropertyChanged("LastSelectedType");
                SetEditorEnableRules();
                if (_lastSelectedType == typeof(T))
                {
                    PieceBorderBrush = new SolidColorBrush(Colors.Transparent);
                    ProfileBorderBrush = new SolidColorBrush(Colors.DarkBlue);
                }
                else if (_lastSelectedType == typeof(P))
                {
                    PieceBorderBrush = new SolidColorBrush(Colors.DarkBlue);
                    ProfileBorderBrush = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }

    public Brush ProfileBorderBrush
    {
        get
        {
            return _profileBorderBrush;
        }
        set
        {
            _profileBorderBrush = value;
            NotifyPropertyChanged("ProfileBorderBrush");
        }
    }

    public Brush PieceBorderBrush
    {
        get
        {
            return _pieceBorderBrush;
        }
        set
        {
            _pieceBorderBrush = value;
            NotifyPropertyChanged("PieceBorderBrush");
        }
    }

    public FrameworkElement EditScreen
    {
        get
        {
            return _editScreen;
        }
        set
        {
            _editScreen = value;
            NotifyPropertyChanged("EditScreen");
        }
    }

    public Visibility EditScreenVisible
    {
        get
        {
            return _editScreenVisible;
        }
        set
        {
            _editScreenVisible = value;
            NotifyPropertyChanged("EditScreenVisible");
        }
    }

    public bool ShowProfilesWithoutParts
    {
        get
        {
            return _showProfilesWithoutParts;
        }
        set
        {
            _showProfilesWithoutParts = value;
            NotifyPropertyChanged("ShowProfilesWithoutParts");
            LoadTools();
        }
    }

    public bool ShowAllParts
    {
        get
        {
            return _showAllParts;
        }
        set
        {
            _showAllParts = value;
            NotifyPropertyChanged("ShowAllParts");
            LoadTools();
        }
    }

    public bool IsCopyButtonEnabled
    {
        get
        {
            return _isCopyButtonEnabled;
        }
        set
        {
            _isCopyButtonEnabled = value;
            NotifyPropertyChanged("IsCopyButtonEnabled");
        }
    }

    public bool IsDeleteButtonEnabled
    {
        get
        {
            return _isDeleteButtonEnabled;
        }
        set
        {
            _isDeleteButtonEnabled = value;
            NotifyPropertyChanged("IsDeleteButtonEnabled");
        }
    }

    public bool IsEditButtonEnabled
    {
        get
        {
            return _isEditButtonEnabled;
        }
        set
        {
            _isEditButtonEnabled = value;
            NotifyPropertyChanged("IsEditButtonEnabled");
        }
    }

    public bool IsAddButtonEnabled
    {
        get
        {
            return _isAddButtonEnabled;
        }
        set
        {
            _isAddButtonEnabled = value;
            NotifyPropertyChanged("IsAddButtonEnabled");
        }
    }

    public bool IsOkButtonEnabled
    {
        get
        {
            return _isOkButtonEnabled;
        }
        set
        {
            _isOkButtonEnabled = value;
            NotifyPropertyChanged("IsOkButtonEnabled");
        }
    }

    public bool IsCancelButtonEnabled
    {
        get
        {
            return _isCancelButtonEnabled;
        }
        set
        {
            _isCancelButtonEnabled = value;
            NotifyPropertyChanged("IsCancelButtonEnabled");
        }
    }

    public bool IsSaveButtonEnabled
    {
        get
        {
            return _isSaveButtonEnabled;
        }
        set
        {
            _isSaveButtonEnabled = value;
            NotifyPropertyChanged("IsSaveButtonEnabled");
        }
    }

    public abstract ObservableCollection<T> Profiles { get; }

    public abstract ObservableCollection<P> Pieces { get; }

    public ICommand SelectClick => _selectClick ?? (_selectClick = new RelayCommand((Action)delegate
    {
    }));

    public ICommand KeyDownDelete => _keyDownDelete ?? (_keyDownDelete = new RelayCommand(DeleteButtonClick));

    public RelayCommand<object> CmdMirrorGeometry => _cmdMirrorGeometry ?? (_cmdMirrorGeometry = new RelayCommand<object>(MirrorGeometry));

    public RelayCommand<object> CmdEditGeometry => _cmdEditGeometry ?? (_cmdEditGeometry = new RelayCommand<object>(EditGeometry));

    public RelayCommand<object> CmdSaveGeometry => _cmdSaveGeometry ?? (_cmdSaveGeometry = new RelayCommand<object>(SaveGeometry));

    public RelayCommand<object> CmdChooseGeometryFile => _cmdChooseGeometryFile ?? (_cmdChooseGeometryFile = new RelayCommand<object>(ChooseGeometryFile));

    public RelayCommand<object> CmdRemoveGeometryFile => _cmdRemoveGeometryFile ?? (_cmdRemoveGeometryFile = new RelayCommand<object>(RemoveGeometryFile));

    public RadObservableCollection<T> ProfilesFiltered { get; }

    public RadObservableCollection<P> PartFiltered { get; }

    private IModelFactory _modelFactory;

    public ObservableCollection<RadMenuItem> ProfileContextMenuItems { get; set; }

    public ObservableCollection<RadMenuItem> PartContextMenuItems { get; set; }

    public List<ComboboxEntry<AllowedFlippedStates>> AllAllowedFlippingStates => base.ToolConfigModel.AllAllowedFlippingStates;

    protected ProfilesAndPartsViewModel(IConfigProvider _configProvider, Contracts.Common.ITranslator _translator,
        IDrawToolProfiles _drawToolProfiles, IGlobalToolStorage _toolStorage, IModelFactory _modelFactory) : base(_modelFactory, _translator)
    {
        this._configProvider = _configProvider;
        this._drawToolProfiles = _drawToolProfiles;
        this._toolStorage = _toolStorage;
        _selectedProfiles = new ObservableCollection<T>();
        _selectedParts = new ObservableCollection<P>();
        ProfilesFiltered = new RadObservableCollection<T>();
        PartFiltered = new RadObservableCollection<P>();
        this._modelFactory = _modelFactory;
        // base._002Ector(_modelFactory, _translator);
        //this.base(_modelFactory, _translator);
    }

    public new ProfilesAndPartsViewModel<T, P> Init(MachineToolsViewModel toolModel)
    {
        base.Init(toolModel);
        EventManager.RegisterClassHandler(typeof(RadGridView), Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FocusChanged));
        EditScreenVisible = Visibility.Collapsed;
        base.ToolConfigModel = toolModel;
        base.ImagePart.Loaded += delegate
        {
            RefreshProfileView();
        };
        base.ImagePart.Loaded += delegate
        {
            RefreshPartView();
        };
        GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
        InitializeProfileContextMenuItems();
        InitializePartContextMenuItems();
        ((Screen3D)base.ImagePart).MouseWheelInverted = generalUserSettingsConfig.P3D_InvertMouseWheel;
        LoadTools();
        Profiles.CollectionChanged += delegate
        {
            if (!_dirtyFlagProfileView)
            {
                _dirtyFlagProfileView = true;
                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    _dirtyFlagProfileView = false;
                    RefreshProfileView();
                });
            }
        };
        Pieces.CollectionChanged += delegate
        {
            if (!_dirtyFlagProfileView)
            {
                _dirtyFlagProfileView = true;
                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    _dirtyFlagProfileView = false;
                    RefreshProfileView();
                });
            }
            if (!_dirtyFlagPartView)
            {
                _dirtyFlagPartView = true;
                Application.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    _dirtyFlagPartView = false;
                    RefreshPartView();
                });
            }
        };
        return this;
    }

    public abstract void AddProfile();

    public abstract void AddPiece();

    public abstract void DeleteProfile(T profile);

    public abstract void DeletePiece(P piece);

    private void FocusChanged(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is RadGridView radGridView)
        {
            if (radGridView.ItemsSource == ProfilesFiltered)
            {
                base.LastSelectedObject = SelectedProfile;
                LastSelectedType = typeof(T);
            }
            else if (radGridView.ItemsSource == PartFiltered)
            {
                base.LastSelectedObject = SelectedPart;
                LastSelectedType = typeof(P);
            }
        }
    }

    public void LoadTools()
    {
        RefreshProfileView();
        RefreshPartView();
    }

    public void AddProfile(T profile)
    {
        Profiles.Add(profile);
        SelectedProfile = profile;
    }

    public void AddPiece(P piece)
    {
        Pieces.Add(piece);
        SelectedPart = piece;
    }

    public void ReplaceProfileValues(T before, T after)
    {
        Profiles.Remove(before);
        Profiles.Add(after);
    }

    public void ReplacePartValues(P before, P after)
    {
        Pieces.Remove(before);
        after.TotalAmount = 0;
        foreach (ToolListViewModel toolList in base.ToolConfigModel.ToolLists)
        {
            toolList.ReplacePiece(before, after);
            after.TotalAmount += toolList.GetAmount(after);
        }
        Pieces.Add(after);
    }

    public void DeleteProfiles(IEnumerable<T> profiles)
    {
        int value = ProfilesFiltered.IndexOf(profiles.Last()) + 1 - profiles.Count();
        foreach (T item in profiles.ToList())
        {
            DeleteProfile(item);
        }
        if (ProfilesFiltered.Count > 0)
        {
            value = Math.Clamp(value, 0, ProfilesFiltered.Count - 1);
            SelectedProfile = ProfilesFiltered[value];
        }
        else
        {
            SelectedProfile = null;
        }
    }

    public void DeletePieces(IEnumerable<P> pieces)
    {
        int value = PartFiltered.IndexOf(pieces.Last()) + 1 - pieces.Count();
        foreach (P item in pieces.ToList())
        {
            DeletePiece(item);
        }
        if (PartFiltered.Count > 0)
        {
            value = Math.Clamp(value, 0, PartFiltered.Count - 1);
            SelectedPart = PartFiltered[value];
        }
        else
        {
            SelectedPart = null;
        }
    }

    public void CopyButtonClick()
    {
        if (LastSelectedType == typeof(T))
        {
            CopyProfile(SelectedProfile);
        }
        else if (LastSelectedType == typeof(P))
        {
            CopyPart(SelectedPart);
        }
    }

    public void AddButtonClick()
    {
        if (LastSelectedType == typeof(T))
        {
            AddProfile();
        }
        else if (LastSelectedType == typeof(P))
        {
            AddPiece();
        }
    }

    public void CopyPart(P part)
    {
        P val = (P)part.Copy();
        val.AfterCopy();
        AddPiece(val);
        SelectedPart = val;
    }

    public void CopyProfile(T profile)
    {
        T val = (T)profile.Copy();
        val.AfterCopy(base.ToolConfigModel.CreateMultiTool(""));
        AddProfile(val);
        SelectedProfile = val;
    }

    protected void InitializeProfileContextMenuItems()
    {
        ProfileContextMenuItems = new ObservableCollection<RadMenuItem>();
        RadMenuItem item = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button8_Copy"),
            Icon = new RadGlyph
            {
                Glyph = "\ue65d"
            },
            Command = new RelayCommand((Action<object>)delegate
            {
                CopyProfile(SelectedProfile);
            })
        };
        ProfileContextMenuItems.Add(item);
        RadMenuItem item2 = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button1_Edit"),
            Icon = new RadGlyph
            {
                Glyph = "\ue10b"
            },
            Command = new RelayCommand((Action<object>)delegate
            {
                EditProfile_Click(SelectedProfile);
            })
        };
        ProfileContextMenuItems.Add(item2);
        RadMenuItem item3 = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button0_Delete"),
            Icon = new RadGlyph
            {
                Glyph = "\ue10c"
            },
            Command = new RelayCommand((Action<object>)delegate
            {
                DeleteProfiles(SelectedProfiles);
            })
        };
        ProfileContextMenuItems.Add(item3);
    }

    protected void InitializePartContextMenuItems()
    {
        PartContextMenuItems = new ObservableCollection<RadMenuItem>();
        RadMenuItem item = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button8_Copy"),
            Icon = new RadGlyph
            {
                Glyph = "\ue65d"
            },
            Command = new RelayCommand((Action<object>)delegate
            {
                CopyPart(SelectedPart);
            })
        };
        PartContextMenuItems.Add(item);
        RadMenuItem item2 = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button1_Edit"),
            Icon = new RadGlyph
            {
                Glyph = "\ue10b"
            },
            Command = new RelayCommand((Action<object>)delegate
            {
                EditPart_Click(SelectedPart);
            })
        };
        PartContextMenuItems.Add(item2);
        RadMenuItem item3 = new RadMenuItem
        {
            Header = Application.Current.FindResource("l_popup.Button0_Delete"),
            Icon = new RadGlyph
            {
                Glyph = "\ue10c"
            },
            Command = new RelayCommand((Action)delegate
            {
                DeletePieces(SelectedParts);
            })
        };
        PartContextMenuItems.Add(item3);
    }

    protected void RefreshProfileView()
    {
        T selectedProfile = SelectedProfile;
        P selectedPart = SelectedPart;
        ProfilesFiltered.Clear();
        ProfilesFiltered.AddRange(Profiles.Where(ProfileFilter));
        if (ProfilesFiltered.Count > 0)
        {
            if (selectedProfile != null && ProfilesFiltered.Contains(selectedProfile))
            {
                SelectedProfile = selectedProfile;
                SelectedPart = (P?)((selectedPart != null && PartFiltered.Contains(selectedPart)) ? ((ToolPieceViewModel)selectedPart) : ((ToolPieceViewModel)SelectedPart));
            }
            else
            {
                SelectedProfile = ProfilesFiltered[0];
            }
        }
    }

    protected void RefreshPartView()
    {
        P selectedPart = SelectedPart;
        PartFiltered.Clear();
        PartFiltered.AddRange(Pieces.Where(PartFilter));
        if (PartFiltered.Count > 0)
        {
            SelectedPart = ((selectedPart != null && PartFiltered.Contains(selectedPart)) ? selectedPart : PartFiltered[0]);
        }
    }

    protected void RefreshProfileImage()
    {
        if (SelectedProfile == null)
        {
            ((Canvas)base.ImageProfile)?.Children.Clear();
        }
        else
        {
            SelectedProfile.Load2DPreview((Canvas)base.ImageProfile, _drawToolProfiles);
        }
        NotifyPropertyChanged("ImageProfile");
    }

    protected void RefreshPartImage()
    {
        if (SelectedPart == null || SelectedProfile == null)
        {
            ((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveModel(null);
            ((Screen3D)base.ImagePart)?.ScreenD3D?.RemoveBillboard(null);
        }
        else
        {
            SelectedPart.Load3DPreview(base.ImagePart);
        }
        NotifyPropertyChanged("ImagePart");
    }

    private void ProfileSelectionChanged()
    {
        RefreshProfileImage();
        RefreshPartView();
        SetEditorEnableRules();
        base.LastSelectedObject = SelectedProfile;
    }

    private void PartsSelectionChanged()
    {
        RefreshPartImage();
        SetEditorEnableRules();
        base.LastSelectedObject = SelectedPart;
    }

    protected bool ProfileFilter(T item)
    {
        if (item == null)
        {
            return false;
        }
        if (ShowProfilesWithoutParts)
        {
            return true;
        }
        foreach (ToolPieceViewModel toolPiece in item.MultiTool.ToolPieces)
        {
            if (toolPiece.Amount > 0 || ShowAllParts)
            {
                return true;
            }
        }
        return false;
    }

    protected bool PartFilter(P piece)
    {
        if (piece != null && SelectedProfile != null)
        {
            if (piece.MultiTool == SelectedProfile.MultiTool)
            {
                if (piece.Amount <= 0)
                {
                    return ShowAllParts;
                }
                return true;
            }
            return false;
        }
        return false;
    }

    public void DeleteButtonClick()
    {
        if (LastSelectedType == typeof(T))
        {
            DeleteProfiles(SelectedProfiles);
        }
        else if (LastSelectedType == typeof(P))
        {
            DeletePieces(SelectedParts);
        }
    }

    public void MirrorGeometry(object param)
    {
        MultiToolViewModel multiTool = null;
        if (param is T val)
        {
            multiTool = val.MultiTool;
        }
        else if (param is P val2)
        {
            multiTool = val2.MultiTool;
        }
        if (multiTool != null)
        {
            IEnumerable<string> pieceFiles = from x in Pieces
                                             where x.MultiTool == multiTool
                                             select x.GeometryFileFull;
            MirrorGeometry(multiTool.GeometryFileFull, pieceFiles);
            ProfileSelectionChanged();
        }
    }

    public void EditGeometry(object param)
    {
        T profileVm = param as T;
        if (profileVm != null && profileVm.MultiTool != null)
        {
            IEnumerable<string> pieceFiles = from x in Pieces
                                             where x.MultiTool == profileVm.MultiTool
                                             select x.GeometryFileFull;
            EditGeometry(profileVm.MultiTool.GeometryFileFull, pieceFiles);
            ProfileSelectionChanged();
        }
    }

    public void SaveGeometry(object param)
    {
        if (param is T val && val.MultiTool != null)
        {
            MultiToolViewModel multiTool = val.MultiTool;
            IEnumerable<string> pieceFiles = from x in Pieces
                                             where x.MultiTool == multiTool
                                             select x.GeometryFileFull;
            SaveGeometry(multiTool.GeometryFileFull, pieceFiles);
            if (multiTool.SetGeometryFile(multiTool.GeometryFileFull) && SelectedProfile?.MultiTool == val.MultiTool)
            {
                val.ValidateGeometry();
                RefreshProfileImage();
                RefreshPartImage();
            }
            ProfileSelectionChanged();
        }
    }

    public void ChooseGeometryFile(object param)
    {
        if (param is T val)
        {
            MultiToolViewModel multiTool = val.MultiTool;
            if (multiTool != null)
            {
                string geometryFileFull = multiTool.GeometryFileFull;
                FileInfo fileInfo = (string.IsNullOrEmpty(geometryFileFull) ? null : new FileInfo(geometryFileFull));
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    FileName = (fileInfo?.Name ?? ""),
                    Multiselect = false,
                    DefaultExt = "",
                    CheckFileExists = true,
                    InitialDirectory = (fileInfo?.DirectoryName ?? _toolStorage.GetGeometryBaseDirectory())
                };
                if (openFileDialog.ShowDialog().GetValueOrDefault() && multiTool.SetGeometryFile(openFileDialog.FileName) && SelectedProfile?.MultiTool == val.MultiTool)
                {
                    RefreshProfileImage();
                    RefreshPartImage();
                }
            }
        }
        else if (param is P val2)
        {
            string geometryFileFull2 = val2.GeometryFileFull;
            FileInfo fileInfo2 = (string.IsNullOrEmpty(geometryFileFull2) ? null : new FileInfo(geometryFileFull2));
            OpenFileDialog openFileDialog2 = new OpenFileDialog
            {
                FileName = (fileInfo2?.Name ?? ""),
                Multiselect = false,
                DefaultExt = "c3mo",
                AddExtension = true,
                Filter = "C3MO|*.c3mo",
                CheckFileExists = true,
                InitialDirectory = (fileInfo2?.DirectoryName ?? _toolStorage.GetGeometryBaseDirectory())
            };
            if (openFileDialog2.ShowDialog().GetValueOrDefault() && val2.SetGeometryFile(openFileDialog2.FileName) && val2 == SelectedPart)
            {
                RefreshPartImage();
            }
        }
    }

    public void RemoveGeometryFile(object param)
    {
        if (param is P val && val.SetGeometryFile(null) && val == SelectedPart)
        {
            RefreshPartImage();
        }
    }

    public void EditButtonClick()
    {
        if (LastSelectedType == typeof(T))
        {
            EditProfile_Click(SelectedProfile);
        }
        else if (LastSelectedType == typeof(P))
        {
            EditPart_Click(SelectedPart);
        }
    }

    public void EditProfile_Click(T profile)
    {
        T profile2 = profile;
        Action<bool, bool> closeAction = delegate (bool isOk, bool close)
        {
            if (isOk)
            {
                T val = ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as T;
                ReplaceProfileValues(profile2, val);
                profile2 = val;
            }
            if (close)
            {
                ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
                EditScreen = null;
                SetEditorEnableRules();
            }
        };
        EditScreenViewModel dataContext = new EditScreenViewModel(ProfilesFiltered, profile2, isUpper: false, base.ToolConfigModel, _configProvider, closeAction, _drawToolProfiles);
        EditScreenView editScreen = new EditScreenView
        {
            DataContext = dataContext
        };
        if (EditScreen != null)
        {
            ((EditScreen as EditScreenView)?.DataContext as EditScreenViewModel)?.Dispose();
            EditScreen = null;
        }
        EditScreen = editScreen;
        EditScreenVisible = Visibility.Visible;
        SetEditorEnableRules();
    }

    public void EditPart_Click(P toolPiece)
    {
        P toolPiece2 = toolPiece;
        Action<bool, bool> closeAction = delegate (bool isOk, bool close)
        {
            if (isOk)
            {
                P after = ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).SelectedItem as P;
                ReplacePartValues(toolPiece2, after);
            }
            if (close)
            {
                ((EditScreen as EditScreenView).DataContext as EditScreenViewModel).Dispose();
                EditScreen = null;
                SetEditorEnableRules();
            }
        };
        EditScreenViewModel dataContext = new EditScreenViewModel(PartFiltered.Where((P p) => p.MultiTool == SelectedProfile.MultiTool).Select((Func<P, ToolItemViewModel>)((P i) => i)), toolPiece2, isUpper: false, base.ToolConfigModel, _configProvider, closeAction, _drawToolProfiles);
        EditScreenView editScreen = new EditScreenView
        {
            DataContext = dataContext
        };
        EditScreen = editScreen;
        EditScreenVisible = Visibility.Visible;
        SetEditorEnableRules();
    }

    public void SetEditorEnableRules()
    {
        if (EditScreen != null)
        {
            IsAddButtonEnabled = false;
            IsCopyButtonEnabled = false;
            IsDeleteButtonEnabled = false;
            IsEditButtonEnabled = false;
            IsOkButtonEnabled = false;
            IsCancelButtonEnabled = false;
            IsSaveButtonEnabled = false;
            return;
        }
        if (LastSelectedType == typeof(T))
        {
            ObservableCollection<T> selectedProfiles = SelectedProfiles;
            bool flag = selectedProfiles == null || selectedProfiles.Count <= 1;
            if (SelectedProfile == null)
            {
                IsCopyButtonEnabled = false;
                IsDeleteButtonEnabled = false;
                IsEditButtonEnabled = false;
                IsAddButtonEnabled = true;
                IsOkButtonEnabled = true;
                IsCancelButtonEnabled = true;
                IsSaveButtonEnabled = true;
                return;
            }
            IsCopyButtonEnabled = SelectedProfile != null && flag;
            IsEditButtonEnabled = SelectedProfile != null && flag;
            IsDeleteButtonEnabled = SelectedProfile != null;
        }
        else if (LastSelectedType == typeof(P))
        {
            ObservableCollection<P> selectedParts = SelectedParts;
            bool flag2 = selectedParts == null || selectedParts.Count <= 1;
            if (SelectedPart == null)
            {
                IsCopyButtonEnabled = false;
                IsDeleteButtonEnabled = false;
                IsEditButtonEnabled = false;
                IsAddButtonEnabled = true;
                IsOkButtonEnabled = true;
                IsCancelButtonEnabled = true;
                IsSaveButtonEnabled = true;
                return;
            }
            IsCopyButtonEnabled = SelectedPart != null && flag2;
            IsEditButtonEnabled = SelectedPart != null && flag2;
            IsDeleteButtonEnabled = SelectedPart != null;
        }
        else
        {
            IsCopyButtonEnabled = false;
            IsDeleteButtonEnabled = false;
            IsEditButtonEnabled = false;
        }
        IsAddButtonEnabled = true;
        IsOkButtonEnabled = true;
        IsCancelButtonEnabled = true;
        IsSaveButtonEnabled = true;
    }

    public void Dispose()
    {
        if (base.ImageProfile?.GetType() == typeof(Screen3D))
        {
            ((Screen3D)base.ImageProfile).Dispose();
        }
        base.ImageProfile = null;
        if (base.ImagePart?.GetType() == typeof(Screen3D))
        {
            ((Screen3D)base.ImagePart).Dispose();
        }
        base.ImagePart = null;
    }
}

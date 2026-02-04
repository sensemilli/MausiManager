using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public partial class AssemblyView : UserControl, INotifyPropertyChanged, IAssemblyView, IComponentConnector, IStyleConnector
{
    public IAssemblyViewModel _vm;

    public HashSet<FrameworkElement> ElementsScrollToView = new HashSet<FrameworkElement>();

    public IAssemblyViewModel Vm
    {
        get
        {
            return _vm;
        }
        set
        {
            if (_vm != null)
            {
                _vm.AnalyzingStatusChanged -= RefreshFilter;
                _vm.OnScrollIntoView -= ScrollIntoView;
            }
            _vm = value;
            base.DataContext = _vm;
            NotifyPropertyChanged("Vm");
            if (_vm != null)
            {
                _vm.AnalyzingStatusChanged += RefreshFilter;
                _vm.OnScrollIntoView += ScrollIntoView;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (this.PropertyChanged != null)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public AssemblyView(IAssemblyViewModel vm)
    {
        Vm = vm;
    }

    public bool Init(Action<UserControl> endView, WiCAM.Pn4000.PN3D.Assembly.Assembly? assembly, IImportArg importSetting, Action<F2exeReturnCode> answer)
    {
        if (!Vm.Init(endView, assembly, importSetting, answer))
        {
            return false;
        }
        InitializeComponent();
        GridClassic.FilterDescriptors.SuspendNotifications();
        GridClassicName.ColumnFilterDescriptor.FieldFilter.Filter1.Operator = FilterOperator.Contains;
        IColumnFilterDescriptor columnFilterDescriptor = GridClassicType.ColumnFilterDescriptor;
        columnFilterDescriptor.FieldFilter.Filter1.Value = DisassemblyPartViewModel.GetPartTypeTranslation(PartType.SmallPart);
        columnFilterDescriptor.FieldFilter.Filter1.Operator = FilterOperator.IsNotEqualTo;
        columnFilterDescriptor.FieldFilter.LogicalOperator = FilterCompositionLogicalOperator.And;
        columnFilterDescriptor.FieldFilter.Filter2.Operator = FilterOperator.IsNotNull;
        GridClassic.FilterDescriptors.ResumeNotifications();
        if (_vm.MaterialAsignments.Count > 0)
        {
            Docking.ActivePane = PaneMaterialAssignment;
        }
        return true;
    }

    public void ScrollIntoView(object element)
    {
        foreach (FrameworkElement item in ElementsScrollToView)
        {
            if (element is DisassemblyPartViewModel dataItem && item is RadGridView radGridView)
            {
                radGridView.ScrollIntoViewAsync(dataItem, null);
            }
        }
    }

    public void RefreshFilter()
    {
        Application.Current.Dispatcher.BeginInvoke((Action)delegate
        {
            if (GridClassic != null)
            {
                GridClassic.FilterDescriptors.SuspendNotifications();
                GridClassicType.ColumnFilterDescriptor.FieldFilter.LogicalOperator = FilterCompositionLogicalOperator.Or;
                GridClassicType.ColumnFilterDescriptor.FieldFilter.LogicalOperator = FilterCompositionLogicalOperator.And;
                GridClassic.FilterDescriptors.ResumeNotifications();
            }
        });
    }

    public void UIElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is RadDocking && e.Key != Key.F3 && e.Key == Key.F)
        {
            HashSet<Model> hashSet = new HashSet<Model>();
            if (Vm.CurrentPropertyPart?.Part?.ModelLowTesselation != null)
            {
                hashSet.Add(Vm.CurrentPropertyPart.Part.ModelLowTesselation);
                Vm.ImageAssembly3D.ScreenD3D.ZoomBorderless(render: true, hashSet);
            }
        }
    }

    public void EventSetter_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            if (frameworkElement.DataContext is DisassemblyNodeViewModel disassemblyNodeViewModel)
            {
                disassemblyNodeViewModel.IsMouseHovering = true;
            }
            else if (frameworkElement.DataContext is DisassemblyPartViewModel disassemblyPartViewModel)
            {
                disassemblyPartViewModel.IsMouseHovering = true;
            }
        }
    }

    public void EventSetter_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is FrameworkElement frameworkElement)
        {
            if (frameworkElement.DataContext is DisassemblyNodeViewModel disassemblyNodeViewModel)
            {
                disassemblyNodeViewModel.IsMouseHovering = false;
            }
            else if (frameworkElement.DataContext is DisassemblyPartViewModel disassemblyPartViewModel)
            {
                disassemblyPartViewModel.IsMouseHovering = false;
            }
        }
    }

    public void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!(sender is DependencyObject dependencyObject) || !(dependencyObject.GetParents().Skip(3).FirstOrDefault() is Grid grid))
        {
            return;
        }
        foreach (object child in grid.Children)
        {
            if (child is Border { Name: "BorderVisual" } border)
            {
                Binding binding = new Binding("Background");
                binding.Source = dependencyObject;
                border.SetBinding(Border.BackgroundProperty, binding);
            }
        }
    }

    public void ScrollIntoViewElement_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement item)
        {
            ElementsScrollToView.Add(item);
        }
    }

    public void RadGridView_RowLoaded(object sender, RowLoadedEventArgs e)
    {
        if (SearchElementDown(e.Row) is Rectangle rectangle)
        {
            rectangle.Opacity = 0.0;
        }
    }

    public DependencyObject SearchElementUp(DependencyObject obj, int upCount)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(obj);
        if (upCount == 0 || parent == null)
        {
            return SearchElementDown(obj);
        }
        if (parent != null)
        {
            return SearchElementUp(parent, upCount - 1);
        }
        return null;
    }

    public DependencyObject SearchElementDown(DependencyObject obj)
    {
        string text = "Background_Selected";
        int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
        for (int i = 0; i < childrenCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(obj, i);
            if (child is FrameworkElement frameworkElement && frameworkElement.Name == text)
            {
                return child;
            }
            DependencyObject dependencyObject = SearchElementDown(child);
            if (dependencyObject != null)
            {
                return dependencyObject;
            }
        }
        return null;
    }

    public void MenuShowFiltered(object sender, RadRoutedEventArgs e)
    {
        foreach (DisassemblyPartViewModel item in GridContainer1.GridView.Items)
        {
            item.Visible = true;
        }
    }

    public void MenuHideFiltered(object sender, RadRoutedEventArgs e)
    {
        foreach (DisassemblyPartViewModel item in GridContainer1.GridView.Items)
        {
            item.Visible = false;
        }
    }

    public void MenuTransparentFiltered(object sender, RadRoutedEventArgs e)
    {
        foreach (DisassemblyPartViewModel item in GridContainer1.GridView.Items)
        {
            item.Visible = null;
        }
    }

    public void MenuHideExcept(object sender, RadRoutedEventArgs e)
    {
        if (!(sender is FrameworkElement { DataContext: DisassemblyPartViewModel dataContext }))
        {
            return;
        }
        foreach (DisassemblyPartViewModel listPart in Vm.ListParts)
        {
            if (listPart == dataContext)
            {
                listPart.Visible = true;
            }
            else
            {
                listPart.Visible = false;
            }
        }
    }

    public void MenuTransparentExcept(object sender, RadRoutedEventArgs e)
    {
        if (!(sender is FrameworkElement { DataContext: DisassemblyPartViewModel dataContext }))
        {
            return;
        }
        foreach (DisassemblyPartViewModel listPart in Vm.ListParts)
        {
            if (listPart == dataContext)
            {
                listPart.Visible = true;
            }
            else
            {
                listPart.Visible = null;
            }
        }
    }

    public void PartList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement frameworkElement)
        {
            if (frameworkElement.DataContext is DisassemblyPartViewModel disassemblyPartViewModel)
            {
                disassemblyPartViewModel.CmdOpenPart.Execute(null);
            }
            else
            {
                _ = frameworkElement.DataContext is DisassemblyNodeViewModel;
            }
        }
    }

    public void GridView_PreparedCellForEdit(object sender, GridViewPreparingCellForEditEventArgs e)
    {
        if (!(e.EditingElement is RadComboBox radComboBox))
        {
            return;
        }
        radComboBox.DropDownClosed += delegate (object? s, EventArgs a)
        {
            if (s is RadComboBox element)
            {
                element.ParentOfType<GridViewCell>().CommitEdit();
            }
        };
    }

    Dispatcher Dispatcher()
    {
        return base.Dispatcher;
    }

    DependencyObjectType DependencyObjectType()
    {
        return base.DependencyObjectType;
    }

    bool IsSealed()
    {
        return base.IsSealed;
    }

    bool AllowDrop()
    {
        return base.AllowDrop;
    }

    void AllowDrop(bool value)
    {
        base.AllowDrop = value;
    }

    bool AreAnyTouchesCaptured()
    {
        return base.AreAnyTouchesCaptured;
    }

    bool AreAnyTouchesCapturedWithin()
    {
        return base.AreAnyTouchesCapturedWithin;
    }

    bool AreAnyTouchesDirectlyOver()
    {
        return base.AreAnyTouchesDirectlyOver;
    }

    bool AreAnyTouchesOver()
    {
        return base.AreAnyTouchesOver;
    }

    BitmapEffect BitmapEffect()
    {
        return base.BitmapEffect;
    }

    void BitmapEffect(BitmapEffect value)
    {
        base.BitmapEffect = value;
    }

    BitmapEffectInput BitmapEffectInput()
    {
        return base.BitmapEffectInput;
    }

    void BitmapEffectInput(BitmapEffectInput value)
    {
        base.BitmapEffectInput = value;
    }

    CacheMode CacheMode()
    {
        return base.CacheMode;
    }

    void CacheMode(CacheMode value)
    {
        base.CacheMode = value;
    }

    Geometry Clip()
    {
        return base.Clip;
    }

    void Clip(Geometry value)
    {
        base.Clip = value;
    }

    bool ClipToBounds()
    {
        return base.ClipToBounds;
    }

    void ClipToBounds(bool value)
    {
        base.ClipToBounds = value;
    }

    CommandBindingCollection CommandBindings()
    {
        return base.CommandBindings;
    }

    Size DesiredSize()
    {
        return base.DesiredSize;
    }

    Effect Effect()
    {
        return base.Effect;
    }

    void Effect(Effect value)
    {
        base.Effect = value;
    }

    InputBindingCollection InputBindings()
    {
        return base.InputBindings;
    }

    bool IsArrangeValid()
    {
        return base.IsArrangeValid;
    }

    void IsEnabled(bool value)
    {
        base.IsEnabled = value;
    }

    bool IsFocused()
    {
        return base.IsFocused;
    }

    bool IsHitTestVisible()
    {
        return base.IsHitTestVisible;
    }

    void IsHitTestVisible(bool value)
    {
        base.IsHitTestVisible = value;
    }

    bool IsInputMethodEnabled()
    {
        return base.IsInputMethodEnabled;
    }

    bool IsManipulationEnabled()
    {
        return base.IsManipulationEnabled;
    }

    void IsManipulationEnabled(bool value)
    {
        base.IsManipulationEnabled = value;
    }

    bool IsMeasureValid()
    {
        return base.IsMeasureValid;
    }

    bool IsMouseCaptureWithin()
    {
        return base.IsMouseCaptureWithin;
    }

    bool IsStylusCaptureWithin()
    {
        return base.IsStylusCaptureWithin;
    }

    bool IsVisible()
    {
        return base.IsVisible;
    }

    double Opacity()
    {
        return base.Opacity;
    }

    void Opacity(double value)
    {
        base.Opacity = value;
    }

    Brush OpacityMask()
    {
        return base.OpacityMask;
    }

    void OpacityMask(Brush value)
    {
        base.OpacityMask = value;
    }

    int PersistId()
    {
        return base.PersistId;
    }

    Size RenderSize()
    {
        return base.RenderSize;
    }

    void RenderSize(Size value)
    {
        base.RenderSize = value;
    }

    Transform RenderTransform()
    {
        return base.RenderTransform;
    }

    void RenderTransform(Transform value)
    {
        base.RenderTransform = value;
    }

    Point RenderTransformOrigin()
    {
        return base.RenderTransformOrigin;
    }

    void RenderTransformOrigin(Point value)
    {
        base.RenderTransformOrigin = value;
    }

    bool SnapsToDevicePixels()
    {
        return base.SnapsToDevicePixels;
    }

    void SnapsToDevicePixels(bool value)
    {
        base.SnapsToDevicePixels = value;
    }

    IEnumerable<TouchDevice> TouchesCaptured()
    {
        return base.TouchesCaptured;
    }

    IEnumerable<TouchDevice> TouchesCapturedWithin()
    {
        return base.TouchesCapturedWithin;
    }

    IEnumerable<TouchDevice> TouchesDirectlyOver()
    {
        return base.TouchesDirectlyOver;
    }

    IEnumerable<TouchDevice> TouchesOver()
    {
        return base.TouchesOver;
    }

    string Uid()
    {
        return base.Uid;
    }

    void Uid(string value)
    {
        base.Uid = value;
    }

    Visibility Visibility()
    {
        return base.Visibility;
    }

    void Visibility(Visibility value)
    {
        base.Visibility = value;
    }

    double ActualHeight()
    {
        return base.ActualHeight;
    }

    double ActualWidth()
    {
        return base.ActualWidth;
    }

    BindingGroup BindingGroup()
    {
        return base.BindingGroup;
    }

    void BindingGroup(BindingGroup value)
    {
        base.BindingGroup = value;
    }

    ContextMenu ContextMenu()
    {
        return base.ContextMenu;
    }

    void ContextMenu(ContextMenu value)
    {
        base.ContextMenu = value;
    }

    Cursor Cursor()
    {
        return base.Cursor;
    }

    void Cursor(Cursor value)
    {
        base.Cursor = value;
    }

    object DataContext()
    {
        return base.DataContext;
    }

    void DataContext(object value)
    {
        base.DataContext = value;
    }

    FlowDirection FlowDirection()
    {
        return base.FlowDirection;
    }

    void FlowDirection(FlowDirection value)
    {
        base.FlowDirection = value;
    }

    Style FocusVisualStyle()
    {
        return base.FocusVisualStyle;
    }

    void FocusVisualStyle(Style value)
    {
        base.FocusVisualStyle = value;
    }

    bool ForceCursor()
    {
        return base.ForceCursor;
    }

    void ForceCursor(bool value)
    {
        base.ForceCursor = value;
    }

    double Height()
    {
        return base.Height;
    }

    void Height(double value)
    {
        base.Height = value;
    }

    HorizontalAlignment HorizontalAlignment()
    {
        return base.HorizontalAlignment;
    }

    void HorizontalAlignment(HorizontalAlignment value)
    {
        base.HorizontalAlignment = value;
    }

    InputScope InputScope()
    {
        return base.InputScope;
    }

    void InputScope(InputScope value)
    {
        base.InputScope = value;
    }

    bool IsInitialized()
    {
        return base.IsInitialized;
    }

    bool IsLoaded()
    {
        return base.IsLoaded;
    }

    XmlLanguage Language()
    {
        return base.Language;
    }

    void Language(XmlLanguage value)
    {
        base.Language = value;
    }

    Transform LayoutTransform()
    {
        return base.LayoutTransform;
    }

    void LayoutTransform(Transform value)
    {
        base.LayoutTransform = value;
    }

    Thickness Margin()
    {
        return base.Margin;
    }

    void Margin(Thickness value)
    {
        base.Margin = value;
    }

    double MaxHeight()
    {
        return base.MaxHeight;
    }

    void MaxHeight(double value)
    {
        base.MaxHeight = value;
    }

    double MaxWidth()
    {
        return base.MaxWidth;
    }

    void MaxWidth(double value)
    {
        base.MaxWidth = value;
    }

    double MinHeight()
    {
        return base.MinHeight;
    }

    void MinHeight(double value)
    {
        base.MinHeight = value;
    }

    double MinWidth()
    {
        return base.MinWidth;
    }

    void MinWidth(double value)
    {
        base.MinWidth = value;
    }

    bool OverridesDefaultStyle()
    {
        return base.OverridesDefaultStyle;
    }

    void OverridesDefaultStyle(bool value)
    {
        base.OverridesDefaultStyle = value;
    }

    DependencyObject Parent()
    {
        return base.Parent;
    }

    ResourceDictionary Resources()
    {
        return base.Resources;
    }

    void Resources(ResourceDictionary value)
    {
        base.Resources = value;
    }

    Style Style()
    {
        return base.Style;
    }

    void Style(Style value)
    {
        base.Style = value;
    }

    object Tag()
    {
        return base.Tag;
    }

    void Tag(object value)
    {
        base.Tag = value;
    }

    DependencyObject TemplatedParent()
    {
        return base.TemplatedParent;
    }

    object ToolTip()
    {
        return base.ToolTip;
    }

    void ToolTip(object value)
    {
        base.ToolTip = value;
    }

    TriggerCollection Triggers()
    {
        return base.Triggers;
    }

    bool UseLayoutRounding()
    {
        return base.UseLayoutRounding;
    }

    void UseLayoutRounding(bool value)
    {
        base.UseLayoutRounding = value;
    }

    VerticalAlignment VerticalAlignment()
    {
        return base.VerticalAlignment;
    }

    void VerticalAlignment(VerticalAlignment value)
    {
        base.VerticalAlignment = value;
    }

    double Width()
    {
        return base.Width;
    }

    void Width(double value)
    {
        base.Width = value;
    }

    Brush Background()
    {
        return base.Background;
    }

    void Background(Brush value)
    {
        base.Background = value;
    }

    Brush BorderBrush()
    {
        return base.BorderBrush;
    }

    void BorderBrush(Brush value)
    {
        base.BorderBrush = value;
    }

    Thickness BorderThickness()
    {
        return base.BorderThickness;
    }

    void BorderThickness(Thickness value)
    {
        base.BorderThickness = value;
    }

    FontFamily FontFamily()
    {
        return base.FontFamily;
    }

    void FontFamily(FontFamily value)
    {
        base.FontFamily = value;
    }

    double FontSize()
    {
        return base.FontSize;
    }

    void FontSize(double value)
    {
        base.FontSize = value;
    }

    FontStretch FontStretch()
    {
        return base.FontStretch;
    }

    void FontStretch(FontStretch value)
    {
        base.FontStretch = value;
    }

    FontStyle FontStyle()
    {
        return base.FontStyle;
    }

    void FontStyle(FontStyle value)
    {
        base.FontStyle = value;
    }

    FontWeight FontWeight()
    {
        return base.FontWeight;
    }

    void FontWeight(FontWeight value)
    {
        base.FontWeight = value;
    }

    Brush Foreground()
    {
        return base.Foreground;
    }

    void Foreground(Brush value)
    {
        base.Foreground = value;
    }

    HorizontalAlignment HorizontalContentAlignment()
    {
        return base.HorizontalContentAlignment;
    }

    void HorizontalContentAlignment(HorizontalAlignment value)
    {
        base.HorizontalContentAlignment = value;
    }

    bool IsTabStop()
    {
        return base.IsTabStop;
    }

    void IsTabStop(bool value)
    {
        base.IsTabStop = value;
    }

    Thickness Padding()
    {
        return base.Padding;
    }

    void Padding(Thickness value)
    {
        base.Padding = value;
    }

    int TabIndex()
    {
        return base.TabIndex;
    }

    void TabIndex(int value)
    {
        base.TabIndex = value;
    }

    ControlTemplate Template()
    {
        return base.Template;
    }

    void Template(ControlTemplate value)
    {
        base.Template = value;
    }

    public VerticalAlignment VerticalContentAlignment()
    {
        return base.VerticalContentAlignment;
    }

    public void VerticalContentAlignment(VerticalAlignment value)
    {
        base.VerticalContentAlignment = value;
    }

    public object Content()
    {
        return base.Content;
    }

    public void Content(object value)
    {
        base.Content = value;
    }

    public string ContentStringFormat()
    {
        return base.ContentStringFormat;
    }

    public void ContentStringFormat(string value)
    {
        base.ContentStringFormat = value;
    }

    public DataTemplate ContentTemplate()
    {
        return base.ContentTemplate;
    }

    public void ContentTemplate(DataTemplate value)
    {
        base.ContentTemplate = value;
    }

    public DataTemplateSelector ContentTemplateSelector()
    {
        return base.ContentTemplateSelector;
    }

    public void ContentTemplateSelector(DataTemplateSelector value)
    {
        base.ContentTemplateSelector = value;
    }

    public bool HasContent()
    {
        return base.HasContent;
    }

    public bool CheckAccess()
    {
        return CheckAccess();
    }

    public void VerifyAccess()
    {
        VerifyAccess();
    }

    public void ClearValue(DependencyProperty dp)
    {
        ClearValue(dp);
    }

    public void ClearValue(DependencyPropertyKey key)
    {
        ClearValue(key);
    }

    public void CoerceValue(DependencyProperty dp)
    {
        CoerceValue(dp);
    }

    public LocalValueEnumerator GetLocalValueEnumerator()
    {
        return GetLocalValueEnumerator();
    }

    public object GetValue(DependencyProperty dp)
    {
        return GetValue(dp);
    }

    public void InvalidateProperty(DependencyProperty dp)
    {
        InvalidateProperty(dp);
    }

    public object ReadLocalValue(DependencyProperty dp)
    {
        return ReadLocalValue(dp);
    }

    public void SetCurrentValue(DependencyProperty dp, object value)
    {
        SetCurrentValue(dp, value);
    }

    public void SetValue(DependencyProperty dp, object value)
    {
        SetValue(dp, value);
    }

    public void SetValue(DependencyPropertyKey key, object value)
    {
        SetValue(key, value);
    }

    public DependencyObject FindCommonVisualAncestor(DependencyObject otherVisual)
    {
        return FindCommonVisualAncestor(otherVisual);
    }

    public bool IsAncestorOf(DependencyObject descendant)
    {
        return IsAncestorOf(descendant);
    }

    public bool IsDescendantOf(DependencyObject ancestor)
    {
        return IsDescendantOf(ancestor);
    }

    public Point PointFromScreen(Point point)
    {
        return PointFromScreen(point);
    }

    public Point PointToScreen(Point point)
    {
        return PointToScreen(point);
    }

    public GeneralTransform2DTo3D TransformToAncestor(Visual3D ancestor)
    {
        return TransformToAncestor(ancestor);
    }

    public GeneralTransform TransformToAncestor(Visual ancestor)
    {
        return TransformToAncestor(ancestor);
    }

    public GeneralTransform TransformToDescendant(Visual descendant)
    {
        return TransformToDescendant(descendant);
    }

    public GeneralTransform TransformToVisual(Visual visual)
    {
        return TransformToVisual(visual);
    }

    public void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
    {
        AddHandler(routedEvent, handler, handledEventsToo);
    }

    public void AddToEventRoute(EventRoute route, RoutedEventArgs e)
    {
        AddToEventRoute(route, e);
    }

    public void Arrange(Rect finalRect)
    {
        Arrange(finalRect);
    }

    public bool CaptureTouch(TouchDevice touchDevice)
    {
        return CaptureTouch(touchDevice);
    }

    public IInputElement InputHitTest(Point point)
    {
        return InputHitTest(point);
    }

    public void InvalidateArrange()
    {
        InvalidateArrange();
    }

    public void InvalidateMeasure()
    {
        InvalidateMeasure();
    }

    public void InvalidateVisual()
    {
        InvalidateVisual();
    }

    public void Measure(Size availableSize)
    {
        Measure(availableSize);
    }

    public void ReleaseAllTouchCaptures()
    {
        ReleaseAllTouchCaptures();
    }

    public bool ReleaseTouchCapture(TouchDevice touchDevice)
    {
        return ReleaseTouchCapture(touchDevice);
    }

    public bool ShouldSerializeCommandBindings()
    {
        return ShouldSerializeCommandBindings();
    }

    public bool ShouldSerializeInputBindings()
    {
        return ShouldSerializeInputBindings();
    }

    public Point TranslatePoint(Point point, UIElement relativeTo)
    {
        return TranslatePoint(point, relativeTo);
    }

    public void UpdateLayout()
    {
        UpdateLayout();
    }

    public void AddDragEnter(DragEventHandler value)
    {
        base.DragEnter += value;
    }

    public void RemoveDragEnter(DragEventHandler value)
    {
        base.DragEnter -= value;
    }

    public void AddDragLeave(DragEventHandler value)
    {
        base.DragLeave += value;
    }

    public void RemoveDragLeave(DragEventHandler value)
    {
        base.DragLeave -= value;
    }

    public void AddDragOver(DragEventHandler value)
    {
        base.DragOver += value;
    }

    public void RemoveDragOver(DragEventHandler value)
    {
        base.DragOver -= value;
    }

    public void AddDrop(DragEventHandler value)
    {
        base.Drop += value;
    }

    public void RemoveDrop(DragEventHandler value)
    {
        base.Drop -= value;
    }

    public void AddFocusableChanged(DependencyPropertyChangedEventHandler value)
    {
        base.FocusableChanged += value;
    }

    public void RemoveFocusableChanged(DependencyPropertyChangedEventHandler value)
    {
        base.FocusableChanged -= value;
    }

    public void AddGiveFeedback(GiveFeedbackEventHandler value)
    {
        base.GiveFeedback += value;
    }

    public void RemoveGiveFeedback(GiveFeedbackEventHandler value)
    {
        base.GiveFeedback -= value;
    }

    public void AddGotFocus(RoutedEventHandler value)
    {
        base.GotFocus += value;
    }

    public void RemoveGotFocus(RoutedEventHandler value)
    {
        base.GotFocus -= value;
    }

    public void AddGotTouchCapture(EventHandler<TouchEventArgs> value)
    {
        base.GotTouchCapture += value;
    }

    public void RemoveGotTouchCapture(EventHandler<TouchEventArgs> value)
    {
        base.GotTouchCapture -= value;
    }

    public void AddIsEnabledChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsEnabledChanged += value;
    }

    public void RemoveIsEnabledChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsEnabledChanged -= value;
    }

    public void AddIsHitTestVisibleChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsHitTestVisibleChanged += value;
    }

    public void RemoveIsHitTestVisibleChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsHitTestVisibleChanged -= value;
    }

    public void AddIsKeyboardFocusedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsKeyboardFocusedChanged += value;
    }

    public void RemoveIsKeyboardFocusedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsKeyboardFocusedChanged -= value;
    }

    public void AddIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsKeyboardFocusWithinChanged += value;
    }

    public void RemoveIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsKeyboardFocusWithinChanged -= value;
    }

    public void AddIsMouseCapturedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseCapturedChanged += value;
    }

    public void RemoveIsMouseCapturedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseCapturedChanged -= value;
    }

    public void AddIsMouseCaptureWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseCaptureWithinChanged += value;
    }

    public void RemoveIsMouseCaptureWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseCaptureWithinChanged -= value;
    }

    public void AddIsMouseDirectlyOverChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseDirectlyOverChanged += value;
    }

    public void RemoveIsMouseDirectlyOverChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsMouseDirectlyOverChanged -= value;
    }

    public void AddIsStylusCapturedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusCapturedChanged += value;
    }

    public void RemoveIsStylusCapturedChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusCapturedChanged -= value;
    }

    public void AddIsStylusCaptureWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusCaptureWithinChanged += value;
    }

    public void RemoveIsStylusCaptureWithinChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusCaptureWithinChanged -= value;
    }

    public void AddIsStylusDirectlyOverChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusDirectlyOverChanged += value;
    }

    public void RemoveIsStylusDirectlyOverChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsStylusDirectlyOverChanged -= value;
    }

    public void AddIsVisibleChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsVisibleChanged += value;
    }

    public void RemoveIsVisibleChanged(DependencyPropertyChangedEventHandler value)
    {
        base.IsVisibleChanged -= value;
    }

    public void AddLayoutUpdated(EventHandler value)
    {
        base.LayoutUpdated += value;
    }

    public void RemoveLayoutUpdated(EventHandler value)
    {
        base.LayoutUpdated -= value;
    }

    public void AddLostFocus(RoutedEventHandler value)
    {
        base.LostFocus += value;
    }

    public void RemoveLostFocus(RoutedEventHandler value)
    {
        base.LostFocus -= value;
    }

    public void AddLostTouchCapture(EventHandler<TouchEventArgs> value)
    {
        base.LostTouchCapture += value;
    }

    public void RemoveLostTouchCapture(EventHandler<TouchEventArgs> value)
    {
        base.LostTouchCapture -= value;
    }

    public void AddManipulationBoundaryFeedback(EventHandler<ManipulationBoundaryFeedbackEventArgs> value)
    {
        base.ManipulationBoundaryFeedback += value;
    }

    public void RemoveManipulationBoundaryFeedback(EventHandler<ManipulationBoundaryFeedbackEventArgs> value)
    {
        base.ManipulationBoundaryFeedback -= value;
    }

    public void AddManipulationCompleted(EventHandler<ManipulationCompletedEventArgs> value)
    {
        base.ManipulationCompleted += value;
    }

    public void RemoveManipulationCompleted(EventHandler<ManipulationCompletedEventArgs> value)
    {
        base.ManipulationCompleted -= value;
    }

    public void AddManipulationDelta(EventHandler<ManipulationDeltaEventArgs> value)
    {
        base.ManipulationDelta += value;
    }

    public void RemoveManipulationDelta(EventHandler<ManipulationDeltaEventArgs> value)
    {
        base.ManipulationDelta -= value;
    }

    public void AddManipulationInertiaStarting(EventHandler<ManipulationInertiaStartingEventArgs> value)
    {
        base.ManipulationInertiaStarting += value;
    }

    public void RemoveManipulationInertiaStarting(EventHandler<ManipulationInertiaStartingEventArgs> value)
    {
        base.ManipulationInertiaStarting -= value;
    }

    public void AddManipulationStarted(EventHandler<ManipulationStartedEventArgs> value)
    {
        base.ManipulationStarted += value;
    }

    public void RemoveManipulationStarted(EventHandler<ManipulationStartedEventArgs> value)
    {
        base.ManipulationStarted -= value;
    }

    public void AddManipulationStarting(EventHandler<ManipulationStartingEventArgs> value)
    {
        base.ManipulationStarting += value;
    }

    public void RemoveManipulationStarting(EventHandler<ManipulationStartingEventArgs> value)
    {
        base.ManipulationStarting -= value;
    }

    public void AddMouseDown(MouseButtonEventHandler value)
    {
        base.MouseDown += value;
    }

    public void RemoveMouseDown(MouseButtonEventHandler value)
    {
        base.MouseDown -= value;
    }

    public void AddMouseUp(MouseButtonEventHandler value)
    {
        base.MouseUp += value;
    }

    public void RemoveMouseUp(MouseButtonEventHandler value)
    {
        base.MouseUp -= value;
    }

    public void AddPreviewDragEnter(DragEventHandler value)
    {
        base.PreviewDragEnter += value;
    }

    public void RemovePreviewDragEnter(DragEventHandler value)
    {
        base.PreviewDragEnter -= value;
    }

    public void AddPreviewDragLeave(DragEventHandler value)
    {
        base.PreviewDragLeave += value;
    }

    public void RemovePreviewDragLeave(DragEventHandler value)
    {
        base.PreviewDragLeave -= value;
    }

    public void AddPreviewDragOver(DragEventHandler value)
    {
        base.PreviewDragOver += value;
    }

    public void RemovePreviewDragOver(DragEventHandler value)
    {
        base.PreviewDragOver -= value;
    }

    public void AddPreviewDrop(DragEventHandler value)
    {
        base.PreviewDrop += value;
    }

    public void RemovePreviewDrop(DragEventHandler value)
    {
        base.PreviewDrop -= value;
    }

    public void AddPreviewGiveFeedback(GiveFeedbackEventHandler value)
    {
        base.PreviewGiveFeedback += value;
    }

    public void RemovePreviewGiveFeedback(GiveFeedbackEventHandler value)
    {
        base.PreviewGiveFeedback -= value;
    }

    public void AddPreviewMouseDown(MouseButtonEventHandler value)
    {
        base.PreviewMouseDown += value;
    }

    public void RemovePreviewMouseDown(MouseButtonEventHandler value)
    {
        base.PreviewMouseDown -= value;
    }

    public void AddPreviewMouseUp(MouseButtonEventHandler value)
    {
        base.PreviewMouseUp += value;
    }

    public void RemovePreviewMouseUp(MouseButtonEventHandler value)
    {
        base.PreviewMouseUp -= value;
    }

    public void AddPreviewQueryContinueDrag(QueryContinueDragEventHandler value)
    {
        base.PreviewQueryContinueDrag += value;
    }

    public void RemovePreviewQueryContinueDrag(QueryContinueDragEventHandler value)
    {
        base.PreviewQueryContinueDrag -= value;
    }

    public void AddPreviewTouchDown(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchDown += value;
    }

    public void RemovePreviewTouchDown(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchDown -= value;
    }

    public void AddPreviewTouchMove(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchMove += value;
    }

    public void RemovePreviewTouchMove(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchMove -= value;
    }

    public void AddPreviewTouchUp(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchUp += value;
    }

    public void RemovePreviewTouchUp(EventHandler<TouchEventArgs> value)
    {
        base.PreviewTouchUp -= value;
    }

    public void AddQueryContinueDrag(QueryContinueDragEventHandler value)
    {
        base.QueryContinueDrag += value;
    }

    public void RemoveQueryContinueDrag(QueryContinueDragEventHandler value)
    {
        base.QueryContinueDrag -= value;
    }

    public void AddQueryCursor(QueryCursorEventHandler value)
    {
        base.QueryCursor += value;
    }

    public void RemoveQueryCursor(QueryCursorEventHandler value)
    {
        base.QueryCursor -= value;
    }

    public void AddTouchDown(EventHandler<TouchEventArgs> value)
    {
        base.TouchDown += value;
    }

    public void RemoveTouchDown(EventHandler<TouchEventArgs> value)
    {
        base.TouchDown -= value;
    }

    public void AddTouchEnter(EventHandler<TouchEventArgs> value)
    {
        base.TouchEnter += value;
    }

    public void RemoveTouchEnter(EventHandler<TouchEventArgs> value)
    {
        base.TouchEnter -= value;
    }

    public void AddTouchLeave(EventHandler<TouchEventArgs> value)
    {
        base.TouchLeave += value;
    }

    public void RemoveTouchLeave(EventHandler<TouchEventArgs> value)
    {
        base.TouchLeave -= value;
    }

    public void AddTouchMove(EventHandler<TouchEventArgs> value)
    {
        base.TouchMove += value;
    }

    public void RemoveTouchMove(EventHandler<TouchEventArgs> value)
    {
        base.TouchMove -= value;
    }

    public void AddTouchUp(EventHandler<TouchEventArgs> value)
    {
        base.TouchUp += value;
    }

    public void RemoveTouchUp(EventHandler<TouchEventArgs> value)
    {
        base.TouchUp -= value;
    }

    public bool ApplyTemplate()
    {
        return ApplyTemplate();
    }

    public void BeginStoryboard(Storyboard storyboard)
    {
        BeginStoryboard(storyboard);
    }

    public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior)
    {
        BeginStoryboard(storyboard, handoffBehavior);
    }

    public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
    {
        BeginStoryboard(storyboard, handoffBehavior, isControllable);
    }

    public void BringIntoView()
    {
        BringIntoView();
    }

    public void BringIntoView(Rect targetRectangle)
    {
        BringIntoView(targetRectangle);
    }

    public object FindName(string name)
    {
        return FindName(name);
    }

    public object FindResource(object resourceKey)
    {
        return FindResource(resourceKey);
    }

    public BindingExpression GetBindingExpression(DependencyProperty dp)
    {
        return GetBindingExpression(dp);
    }

    public void RegisterName(string name, object scopedElement)
    {
        RegisterName(name, scopedElement);
    }

    public BindingExpression SetBinding(DependencyProperty dp, string path)
    {
        return SetBinding(dp, path);
    }

    public BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding)
    {
        return SetBinding(dp, binding);
    }

    public void SetResourceReference(DependencyProperty dp, object name)
    {
        SetResourceReference(dp, name);
    }

    public bool ShouldSerializeResources()
    {
        return ShouldSerializeResources();
    }

    public bool ShouldSerializeStyle()
    {
        return ShouldSerializeStyle();
    }

    public bool ShouldSerializeTriggers()
    {
        return ShouldSerializeTriggers();
    }

    public object TryFindResource(object resourceKey)
    {
        return TryFindResource(resourceKey);
    }

    public void UnregisterName(string name)
    {
        UnregisterName(name);
    }

    public void UpdateDefaultStyle()
    {
        UpdateDefaultStyle();
    }

    public void ContextMenuClosing(ContextMenuEventHandler value)
    {
        base.ContextMenuClosing += value;
    }

    public void RemoveContextMenuClosing(ContextMenuEventHandler value)
    {
        base.ContextMenuClosing -= value;
    }

    public void ContextMenuOpening(ContextMenuEventHandler value)
    {
        base.ContextMenuOpening += value;
    }

    public void RemoveContextMenuOpening(ContextMenuEventHandler value)
    {
        base.ContextMenuOpening -= value;
    }

    public void AddDataContextChanged(DependencyPropertyChangedEventHandler value)
    {
        base.DataContextChanged += value;
    }

    public void RemoveDataContextChanged(DependencyPropertyChangedEventHandler value)
    {
        base.DataContextChanged -= value;
    }

    public void AddInitialized(EventHandler value)
    {
        base.Initialized += value;
    }

    public void RemoveInitialized(EventHandler value)
    {
        base.Initialized -= value;
    }

    public void AddLoaded(RoutedEventHandler value)
    {
        base.Loaded += value;
    }

    public void RemoveLoaded(RoutedEventHandler value)
    {
        base.Loaded -= value;
    }

    public void AddRequestBringIntoView(RequestBringIntoViewEventHandler value)
    {
        base.RequestBringIntoView += value;
    }

    public void RemoveRequestBringIntoView(RequestBringIntoViewEventHandler value)
    {
        base.RequestBringIntoView -= value;
    }

    public void AddSizeChanged(SizeChangedEventHandler value)
    {
        base.SizeChanged += value;
    }

    public void RemoveSizeChanged(SizeChangedEventHandler value)
    {
        base.SizeChanged -= value;
    }

    public void AddSourceUpdated(EventHandler<DataTransferEventArgs> value)
    {
        base.SourceUpdated += value;
    }

    public void RemoveSourceUpdated(EventHandler<DataTransferEventArgs> value)
    {
        base.SourceUpdated -= value;
    }

    public void AddTargetUpdated(EventHandler<DataTransferEventArgs> value)
    {
        base.TargetUpdated += value;
    }

    public void RemoveTargetUpdated(EventHandler<DataTransferEventArgs> value)
    {
        base.TargetUpdated -= value;
    }

    public void AddToolTipClosing(ToolTipEventHandler value)
    {
        base.ToolTipClosing += value;
    }

    public void RemoveToolTipClosing(ToolTipEventHandler value)
    {
        base.ToolTipClosing -= value;
    }

    public void AddToolTipOpening(ToolTipEventHandler value)
    {
        base.ToolTipOpening += value;
    }

    public void RemoveToolTipOpening(ToolTipEventHandler value)
    {
        base.ToolTipOpening -= value;
    }

    public void AddUnloaded(RoutedEventHandler value)
    {
        base.Unloaded += value;
    }

    public void RemoveUnloaded(RoutedEventHandler value)
    {
        base.Unloaded -= value;
    }

    public void AddMouseDoubleClick(MouseButtonEventHandler value)
    {
        base.MouseDoubleClick += value;
    }

    public void RemoveMouseDoubleClick(MouseButtonEventHandler value)
    {
        base.MouseDoubleClick -= value;
    }

    public void AddPreviewMouseDoubleClick(MouseButtonEventHandler value)
    {
        base.PreviewMouseDoubleClick += value;
    }

    public void RemovePreviewMouseDoubleClick(MouseButtonEventHandler value)
    {
        base.PreviewMouseDoubleClick -= value;
    }

    public DependencyObject PredictFocus(Telerik.Windows.Controls.FocusNavigationDirection direction)
    {
        return ((IAssemblyView)ThisUserControl).PredictFocus(direction);
    }
}

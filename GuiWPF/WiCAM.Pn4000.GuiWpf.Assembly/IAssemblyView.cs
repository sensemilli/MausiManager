using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Assembly;
using FocusNavigationDirection = Telerik.Windows.Controls.FocusNavigationDirection;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public interface IAssemblyView
{
	IAssemblyViewModel Vm { get; set; }

	Dispatcher Dispatcher { get; }

	DependencyObjectType DependencyObjectType { get; }

	bool IsSealed { get; }

	bool AllowDrop { get; set; }

	bool AreAnyTouchesCaptured { get; }

	bool AreAnyTouchesCapturedWithin { get; }

	bool AreAnyTouchesDirectlyOver { get; }

	bool AreAnyTouchesOver { get; }

	BitmapEffect BitmapEffect { get; set; }

	BitmapEffectInput BitmapEffectInput { get; set; }

	CacheMode CacheMode { get; set; }

	Geometry Clip { get; set; }

	bool ClipToBounds { get; set; }

	CommandBindingCollection CommandBindings { get; }

	Size DesiredSize { get; }

	Effect Effect { get; set; }

	bool Focusable { get; set; }

	bool HasAnimatedProperties { get; }

	InputBindingCollection InputBindings { get; }

	bool IsArrangeValid { get; }

	bool IsEnabled { get; set; }

	bool IsFocused { get; }

	bool IsHitTestVisible { get; set; }

	bool IsInputMethodEnabled { get; }

	bool IsKeyboardFocused { get; }

	bool IsKeyboardFocusWithin { get; }

	bool IsManipulationEnabled { get; set; }

	bool IsMeasureValid { get; }

	bool IsMouseCaptured { get; }

	bool IsMouseCaptureWithin { get; }

	bool IsMouseDirectlyOver { get; }

	bool IsMouseOver { get; }

	bool IsStylusCaptured { get; }

	bool IsStylusCaptureWithin { get; }

	bool IsStylusDirectlyOver { get; }

	bool IsStylusOver { get; }

	bool IsVisible { get; }

	double Opacity { get; set; }

	Brush OpacityMask { get; set; }

	int PersistId { get; }

	Size RenderSize { get; set; }

	Transform RenderTransform { get; set; }

	Point RenderTransformOrigin { get; set; }

	bool SnapsToDevicePixels { get; set; }

	IEnumerable<TouchDevice> TouchesCaptured { get; }

	IEnumerable<TouchDevice> TouchesCapturedWithin { get; }

	IEnumerable<TouchDevice> TouchesDirectlyOver { get; }

	IEnumerable<TouchDevice> TouchesOver { get; }

	string Uid { get; set; }

	Visibility Visibility { get; set; }

	double ActualHeight { get; }

	double ActualWidth { get; }

	BindingGroup BindingGroup { get; set; }

	ContextMenu ContextMenu { get; set; }

	Cursor Cursor { get; set; }

	object DataContext { get; set; }

	FlowDirection FlowDirection { get; set; }

	Style FocusVisualStyle { get; set; }

	bool ForceCursor { get; set; }

	double Height { get; set; }

	HorizontalAlignment HorizontalAlignment { get; set; }

	InputScope InputScope { get; set; }

	bool IsInitialized { get; }

	bool IsLoaded { get; }

	XmlLanguage Language { get; set; }

	Transform LayoutTransform { get; set; }

	Thickness Margin { get; set; }

	double MaxHeight { get; set; }

	double MaxWidth { get; set; }

	double MinHeight { get; set; }

	double MinWidth { get; set; }

	string Name { get; set; }

	bool OverridesDefaultStyle { get; set; }

	DependencyObject Parent { get; }

	ResourceDictionary Resources { get; set; }

	Style Style { get; set; }

	object Tag { get; set; }

	DependencyObject TemplatedParent { get; }

	object ToolTip { get; set; }

	TriggerCollection Triggers { get; }

	bool UseLayoutRounding { get; set; }

	VerticalAlignment VerticalAlignment { get; set; }

	double Width { get; set; }

	Brush Background { get; set; }

	Brush BorderBrush { get; set; }

	Thickness BorderThickness { get; set; }

	FontFamily FontFamily { get; set; }

	double FontSize { get; set; }

	FontStretch FontStretch { get; set; }

	FontStyle FontStyle { get; set; }

	FontWeight FontWeight { get; set; }

	Brush Foreground { get; set; }

	HorizontalAlignment HorizontalContentAlignment { get; set; }

	bool IsTabStop { get; set; }

	Thickness Padding { get; set; }

	int TabIndex { get; set; }

	ControlTemplate Template { get; set; }

	VerticalAlignment VerticalContentAlignment { get; set; }

	object Content { get; set; }

	string ContentStringFormat { get; set; }

	DataTemplate ContentTemplate { get; set; }

	DataTemplateSelector ContentTemplateSelector { get; set; }

	bool HasContent { get; }

	event PropertyChangedEventHandler PropertyChanged;

	event DragEventHandler DragEnter;

	event DragEventHandler DragLeave;

	event DragEventHandler DragOver;

	event DragEventHandler Drop;

	event DependencyPropertyChangedEventHandler FocusableChanged;

	event GiveFeedbackEventHandler GiveFeedback;

	event RoutedEventHandler GotFocus;

	event KeyboardFocusChangedEventHandler GotKeyboardFocus;

	event MouseEventHandler GotMouseCapture;

	event StylusEventHandler GotStylusCapture;

	event EventHandler<TouchEventArgs> GotTouchCapture;

	event DependencyPropertyChangedEventHandler IsEnabledChanged;

	event DependencyPropertyChangedEventHandler IsHitTestVisibleChanged;

	event DependencyPropertyChangedEventHandler IsKeyboardFocusedChanged;

	event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged;

	event DependencyPropertyChangedEventHandler IsMouseCapturedChanged;

	event DependencyPropertyChangedEventHandler IsMouseCaptureWithinChanged;

	event DependencyPropertyChangedEventHandler IsMouseDirectlyOverChanged;

	event DependencyPropertyChangedEventHandler IsStylusCapturedChanged;

	event DependencyPropertyChangedEventHandler IsStylusCaptureWithinChanged;

	event DependencyPropertyChangedEventHandler IsStylusDirectlyOverChanged;

	event DependencyPropertyChangedEventHandler IsVisibleChanged;

	event KeyEventHandler KeyDown;

	event KeyEventHandler KeyUp;

	event EventHandler LayoutUpdated;

	event RoutedEventHandler LostFocus;

	event KeyboardFocusChangedEventHandler LostKeyboardFocus;

	event MouseEventHandler LostMouseCapture;

	event StylusEventHandler LostStylusCapture;

	event EventHandler<TouchEventArgs> LostTouchCapture;

	event EventHandler<ManipulationBoundaryFeedbackEventArgs> ManipulationBoundaryFeedback;

	event EventHandler<ManipulationCompletedEventArgs> ManipulationCompleted;

	event EventHandler<ManipulationDeltaEventArgs> ManipulationDelta;

	event EventHandler<ManipulationInertiaStartingEventArgs> ManipulationInertiaStarting;

	event EventHandler<ManipulationStartedEventArgs> ManipulationStarted;

	event EventHandler<ManipulationStartingEventArgs> ManipulationStarting;

	event MouseButtonEventHandler MouseDown;

	event MouseEventHandler MouseEnter;

	event MouseEventHandler MouseLeave;

	event MouseButtonEventHandler MouseLeftButtonDown;

	event MouseButtonEventHandler MouseLeftButtonUp;

	event MouseEventHandler MouseMove;

	event MouseButtonEventHandler MouseRightButtonDown;

	event MouseButtonEventHandler MouseRightButtonUp;

	event MouseButtonEventHandler MouseUp;

	event MouseWheelEventHandler MouseWheel;

	event DragEventHandler PreviewDragEnter;

	event DragEventHandler PreviewDragLeave;

	event DragEventHandler PreviewDragOver;

	event DragEventHandler PreviewDrop;

	event GiveFeedbackEventHandler PreviewGiveFeedback;

	event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;

	event KeyEventHandler PreviewKeyDown;

	event KeyEventHandler PreviewKeyUp;

	event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;

	event MouseButtonEventHandler PreviewMouseDown;

	event MouseButtonEventHandler PreviewMouseLeftButtonDown;

	event MouseButtonEventHandler PreviewMouseLeftButtonUp;

	event MouseEventHandler PreviewMouseMove;

	event MouseButtonEventHandler PreviewMouseRightButtonDown;

	event MouseButtonEventHandler PreviewMouseRightButtonUp;

	event MouseButtonEventHandler PreviewMouseUp;

	event MouseWheelEventHandler PreviewMouseWheel;

	event QueryContinueDragEventHandler PreviewQueryContinueDrag;

	event StylusButtonEventHandler PreviewStylusButtonDown;

	event StylusButtonEventHandler PreviewStylusButtonUp;

	event StylusDownEventHandler PreviewStylusDown;

	event StylusEventHandler PreviewStylusInAirMove;

	event StylusEventHandler PreviewStylusInRange;

	event StylusEventHandler PreviewStylusMove;

	event StylusEventHandler PreviewStylusOutOfRange;

	event StylusSystemGestureEventHandler PreviewStylusSystemGesture;

	event StylusEventHandler PreviewStylusUp;

	event TextCompositionEventHandler PreviewTextInput;

	event EventHandler<TouchEventArgs> PreviewTouchDown;

	event EventHandler<TouchEventArgs> PreviewTouchMove;

	event EventHandler<TouchEventArgs> PreviewTouchUp;

	event QueryContinueDragEventHandler QueryContinueDrag;

	event QueryCursorEventHandler QueryCursor;

	event StylusButtonEventHandler StylusButtonDown;

	event StylusButtonEventHandler StylusButtonUp;

	event StylusDownEventHandler StylusDown;

	event StylusEventHandler StylusEnter;

	event StylusEventHandler StylusInAirMove;

	event StylusEventHandler StylusInRange;

	event StylusEventHandler StylusLeave;

	event StylusEventHandler StylusMove;

	event StylusEventHandler StylusOutOfRange;

	event StylusSystemGestureEventHandler StylusSystemGesture;

	event StylusEventHandler StylusUp;

	event TextCompositionEventHandler TextInput;

	event EventHandler<TouchEventArgs> TouchDown;

	event EventHandler<TouchEventArgs> TouchEnter;

	event EventHandler<TouchEventArgs> TouchLeave;

	event EventHandler<TouchEventArgs> TouchMove;

	event EventHandler<TouchEventArgs> TouchUp;

	event ContextMenuEventHandler ContextMenuClosing;

	event ContextMenuEventHandler ContextMenuOpening;

	event DependencyPropertyChangedEventHandler DataContextChanged;

	event EventHandler Initialized;

	event RoutedEventHandler Loaded;

	event RequestBringIntoViewEventHandler RequestBringIntoView;

	event SizeChangedEventHandler SizeChanged;

	event EventHandler<DataTransferEventArgs> SourceUpdated;

	event EventHandler<DataTransferEventArgs> TargetUpdated;

	event ToolTipEventHandler ToolTipClosing;

	event ToolTipEventHandler ToolTipOpening;

	event RoutedEventHandler Unloaded;

	event MouseButtonEventHandler MouseDoubleClick;

	event MouseButtonEventHandler PreviewMouseDoubleClick;

	bool Init(Action<UserControl> endView, WiCAM.Pn4000.PN3D.Assembly.Assembly? assembly, IImportArg importSetting, Action<F2exeReturnCode> answer);

	void ScrollIntoView(object element);

	void RefreshFilter();

	void GridView_PreparedCellForEdit(object sender, GridViewPreparingCellForEditEventArgs e);

	new bool Equals(object obj);

	new int GetHashCode();

	new string ToString();

	bool CheckAccess();

	void VerifyAccess();

	void ClearValue(DependencyProperty dp);

	void ClearValue(DependencyPropertyKey key);

	void CoerceValue(DependencyProperty dp);

	LocalValueEnumerator GetLocalValueEnumerator();

	object GetValue(DependencyProperty dp);

	void InvalidateProperty(DependencyProperty dp);

	object ReadLocalValue(DependencyProperty dp);

	void SetCurrentValue(DependencyProperty dp, object value);

	void SetValue(DependencyProperty dp, object value);

	void SetValue(DependencyPropertyKey key, object value);

	DependencyObject FindCommonVisualAncestor(DependencyObject otherVisual);

	bool IsAncestorOf(DependencyObject descendant);

	bool IsDescendantOf(DependencyObject ancestor);

	Point PointFromScreen(Point point);

	Point PointToScreen(Point point);

	GeneralTransform2DTo3D TransformToAncestor(Visual3D ancestor);

	GeneralTransform TransformToAncestor(Visual ancestor);

	GeneralTransform TransformToDescendant(Visual descendant);

	GeneralTransform TransformToVisual(Visual visual);

	void AddHandler(RoutedEvent routedEvent, Delegate handler);

	void AddHandler(RoutedEvent routedEvent, Delegate handler, bool handledEventsToo);

	void AddToEventRoute(EventRoute route, RoutedEventArgs e);

	void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock);

	void ApplyAnimationClock(DependencyProperty dp, AnimationClock clock, HandoffBehavior handoffBehavior);

	void Arrange(Rect finalRect);

	void BeginAnimation(DependencyProperty dp, AnimationTimeline animation);

	void BeginAnimation(DependencyProperty dp, AnimationTimeline animation, HandoffBehavior handoffBehavior);

	bool CaptureMouse();

	bool CaptureStylus();

	bool CaptureTouch(TouchDevice touchDevice);

	bool Focus();

	object GetAnimationBaseValue(DependencyProperty dp);

	IInputElement InputHitTest(Point point);

	void InvalidateArrange();

	void InvalidateMeasure();

	void InvalidateVisual();

	void Measure(Size availableSize);

	bool MoveFocus(TraversalRequest request);

	DependencyObject PredictFocus(FocusNavigationDirection direction);

	void RaiseEvent(RoutedEventArgs e);

	void ReleaseAllTouchCaptures();

	void ReleaseMouseCapture();

	void ReleaseStylusCapture();

	bool ReleaseTouchCapture(TouchDevice touchDevice);

	void RemoveHandler(RoutedEvent routedEvent, Delegate handler);

	bool ShouldSerializeCommandBindings();

	bool ShouldSerializeInputBindings();

	Point TranslatePoint(Point point, UIElement relativeTo);

	void UpdateLayout();

	bool ApplyTemplate();

	void BeginInit();

	void BeginStoryboard(Storyboard storyboard);

	void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior);

	void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable);

	void BringIntoView();

	void BringIntoView(Rect targetRectangle);

	void EndInit();

	object FindName(string name);

	object FindResource(object resourceKey);

	BindingExpression GetBindingExpression(DependencyProperty dp);

	void OnApplyTemplate();

	void RegisterName(string name, object scopedElement);

	BindingExpression SetBinding(DependencyProperty dp, string path);

	BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding);

	void SetResourceReference(DependencyProperty dp, object name);

	bool ShouldSerializeResources();

	bool ShouldSerializeStyle();

	bool ShouldSerializeTriggers();

	object TryFindResource(object resourceKey);

	void UnregisterName(string name);

	void UpdateDefaultStyle();

	bool ShouldSerializeContent();
}

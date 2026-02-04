using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.DocManagement;

internal class DocumentManagerViewModel : ViewModelBase, IDocumentManagerViewModel
{
	private class DocWrapper : ViewModelBase, IDocWrapper
	{
		private static Brush _borderDefault = new SolidColorBrush(Color.FromRgb(16, 110, 190));

		private static Brush _borderSelected = Brushes.LightSkyBlue;

		private bool _isSelected;

		private ICalculationArg? _currentCalcArg;

		public IDoc3d Document { get; }

		public string Header => Document?.DiskFile.Header.ModelName ?? "-";

		public string Tooltip
		{
			get
			{
				string header = Header;
				string text = "";
				ICalculationArg currentCalcArg = _currentCalcArg;
				if (currentCalcArg != null)
				{
					double? progress = currentCalcArg.Progress;
					text = ((!progress.HasValue) ? (Environment.NewLine + "...") : (Environment.NewLine + progress.Value + "%"));
				}
				return header + text;
			}
		}

		public Visibility BusyVisibility
		{
			get
			{
				if (_currentCalcArg == null)
				{
					return Visibility.Collapsed;
				}
				return Visibility.Visible;
			}
		}

		public Brush BorderSelected { get; private set; }

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					BorderSelected = (_isSelected ? _borderSelected : _borderDefault);
					NotifyPropertyChanged("BorderSelected");
				}
			}
		}

		public event Action<IDocWrapper> Closed;

		public DocWrapper(IDoc3d doc)
		{
			doc.Factorio.Resolve<ICurrentCalculation>().CurrentCalculationChanged += CurrentCalculation_CurrentCalculationChanged;
			Document = doc;
			doc.Closed += Doc_Closed;
		}

		private void CurrentCalculation_CurrentCalculationChanged(ICalculationArg? obj)
		{
			_currentCalcArg = obj;
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				NotifyPropertyChanged("Tooltip");
				NotifyPropertyChanged("BusyVisibility");
			});
		}

		private void Doc_Closed(IPnBndDoc obj)
		{
			this.Closed?.Invoke(this);
		}
	}

	private readonly IDoc3dFactory _doc3dFactory;

	private readonly ICurrentDocProvider _currentDocProvider;

	private IDocWrapper? _emptyDoc;

	private Visibility _visibility;

	private IDocWrapper _selectedDoc;

	public ICommand CmdTileView { get; }

	private IDocWrapper EmptyDoc => _emptyDoc ?? (_emptyDoc = new DocWrapper(_doc3dFactory.EmptyDoc));

	public Visibility Visibility
	{
		get
		{
			return _visibility;
		}
		set
		{
			if (_visibility != value)
			{
				_visibility = value;
				NotifyPropertyChanged("Visibility");
			}
		}
	}

	public RadObservableCollection<IDocWrapper> Documents { get; } = new RadObservableCollection<IDocWrapper>();

	public IDocWrapper? SelectedDoc
	{
		get
		{
			if (_selectedDoc == EmptyDoc)
			{
				return null;
			}
			return _selectedDoc;
		}
		set
		{
			if (value == null)
			{
				value = EmptyDoc;
			}
			if (_selectedDoc != value)
			{
				if (_selectedDoc != null)
				{
					_selectedDoc.IsSelected = false;
				}
				_selectedDoc = value;
				_currentDocProvider.CurrentDoc = value.Document;
				NotifyPropertyChanged("SelectedDoc");
				if (_selectedDoc != null)
				{
					_selectedDoc.IsSelected = true;
				}
			}
		}
	}

	public DocumentManagerViewModel(IDoc3dFactory doc3dFactory, IDocManager docManager, ICurrentDocProvider currentDocProvider)
	{
		_doc3dFactory = doc3dFactory;
		_currentDocProvider = currentDocProvider;
		currentDocProvider.CurrentDocChanged += CurrentDocProvider_CurrentDocChanged;
		Documents.CollectionChanged += Documents_CollectionChanged;
	}

	private void Documents_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
	}

	private void CurrentDocProvider_CurrentDocChanged(IDoc3d docOld, IDoc3d docNew)
	{
		if (docNew == EmptyDoc.Document)
		{
			SelectedDoc = EmptyDoc;
		}
		else
		{
			IDocWrapper docWrapper = Documents.FirstOrDefault((IDocWrapper x) => x.Document == docNew);
			if (docWrapper == null)
			{
				DocWrapper docWrapper2 = new DocWrapper(docNew);
				docWrapper2.Closed += WrapperNew_Closed;
				docWrapper = docWrapper2;
				Documents.Add(docWrapper);
			}
			SelectedDoc = docWrapper;
		}
		NotifyPropertyChanged("SelectedDoc");
	}

	private void WrapperNew_Closed(IDocWrapper docWrapper)
	{
		Documents.Remove(docWrapper);
	}
}

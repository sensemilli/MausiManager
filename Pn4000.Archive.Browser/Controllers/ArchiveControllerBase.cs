using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Database;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.WpfControls;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.Archive.Browser.Controllers
{
	internal abstract class ArchiveControllerBase : DependencyObject
	{
		public static Action OkClickHandler;

		public Action<FilterConfiguration> DatabaseFilterChangedHandler;

		protected bool _isShiftPressed;

		protected bool _isCtrlPressed;

		protected int _firstSelectedIndex;

		protected int _lastSelectedIndex;

		protected MainWindow _mainWindow;

		protected DataGrid _grid;

		protected FilterCriteria _archiveFilterCriteria;

		protected WpfControls.FilterControl _filterControl;

		protected bool _isDatabaseActive;

		private List<PropertyInfo> _properties;

		private readonly static DateTime _minDate;

		static ArchiveControllerBase()
		{
			ArchiveControllerBase._minDate = new DateTime(2000, 1, 1);
		}

		public ArchiveControllerBase()
		{
		}

		protected void ButtokOkClick()
		{
			Action okClickHandler = ArchiveControllerBase.OkClickHandler;
			if (okClickHandler == null)
			{
				return;
			}
			okClickHandler();
		}

		private string ConvertDate(string dateString)
		{
			if (string.IsNullOrWhiteSpace(dateString) || dateString.Length <= 1)
			{
				return " ";
			}
			if (dateString[0] == ' ')
			{
				return dateString;
			}
			DateTime dateTime = StringHelper.ToDateTime(dateString);
			if (dateTime < ArchiveControllerBase._minDate)
			{
				dateTime = ArchiveControllerBase._minDate;
			}
			return dateTime.ToString(" yyyy.MM.dd");
		}

		private void DbFilterChange_Click(object sender, RoutedEventArgs e)
		{
			MenuItem menuItem = sender as MenuItem;
			if (menuItem != null)
			{
				FilterConfiguration header = menuItem.Header as FilterConfiguration;
				if (header != null)
				{
					PnPathBuilder.ChangeArDrive(header.ArDrivePath);
					WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = 0;
					if (this.DatabaseFilterChangedHandler != null)
					{
						this.DatabaseFilterChangedHandler(header);
					}
				}
			}
		}

		protected DependencyObject GetDependencyObject(DependencyObject dep, Type objectType)
		{
			while (dep != null && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
			{
				dep = VisualTreeHelper.GetParent(dep);
			}
			if (dep == null)
			{
				return null;
			}
			if (dep.GetType() == objectType)
			{
				return dep;
			}
			return null;
		}

		protected void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this._grid.SelectedItems.Count > 0)
			{
				this.ButtokOkClick();
			}
		}

		protected void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				e.Handled = true;
			}
		}

		protected void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return && this._grid.SelectedItems.Count > 0)
			{
				this.ButtokOkClick();
			}
		}

		protected void InitializeBase(MainWindow mainWindow, Type itemType, WpfCaptionDictionary captions, string filterSettingsName)
		{
			this._mainWindow = mainWindow;
			this._grid = this._mainWindow.GrdArchiveData;
			this._filterControl = this._mainWindow.FilterControl1;
			MainWindow mainWindow1 = this._mainWindow;
			ApplicationVersionHelper applicationVersionHelper = new ApplicationVersionHelper();
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			StringResourceHelper instance = StringResourceHelper.Instance;
			mainWindow1.Title = applicationVersionHelper.FindHeaderString(executingAssembly, new Func<string, string>(instance.FindString), this._isDatabaseActive);
			this._archiveFilterCriteria = (new FilterCriteria("WiCAM.ArchivBrowser", filterSettingsName)).ReadConfiguration();
			this._archiveFilterCriteria.DateFromLabel = StringResourceHelper.Instance.FindString(WiCAM.Pn4000.Archive.Browser.Classes.CS.From);
			this._archiveFilterCriteria.DateTillLabel = StringResourceHelper.Instance.FindString(WiCAM.Pn4000.Archive.Browser.Classes.CS.Till);
		}

		protected void InitializeFilters(Type itemType)
		{
			this._filterControl.InitializeControl(itemType, AppConfiguration.Instance.ListConfiguration);
			int num = 0;
			foreach (CppConfigurationLineInfo listConfiguration in AppConfiguration.Instance.ListConfiguration)
			{
				DataGridColumn boundColumn = listConfiguration.BoundColumn;
				if (boundColumn == null)
				{
					continue;
				}
				string path = ((Binding)((DataGridBoundColumn)boundColumn).Binding).Path.Path;
				if (!this._filterControl.FieldNames.ContainsKey(path) || boundColumn.Visibility != Visibility.Visible)
				{
					continue;
				}
				if (num < this._archiveFilterCriteria.ListFilters.Count && this._archiveFilterCriteria.ListFilters[num].FieldName == null)
				{
					this._archiveFilterCriteria.ListFilters[num].FieldName = path;
				}
				num++;
			}
			this._filterControl.DataFilterCriteria = this._archiveFilterCriteria;
		}

		protected void InitializeFiltersToolbar(string connectionString)
		{
			_mainWindow = MainWindow.Instance;
			DatabaseFilterHelper.Instance.ShowSqlCommand = true;
			if (!string.IsNullOrEmpty(connectionString))
			{
				DatabaseFilterHelper.Instance.InitializeFilters(connectionString);
				this.InitializeMainWindowToolbar(DatabaseFilterHelper.Instance.DatabaseFilters);
			}
			this._mainWindow.DataContext = this;
			this._mainWindow.FilterControl1.SetFocusOnFirstTextBox();
		}

		private void InitializeMainWindowToolbar(List<FilterConfiguration> filtersList)
		{
			if (!AppArguments.Instance.IsMultiselect)
			{
				if (EnumerableHelper.IsNullOrEmpty(filtersList) || filtersList.Count <= 1)
				{
					//this._mainWindow.MnuFilterSwitch.Visibility = Visibility.Hidden;
				}
				else
				{
					int num = 0;
					foreach (FilterConfiguration filterConfiguration in filtersList)
					{
						MenuItem menuItem = new MenuItem();
						int num1 = num;
						num = num1 + 1;
						menuItem.Name = string.Format(CultureInfo.CurrentCulture, "mnuSwitch{0}", num1);
						menuItem.Header = filterConfiguration;
						menuItem.HeaderTemplate = this._mainWindow.FindResource("filterHeaderTemplate") as DataTemplate;
						menuItem.Icon = filterConfiguration.FilterKey;
						menuItem.Height = 25;
						menuItem.Click += new RoutedEventHandler(this.DbFilterChange_Click);
					//	this._mainWindow.MnuFilterSwitch.Items.Add(menuItem);
					}
				}
			}
		}

		protected void LoadCadgeoPreview(string path, string archivName, string itemName, double dimensionX, double dimensionY)
		{
			Cursor cursor = this._mainWindow.Cursor;
			this._mainWindow.Cursor = Cursors.Wait;
			bool flag = this._mainWindow.CadgeoViewer1.LoadNestingCadgeo(path, this._mainWindow.CadgeoViewer1.ActualWidth, this._mainWindow.CadgeoViewer1.ActualHeight, dimensionX, dimensionY, true);
			this._mainWindow.Cursor = cursor;
			this.UpdateStatusText(flag, itemName, archivName);
		}

		protected void ModifyDateValues(NcPartInfo part)
		{
			/*
			if (this._properties == null)
			{
				this._properties = new List<PropertyInfo>();
				foreach (CppConfigurationLineInfo listConfiguration in AppConfiguration.Instance.ListConfiguration)
				{
					if (listConfiguration.Visibility != 3 || !(listConfiguration.Property.PropertyType == typeof(string)))
					{
						continue;
					}
					this._properties.Add(listConfiguration.Property);
				}
			}
			foreach (PropertyInfo _property in this._properties)
			{
				object value = _property.GetValue(part, null);
				if (value == null)
				{
					continue;
				}
				_property.SetValue(part, this.ConvertDate(value.ToString()));
			}
			*/
		}

		protected void UpdateStatusText(bool cadgeoIsLoaded, string itemName, string archivName)
		{
			this._mainWindow.StatusText2.Text = string.Empty;
			this._mainWindow.StatusImage2.Source = null;
			if (cadgeoIsLoaded)
			{
				this._mainWindow.StatusImage2.Source = ((Image)this._mainWindow.Resources["ImageFolder"]).Source;
				this._mainWindow.StatusText2.Text = archivName;
				return;
			}
			this._mainWindow.StatusImage2.Source = ((Image)this._mainWindow.Resources["ImageWarning"]).Source;
			this._mainWindow.StatusText2.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("ErrorCadgeoNotFound"), itemName);
		}
	}
}
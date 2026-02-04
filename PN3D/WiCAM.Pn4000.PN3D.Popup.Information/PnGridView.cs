using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Controls;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class PnGridView : GridView
{
	private string _isolatedStoredFilename;

	private List<ColumnInfo> _desiredColumnInfo;

	public bool IsInitialized { get; set; }

	public string IsolatedStoredFilename
	{
		get
		{
			return this._isolatedStoredFilename;
		}
		set
		{
			this._isolatedStoredFilename = value;
		}
	}

	public PnGridView()
	{
		base.Columns.CollectionChanged += Columns_CollectionChanged;
	}

	~PnGridView()
	{
		base.Columns.CollectionChanged -= Columns_CollectionChanged;
		this.UnhookColumnWidthsChangedEvents();
	}

	public void HookColumnWidthsChangedEvents()
	{
		foreach (GridViewColumn column in base.Columns)
		{
			((INotifyPropertyChanged)column).PropertyChanged += delegate(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == "Width")
				{
					this.SaveColumnOrder();
				}
			};
		}
	}

	public void SetColumnOrder()
	{
		List<GridViewColumn> list = base.Columns.ToList();
		try
		{
			this.ReadColumnOrder();
			if (this._desiredColumnInfo == null)
			{
				base.Columns.CollectionChanged += Columns_CollectionChanged;
				return;
			}
			base.Columns.CollectionChanged -= Columns_CollectionChanged;
			base.Columns.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				GridViewColumn gridViewColumn = list[i];
				string columnHeader = ((gridViewColumn.Header.GetType() == typeof(TextBlock)) ? ((TextBlock)gridViewColumn.Header).Text : gridViewColumn.Header.ToString());
				ColumnInfo info = this._desiredColumnInfo.FirstOrDefault((ColumnInfo x) => x.Header.ToString() == columnHeader);
				if (info != null)
				{
					base.Columns.Add(list.First((GridViewColumn x) => (!(x.Header.GetType() == typeof(TextBlock))) ? (x.Header.ToString() == info.Header) : (((TextBlock)x.Header).Text == info.Header)));
					base.Columns[i].Width = info.Width;
				}
				else
				{
					base.Columns.Add(gridViewColumn);
				}
			}
			base.Columns.CollectionChanged += Columns_CollectionChanged;
		}
		catch (Exception ex)
		{
			Console.Write($"Could not set column order : {ex.Message}");
			base.Columns.Clear();
			foreach (GridViewColumn item in list)
			{
				base.Columns.Add(item);
			}
		}
	}

	private void ReadColumnOrder()
	{
		IsolatedStorageFile store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
		if (store.GetFileNames(this._isolatedStoredFilename).Length != 1)
		{
			return;
		}
		try
		{
			StreamReader streamReader = new StreamReader(new IsolatedStorageFileStream(string.Format(this._isolatedStoredFilename, default(ReadOnlySpan<object>)), FileMode.Open, store));
			string[] source = streamReader.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			streamReader.Close();
			this._desiredColumnInfo = source.Select((string s) => new ColumnInfo
			{
				Header = s.Split(',')[0],
				NewIndex = Convert.ToInt32(s.Split(',')[1]),
				Width = Convert.ToDouble(s.Split(',')[2])
			}).ToList();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error reading {this._isolatedStoredFilename} : {ex.Message}");
		}
	}

	private void SaveColumnOrder()
	{
		StreamWriter streamWriter = new StreamWriter(new IsolatedStorageFileStream(isf: IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null), path: string.Format(this._isolatedStoredFilename, default(ReadOnlySpan<object>)), mode: FileMode.Create));
		foreach (GridViewColumn column in base.Columns)
		{
			string arg = column.Header.ToString();
			if (column.Header.GetType() == typeof(TextBlock))
			{
				arg = ((TextBlock)column.Header).Text;
			}
			int num = base.Columns.IndexOf(column);
			streamWriter.WriteLine(string.Format(arg2: column.ActualWidth, format: "{0},{1},{2}", arg0: arg, arg1: num));
		}
		streamWriter.Close();
	}

	private void UnhookColumnWidthsChangedEvents()
	{
		foreach (GridViewColumn column in base.Columns)
		{
			((INotifyPropertyChanged)column).PropertyChanged -= delegate(object? sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == "Width")
				{
					this.SaveColumnOrder();
				}
			};
		}
	}

	private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (this.IsInitialized)
		{
			this.SaveColumnOrder();
		}
	}
}

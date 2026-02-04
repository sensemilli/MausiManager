using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupEditDeductionValueModel
{
	private readonly IPnPathService _pnPathService;

	public Dictionary<IBendDescriptor, BendTableReturnValues> BendTableResults { get; }

	public global::WiCAM.Pn4000.BendModel.Model Model { get; }

	public IDoc3d Doc { get; }

	public General3DConfig Config { get; }

	public IBendTable BendTable { get; }

	public IMaterialManager Materials { get; }

	public ToolTip KfTooltip { get; }

	public ToolTip BdTooltip { get; }

	public ToolTip DinToolTip { get; }

	public ToolTip KFactorAlgorithmToolTip { get; }

	public ToolTip ToolSelectionAlgorithmToolTip { get; }

	public double DefaultKFactor => this.Doc?.BendMachine?.UnfoldConfig?.DefaultKFactor ?? this.Config.P3D_Default_KFactor;

	public PopupEditDeductionValueModel(Dictionary<IBendDescriptor, BendTableReturnValues> bendTableResults, IDoc3d doc, General3DConfig config, IBendTable bendTable, IMaterialManager materials, IPnPathService pnPathService)
	{
		this._pnPathService = pnPathService;
		this.BendTableResults = bendTableResults;
		this.Doc = doc;
		this.Model = this.Doc.EntryModel3D;
		this.Config = config;
		this.BendTable = bendTable;
		this.Materials = materials;
		this.KfTooltip = this.GetToolTipImage("BDB_KFactor.png");
		this.BdTooltip = this.GetToolTipImage("BDB_BD.png");
		this.DinToolTip = this.GetToolTipImage("BDB_DinLength.png");
		this.KFactorAlgorithmToolTip = this.GetToolTipKFactorAlgorithm();
		this.ToolSelectionAlgorithmToolTip = this.GetToolTipToolSelectionAlgorithm();
	}

	private ToolTip GetToolTipImage(string name)
	{
		string pFileImagePath = this._pnPathService.GetPFileImagePath(name);
		if (!File.Exists(pFileImagePath))
		{
			return null;
		}
		try
		{
			ToolTip toolTip = new ToolTip();
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.UriSource = new Uri(pFileImagePath);
			bitmapImage.EndInit();
			toolTip.Content = new Image
			{
				Source = bitmapImage
			};
			return toolTip;
		}
		catch
		{
			return null;
		}
	}

	private ToolTip GetToolTipKFactorAlgorithm()
	{
		try
		{
			ToolTip toolTip = new ToolTip();
			string value = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.DEFAULT_VALUE) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.DEFAULT_VALUE);
			string value2 = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.INTERPOLATED) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.INTERPOLATED);
			string value3 = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.EXACT_VALUE) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.EXACT_VALUE);
			string value4 = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.NO_VALUE_FOUND) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.NO_VALUE_FOUND);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(value3);
			stringBuilder.AppendLine(value2);
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(value4);
			toolTip.Content = stringBuilder.ToString();
			return toolTip;
		}
		catch
		{
			return null;
		}
	}

	private ToolTip GetToolTipToolSelectionAlgorithm()
	{
		try
		{
			ToolTip toolTip = new ToolTip();
			string value = ToolSelectionAlgorithmTranslation.GetTranslation(ToolSelectionType.NoTools) + " : " + ToolSelectionAlgorithmTranslation.GetExplanation(ToolSelectionType.NoTools);
			string value2 = ToolSelectionAlgorithmTranslation.GetTranslation(ToolSelectionType.PreferredTools) + " : " + ToolSelectionAlgorithmTranslation.GetExplanation(ToolSelectionType.PreferredTools);
			string value3 = ToolSelectionAlgorithmTranslation.GetTranslation(ToolSelectionType.SuggestedTools) + " : " + ToolSelectionAlgorithmTranslation.GetExplanation(ToolSelectionType.SuggestedTools);
			string value4 = ToolSelectionAlgorithmTranslation.GetTranslation(ToolSelectionType.UserSelectedTools) + " : " + ToolSelectionAlgorithmTranslation.GetExplanation(ToolSelectionType.UserSelectedTools);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(value2);
			stringBuilder.AppendLine(value3);
			stringBuilder.AppendLine(value4);
			toolTip.Content = stringBuilder.ToString();
			return toolTip;
		}
		catch
		{
			return null;
		}
	}
}

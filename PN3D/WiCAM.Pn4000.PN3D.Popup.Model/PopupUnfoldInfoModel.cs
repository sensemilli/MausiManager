using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupUnfoldInfoModel
{
	private readonly IPnPathService _pnPathService;

	public IDoc3d Doc { get; private set; }

	public string FileName => this.Doc.DiskFile.Header.ImportedFilename;

	public Image KfImage { get; }

	public Image BdImage { get; }

	public Image DinImage { get; }

	public ToolTip KFactorAlgorithmToolTip { get; }

	public ToolTip ToolSelectionAlgorithmToolTip { get; }

	public PopupUnfoldInfoModel(IPnPathService pnPathService)
	{
		this._pnPathService = pnPathService;
		this.KfImage = this.GetToolTipImage("BDB_KFactor.png");
		this.BdImage = this.GetToolTipImage("BDB_BD.png");
		this.DinImage = this.GetToolTipImage("BDB_DinLength.png");
		this.KFactorAlgorithmToolTip = this.GetToolTipKFactorAlgorithm();
		this.ToolSelectionAlgorithmToolTip = this.GetToolTipToolSelectionAlgorithm();
	}

	public void Init(IDoc3d doc)
	{
		this.Doc = doc;
	}

	private Image GetToolTipImage(string name)
	{
		string pFileImagePath = this._pnPathService.GetPFileImagePath(name);
		if (!File.Exists(pFileImagePath))
		{
			return null;
		}
		try
		{
			new ToolTip();
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.UriSource = new Uri(pFileImagePath);
			bitmapImage.EndInit();
			return new Image
			{
				Source = bitmapImage
			};
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
			string value4 = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.NEAREST_RADIUS) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.NEAREST_RADIUS);
			string value5 = KFactorAlgorithmTranslation.GetTranslation(BendTableReturnValues.NO_VALUE_FOUND) + " : " + KFactorAlgorithmTranslation.GetExplanation(BendTableReturnValues.NO_VALUE_FOUND);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(value3);
			stringBuilder.AppendLine(value4);
			stringBuilder.AppendLine(value2);
			stringBuilder.AppendLine(value);
			stringBuilder.AppendLine(value5);
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

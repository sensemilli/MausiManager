using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.Model;

public class ContextMenuModel
{
	private readonly IConfigProvider _configProvider;

	public ICombinedBendDescriptorInternal SelectedBend { get; }

	public IDoc3d Doc { get; }

	public ContextMenuModel(IDoc3d doc, ICombinedBendDescriptorInternal selectedBend, IConfigProvider configProvider)
	{
		this._configProvider = configProvider;
		this.Doc = doc;
		this.SelectedBend = selectedBend;
	}

	public bool ChangeRadius(IEnumerable<ICombinedBendDescriptorInternal> cbfs, double newRadius, IGlobals globals)
	{
		bool result = false;
		foreach (ICombinedBendDescriptorInternal cbf in cbfs)
		{
			if (!(Math.Abs(cbf[0].BendParams.FinalRadius - newRadius) < 1E-06) && this.Doc.ChangeManualRadius(this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf), newRadius))
			{
				result = true;
			}
		}
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (general3DConfig.P3D_ObjExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToObj(this.Doc, globals);
		}
		if (general3DConfig.P3D_GlbExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToGlb(this.Doc, globals);
		}
		return result;
	}

	public bool ChangeRadius(ICombinedBendDescriptorInternal cbf, double newRadius, IGlobals globals)
	{
		if (Math.Abs(cbf[0].BendParams.FinalRadius - newRadius) < 1E-06)
		{
			return false;
		}
		bool result = this.Doc.ChangeManualRadius(this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf), newRadius);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (general3DConfig.P3D_ObjExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToObj(this.Doc, globals);
		}
		if (general3DConfig.P3D_GlbExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToGlb(this.Doc, globals);
		}
		return result;
	}

	public bool ChangeDeduction(ICombinedBendDescriptorInternal cbf, double newDeduction, IGlobals globals)
	{
		if (Math.Abs(cbf[0].BendParams.FinalBendDeduction - newDeduction) < 1E-06)
		{
			return false;
		}
		bool result = this.Doc.ChangeManualBendDeduction(this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(cbf), newDeduction);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (general3DConfig.P3D_ObjExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToObj(this.Doc, globals);
		}
		if (general3DConfig.P3D_GlbExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToGlb(this.Doc, globals);
		}
		return result;
	}

	public bool ChangeDeduction(List<ICombinedBendDescriptorInternal> cbf, double newDeduction, IGlobals globals)
	{
		if (cbf.Any((ICombinedBendDescriptorInternal c) => Math.Abs(c[0].BendParams.FinalBendDeduction - newDeduction) < 1E-06))
		{
			return false;
		}
		List<int> bendIndex = cbf.Select((ICombinedBendDescriptorInternal c) => this.Doc.CombinedBendDescriptors.IndexOf<ICombinedBendDescriptorInternal>(c)).ToList();
		bool result = this.Doc.ChangeManualBendDeduction(bendIndex, newDeduction);
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		if (general3DConfig.P3D_ObjExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToObj(this.Doc, globals);
		}
		if (general3DConfig.P3D_GlbExportModeModifiedModel)
		{
			Unfold.WriteModifiedModelToGlb(this.Doc, globals);
		}
		return result;
	}
}

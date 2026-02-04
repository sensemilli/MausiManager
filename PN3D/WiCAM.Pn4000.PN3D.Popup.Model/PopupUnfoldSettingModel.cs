using System.Collections.ObjectModel;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.Model;

public class PopupUnfoldSettingModel : PopupModelBase
{
	public General3DConfig General3DConfig { get; }

	public SimValidationConfig SimValidationConfig { get; }

	public Macro3DConfig Macro3DConfig { get; }

	public ModelColors3DConfig ModelColors3DConfig { get; }

	public ValidationSettingsConfig ValidationGeometryConfig { get; set; }

	public AnalyzeConfig AnalyzeConfig { get; }

	public UserCommentsConfig UserCommentsConfig { get; }

	public ReconstructIrregularBendsConfig ReconstructBendsConfig { get; }

	public ObservableCollection<IBendMachineSummary> Machines { get; set; }

	public PopupUnfoldSettingModel(IConfigProvider configProvider)
		: base(configProvider)
	{
		this.General3DConfig = configProvider.InjectOrCreate<General3DConfig>();
		this.Macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		this.ModelColors3DConfig = configProvider.InjectOrCreate<ModelColors3DConfig>();
		this.ValidationGeometryConfig = configProvider.InjectOrCreate<ValidationSettingsConfig>();
		this.AnalyzeConfig = configProvider.InjectOrCreate<AnalyzeConfig>();
		this.SimValidationConfig = configProvider.InjectOrCreate<SimValidationConfig>();
		this.UserCommentsConfig = configProvider.InjectOrCreate<UserCommentsConfig>();
		this.ReconstructBendsConfig = configProvider.InjectOrCreate<ReconstructIrregularBendsConfig>();
	}

	public override void PushToConfigProvider()
	{
		base.PushToConfigProvider(this.General3DConfig);
		base.PushToConfigProvider(this.Macro3DConfig);
		base.PushToConfigProvider(this.ModelColors3DConfig);
		base.PushToConfigProvider(this.ValidationGeometryConfig);
		base.PushToConfigProvider(this.AnalyzeConfig);
		base.PushToConfigProvider(this.SimValidationConfig);
		base.PushToConfigProvider(this.UserCommentsConfig);
		base.PushToConfigProvider(this.ReconstructBendsConfig);
	}
}

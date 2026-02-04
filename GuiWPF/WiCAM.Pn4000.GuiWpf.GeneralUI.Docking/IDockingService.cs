using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.DependencyInjection;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public interface IDockingService
{
	void Initialize(RadDocking docking);

	void RegisterSingletonView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null);

	void RegisterScopedView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null);

	void RegisterTransientView<TView, TViewModel>(TmpStartPos? startPos = null, string? titleKey = null);

	void ChangeScope(IScopedFactorio factorio);

	void ChangeContext(object? context);

	void Show<TViewModel>(TViewModel? viewModel = null, IScopedFactorio? factorio = null) where TViewModel : class;

	void Hide(object? viewModel);

	void HideAll();

	void ShowIfExists(object? viewModel);

	void RegisterViewModelOnly<TViewModel>();
}

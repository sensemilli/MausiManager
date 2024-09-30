using System.Collections.Generic;

namespace WiCAM.Pn4000.JobManager;

public class ModelViewManager
{
	private List<IViewModel> _registered = new List<IViewModel>();

	private List<IDialogViewModel> _registeredDialogs = new List<IDialogViewModel>();

	public IViewModel Register<TView, TViewModel>(IJobManagerServiceProvider provider) where TView : class, IView, new() where TViewModel : class, IViewModel, new()
	{
		IView view = new TView();
		IViewModel viewModel = new TViewModel();
		viewModel.Initialize(view, provider);
		view.DataContext(viewModel);
		_registered.Add(viewModel);
		return viewModel;
	}

	public IViewModel Register<TView, TViewModel>(TView view, IJobManagerServiceProvider provider) where TView : class, IView, new() where TViewModel : class, IViewModel, new()
	{
		IViewModel viewModel = new TViewModel();
		viewModel.Initialize(view, provider);
		view.DataContext(viewModel);
		_registered.Add(viewModel);
		return viewModel;
	}

	public IDialogViewModel RegisterDialog<TView, TViewModel>(IJobManagerServiceProvider provider) where TView : class, IDialogView, new() where TViewModel : class, IDialogViewModel, new()
	{
		IDialogView dialogView = new TView();
		IDialogViewModel dialogViewModel = new TViewModel();
		dialogViewModel.Initialize(dialogView, provider);
		dialogView.DataContext(dialogViewModel);
		_registeredDialogs.Add(dialogViewModel);
		return dialogViewModel;
	}
}

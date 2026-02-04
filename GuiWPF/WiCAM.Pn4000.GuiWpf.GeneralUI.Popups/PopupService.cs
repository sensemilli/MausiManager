using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.GuiContracts.Popups;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Popups;

internal class PopupService : IPopupService
{
	private class Registration
	{
		public Action<object, object>? ShowViewViewModel;

		public Action<object, object>? ShowDialogViewViewModel;

		public Type ViewModel { get; set; }

		public Type View { get; set; }

		public Registration(Type viewModel, Type view)
		{
			ViewModel = viewModel;
			View = view;
		}
	}

	private readonly IFactorio _factorio;

	private Dictionary<Type, Registration> _registeredVm = new Dictionary<Type, Registration>();

	public PopupService(IFactorio factorio)
	{
		_factorio = factorio;
	}

	public void RegisterPopup<TView, TViewModel>() where TViewModel : IPopupViewModel
	{
		_registeredVm.Add(typeof(TViewModel), new Registration(typeof(TViewModel), typeof(TView)));
	}

	public void RegisterPopup<TView, TViewModel>(Action<object, object> showViewViewModel) where TView : class where TViewModel : class, IPopupViewModel
	{
		Registration registration = new Registration(typeof(TViewModel), typeof(TView));
		registration.ShowViewViewModel = showViewViewModel;
		_registeredVm.Add(typeof(TViewModel), registration);
	}

	public void Show<TViewModel>(IScopedFactorio? factorio, TViewModel vm, out object? view) where TViewModel : IPopupViewModel
	{
		IFactorio factory = factorio;
		if (factory == null)
		{
			factory = _factorio;
		}
		if (_registeredVm.TryGetValue(typeof(TViewModel), out Registration registration))
		{
			object v = null;
			Application.Current.Dispatcher.Invoke(delegate
			{
				v = factory.Resolve(registration.View);
				if (v is IPopupView popupView)
				{
					popupView.Init(vm);
				}
				if (registration.ShowViewViewModel == null)
				{
					Window window = new Window();
					window.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
					window.Closing += ((IPopupViewModel)vm).ViewClosing;
					ref TViewModel reference = ref vm;
					TViewModel val = default(TViewModel);
					if (val == null)
					{
						val = reference;
						reference = val;
					}
					reference.CloseView += window.Close;
					window.Content = v;
					window.ShowActivated = true;
					window.ShowInTaskbar = true;
					window.Show();
					window.Closing -= ((IPopupViewModel)vm).ViewClosing;
					ref TViewModel reference2 = ref vm;
					val = default(TViewModel);
					if (val == null)
					{
						val = reference2;
						reference2 = val;
					}
					reference2.CloseView -= window.Close;
				}
				else
				{
					registration.ShowViewViewModel(v, vm);
				}
			});
			view = v;
			return;
		}
		throw new Exception("no registration for popup '" + typeof(TViewModel).Name + "' found.");
	}

	public TViewModel ShowDialog<TViewModel>(Action<TViewModel>? init = null) where TViewModel : IPopupViewModel
	{
		if (_registeredVm.TryGetValue(typeof(TViewModel), out Registration registration))
		{
			TViewModel vm = _factorio.Resolve<TViewModel>();
			Application.Current.Dispatcher.Invoke(delegate
			{
				init?.Invoke(vm);
				object obj = _factorio.Resolve(registration.View);
				if (obj is IPopupView popupView)
				{
					popupView.Init(vm);
				}
				if (registration.ShowDialogViewViewModel == null)
				{
					Window window = new Window();
					window.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
					window.Closing += ((IPopupViewModel)vm).ViewClosing;
					ref TViewModel reference = ref vm;
					TViewModel val = default(TViewModel);
					if (val == null)
					{
						val = reference;
						reference = val;
					}
					reference.CloseView += window.Close;
					window.Content = obj;
					window.ShowActivated = true;
					window.ShowInTaskbar = true;
					window.ShowDialog();
					window.Closing -= ((IPopupViewModel)vm).ViewClosing;
					ref TViewModel reference2 = ref vm;
					val = default(TViewModel);
					if (val == null)
					{
						val = reference2;
						reference2 = val;
					}
					reference2.CloseView -= window.Close;
				}
				else
				{
					registration.ShowDialogViewViewModel(obj, vm);
				}
			});
			return vm;
		}
		throw new Exception("no registration for popup '" + typeof(TViewModel).Name + "' found.");
	}
}

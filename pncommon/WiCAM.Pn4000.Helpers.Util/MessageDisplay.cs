using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Services.Loggers.Contracts;

namespace WiCAM.Pn4000.Helpers.Util;

public class MessageDisplay : IMessageDisplay
{
	private static RadDesktopAlertManager _alertManager = new RadDesktopAlertManager();

	private static Image _pnLogo;

	private const int MsgBoxMaxLen = 65535;

	private readonly IWiLogger _logger;

	private ITranslator _translator;

	private readonly IFactorio _factorio;

	private readonly IAutoMode _autoMode;

	public Action<(string caption, string message)>? RedirectMessageBoxesError { get; set; }

	public Action<(string caption, string message)>? RedirectMessageBoxesWarning { get; set; }

	public Action<(string caption, string message)>? RedirectMessageBoxesInfo { get; set; }

	public IWeakEvent<ITranslator, IResourcesChangedArgs> ResourcesChangedWeak => this._translator.ResourcesChangedWeak;

	public event Action<ITranslator, IResourcesChangedArgs> ResourcesChangedStrong
	{
		add
		{
			this._translator.ResourcesChangedStrong += value;
		}
		remove
		{
			this._translator.ResourcesChangedStrong -= value;
		}
	}

	public MessageDisplay(IWiLogger logger, ITranslator translator, IFactorio factorio)
	{
		this._logger = logger;
		this._translator = translator;
		this._factorio = factorio;
		this._autoMode = this._factorio.Resolve<IAutoMode>();
	}

	private string Translate(string msgKey, params object[] parameters)
	{
		return this._translator.Translate(msgKey, parameters);
	}

	[Obsolete("Please use Function of IMessageDisplay")]
	public static string Translate(string msgKey)
	{
		return (string)Application.Current.FindResource(msgKey);
	}

	public void ShowErrorMessage(Exception exception)
	{
		this.ShowErrorMessage(exception.Message + Environment.NewLine + exception.StackTrace);
		this._logger.Error(exception);
	}

	public void ShowErrorMessage(Exception exception, string caption)
	{
		this.ShowErrorMessage(exception.Message + Environment.NewLine + exception.StackTrace, caption);
		this._logger.Error(exception);
	}

	public void ShowTranslatedErrorMessage(string messageKey, params object[] parameters)
	{
		this.ShowErrorMessage(this.Translate(messageKey, parameters));
	}

	public void ShowTranslatedErrorMessages(List<string> messageKey, params object[] parameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in messageKey)
		{
			string[] array = item.Split(' ');
			StringBuilder stringBuilder2 = new StringBuilder();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				stringBuilder2.Append(this.Translate(array2[i], parameters)).Append(' ');
			}
			stringBuilder.AppendLine(stringBuilder2.ToString().TrimEnd() + Environment.NewLine);
		}
		this.ShowErrorMessage(stringBuilder.ToString());
	}

	public void ShowTranslatedWarningMessage(string messageKey, params object[] parameters)
	{
		this.ShowWarningMessage(this.Translate(messageKey, parameters));
	}

	public void ShowTranslatedInformationMessage(string messageKey, params object[] parameters)
	{
		this.ShowInformationMessage(this.Translate(messageKey, parameters));
	}

	private string TranslateInternal(string msgKey)
	{
		return (string)Application.Current.FindResource(msgKey);
	}

	public void ShowErrorMessage(string message, string caption = null)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (caption == null)
			{
				caption = (string)Application.Current.FindResource("General.Error");
			}
			if (this._autoMode.PopupsEnabled)
			{
				string text = null;
				if (message.Length > 65535)
				{
					text = message.Substring(0, 65535);
				}
				if (this.RedirectMessageBoxesError != null)
				{
					this.RedirectMessageBoxesError?.Invoke((caption, text ?? message));
				}
				else
				{
					MessageBox.Show(text ?? message, caption, MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
			this._logger.Error("ApplicationError: " + message);
		});
	}

	public void LogErrorMessage(string message)
	{
		this._logger.Error("AppErrorHidden: " + message);
	}

	public void LogWarningMessage(string message)
	{
		this._logger.Warning("AppWaningHidden: " + message);
	}

	public void ShowInformationMessage(string message, string caption = null)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (caption == null)
			{
				caption = (string)Application.Current.FindResource("General.Information");
			}
			if (this._autoMode.PopupsEnabled)
			{
				string text = null;
				if (message.Length > 65535)
				{
					text = message.Substring(0, 65535);
				}
				if (this.RedirectMessageBoxesInfo != null)
				{
					this.RedirectMessageBoxesInfo?.Invoke((caption, text ?? message));
				}
				else
				{
					MessageBox.Show(text ?? message, caption, MessageBoxButton.OK, MessageBoxImage.Asterisk);
				}
			}
			this._logger.Info("ApplicationInformation: " + message);
		});
	}

	public void ShowWarningMessage(string message, string caption = null, bool notificationStyle = false)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (caption == null)
			{
				caption = (string)Application.Current.FindResource("General.Warning");
			}
			if (this._autoMode.PopupsEnabled)
			{
				string text = null;
				if (message.Length > 65535)
				{
					text = message.Substring(0, 65535);
				}
				if (this.RedirectMessageBoxesWarning != null)
				{
					this.RedirectMessageBoxesWarning?.Invoke((caption, text ?? message));
				}
				else if (notificationStyle)
				{
					if (MessageDisplay._pnLogo == null)
					{
						string uriString = this._factorio.Resolve<IPnPathService>().PnMasterOrDrive + "\\u\\pn\\pixmap\\48\\logoPn.png";
						Image image = new Image();
						BitmapImage bitmapImage = new BitmapImage();
						bitmapImage.BeginInit();
						bitmapImage.UriSource = new Uri(uriString, UriKind.Absolute);
						bitmapImage.EndInit();
						image.Stretch = Stretch.None;
						image.Source = bitmapImage;
						MessageDisplay._pnLogo = image;
					}
					RadDesktopAlert alert = new RadDesktopAlert
					{
						Header = caption,
						Content = (text ?? message),
						ShowDuration = 5000,
						IconMargin = new Thickness(0.0, 0.0, 10.0, 0.0),
						Icon = MessageDisplay._pnLogo
					};
					MessageDisplay._alertManager.ShowAlert(alert, useAnimations: false);
				}
				else
				{
					MessageBox.Show(text ?? message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
				}
			}
			this._logger.Warning("ApplicationWarning: " + message);
		});
	}

	public void ShowWarningMessageAsNotification(string message, string caption = null)
	{
	}

	public void LogDebug(string message)
	{
		this._logger.Debug(message);
	}

	public IMessageDisplay CreateSubMessageDisplay(string subName)
	{
		return new MessageLogDoc(this._logger.GetSubLogger(subName), this._translator, this._factorio);
	}

	public void RefreshResources(bool localizationChanged, bool themeChanged)
	{
		this._translator.RefreshResources(localizationChanged, themeChanged);
	}
}

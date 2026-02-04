using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IMessageDisplay
{
	Action<(string caption, string message)>? RedirectMessageBoxesError { get; set; }

	Action<(string caption, string message)>? RedirectMessageBoxesWarning { get; set; }

	Action<(string caption, string message)>? RedirectMessageBoxesInfo { get; set; }

	void ShowErrorMessage(Exception exception);

	void ShowErrorMessage(Exception exception, string caption);

	void ShowInformationMessage(string message, string caption = null);

	void ShowWarningMessage(string message, string caption = null, bool notificationStyle = false);

	void ShowErrorMessage(string message, string caption = null);

	void LogErrorMessage(string message);

	void LogWarningMessage(string message);

	void LogDebug(string message);

	IMessageDisplay CreateSubMessageDisplay(string subName);

	void ShowTranslatedErrorMessage(string messageKey, params object[] parameters);

	void ShowTranslatedErrorMessages(List<string> messageKey, params object[] parameters);

	void ShowTranslatedWarningMessage(string messageKey, params object[] parameters);

	void ShowTranslatedInformationMessage(string messageKey, params object[] parameters);
}

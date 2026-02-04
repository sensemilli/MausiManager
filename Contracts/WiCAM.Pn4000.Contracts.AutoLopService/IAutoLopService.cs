using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.AutoLopService;

public interface IAutoLopService
{
	AutoLopState State { get; }

	IEnumerable<ILopErrorMessage> Errors { get; }

	IEnumerable<ILopErrorMessage> ErrorsCurrent { get; }

	IEnumerable<ILopErrorMessage> ErrorsCurrentTotal { get; }

	event Action OnLopBlockFinish;

	event Action<AutoLopState> OnAutoLopStateChanged;

	event Action<ILopErrorMessage> OnErrorDetected;

	void ClearErrors();

	void SetWorkingState();

	void SetNotWorkingState();

	void RaiseLopBlockFinish();

	void AddError(ILopErrorMessage model);

	void RaiseError(int errorId, string message, string description, string description2, float? x = null, float? y = null);

	IEnumerable<ILopErrorMessage> Run(string fullFileName);
}

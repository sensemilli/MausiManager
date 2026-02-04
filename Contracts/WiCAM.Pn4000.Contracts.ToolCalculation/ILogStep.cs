using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WiCAM.Pn4000.Contracts.ToolCalculation;

public interface ILogStep
{
	ILogStep? Parent { get; }

	List<ILogStep> SubSteps { get; }

	string DescShort { get; set; }

	DateTime Time { get; set; }

	TimeSpan? Duration { get; set; }

	List<(string text, object? complexInfo, DateTime? time)> TextElements { get; set; }

	ILogStep AddSubStep(string desc);

	void Write(string text, object? complexInfo = null);

	void WriteLine(string text, object? complexInfo = null);

	void WriteLine<T>(string text, IEnumerable<T> objects, Func<T, string> toString);

	void WriteErrorLine(string text, object? complexInfo = null, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0);
}

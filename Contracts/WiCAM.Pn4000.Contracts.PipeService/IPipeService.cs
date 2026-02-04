using System;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.Contracts.PipeService;

public interface IPipeService
{
	event Action<int, string> OnCommandBegin;

	event Action<int, string> OnCommandFinish;

	void Add(int groupNumber, string key, Action<string> action);

	void Add(int groupNumber, Func<string, bool> canExecute, Func<string, int> func);

	void Add(int groupNumber, Func<string, bool> canExecute, Func<string, Task<int>> funcAsync);

	int Execute(int groupNumber, string command);
}

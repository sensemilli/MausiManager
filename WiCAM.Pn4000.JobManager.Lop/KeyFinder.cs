using System.Collections.Generic;
using System.Text;

namespace WiCAM.Pn4000.JobManager.Lop;

internal class KeyFinder
{
	private readonly string _line;

	private int _current;

	private char _divider = '$';

	public KeyFinder(string line)
	{
		_line = line;
	}

	public IEnumerable<string> FindKeys()
	{
		List<string> list = new List<string>();
		string text = NextKey();
		while (!string.IsNullOrEmpty(text))
		{
			list.Add(text);
			text = NextKey();
		}
		return list;
	}

	private string NextKey()
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		bool flag = false;
		while (_current < _line.Length)
		{
			if (flag)
			{
				if (_line[_current] == _divider)
				{
					flag = false;
					_current++;
					return stringBuilder.ToString();
				}
				stringBuilder.Append(_line[_current]);
			}
			else if (_line[_current] == _divider)
			{
				flag = true;
			}
			_current++;
		}
		return null;
	}
}

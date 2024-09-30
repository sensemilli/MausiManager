using System.IO;
using System.Text;
using WiCAM.Pn4000.Encodings;

namespace WiCAM.Pn4000.JobManager.Classes;

internal class FileEncodingHelper
{
	public Encoding Find(string path)
	{
		Encoding systemEncoding = CurrentEncoding.SystemEncoding;
		using StreamReader streamReader = new StreamReader(path, CurrentEncoding.SystemEncoding, detectEncodingFromByteOrderMarks: true);
		if (streamReader.Peek() >= 0)
		{
			streamReader.Read();
		}
		return streamReader.CurrentEncoding;
	}
}

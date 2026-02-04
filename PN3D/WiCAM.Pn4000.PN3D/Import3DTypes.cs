using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WiCAM.Pn4000.PN3D;

public class Import3DTypes
{
	private static List<(string Desc, string Extension, int LicenceId)> _formatsList = new List<(string, string, int)>
	{
		("STEP - 3D data exchange format", "*.stp;*.step", 4602),
		("Initial Graphics Exchange Specification format", "*.igs;*.iges", 4603),
		("Inventor", "*.ipt;*.iam", 4614),
		("Solid Edge", "*.par;*.asm;*.psm", 4613),
		("Solidworks", "*.sldprt;*.sldasm;*.slddrw", 4612),
		("Parasolid", "*.x_t;*.xmt_txt;*.x_b;*.xmt_bin;*.p_b", 4615),
		("Autocad with ACIS", "*.dxf;*.dwg", 4604),
		("ACIS Modeler", "*.sat;*.sab;*.asat;*.asab", 4610),
		("Catia", "*.CATPart;*.CATProduct;*.CGR;*.CATDrawing", 4620),
		("NX", "*.prt", 4621),
		("ProE Part", "*.prt;*.prt.*;*.asm;*.asm.*", 4622),
		("Jupiter Tesselation ISO 3D data format ", "*.jt", 4623),
		("Only in Viewer suppoerted data format ", "*.stl;*.smx", 4611)
	};

	public static string GetFileDialogFilter()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("All files (*.*)|*.*");
		foreach (var formats in Import3DTypes._formatsList)
		{
			stringBuilder.Append("|");
			stringBuilder.Append(formats.Desc);
			stringBuilder.Append("(");
			stringBuilder.Append(formats.Extension);
			stringBuilder.Append(")");
			stringBuilder.Append("|");
			stringBuilder.Append(formats.Extension);
		}
		return stringBuilder.ToString();
	}

	public static (string Desc, string Extension, int LicenceId)? GetFormatInfo(string FileName)
	{
		foreach (var formats in Import3DTypes._formatsList)
		{
			if (Import3DTypes.FilenameMatchesFilters(FileName, formats.Extension))
			{
				return formats;
			}
		}
		return null;
	}

	private static bool FilenameMatchesFilters(string filename, string filters)
	{
		string[] array = filters.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			if (Import3DTypes.FilenameMatchesFilter(filename, array[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool FilenameMatchesFilter(string filename, string filter)
	{
		return Regex.Match(filename, Import3DTypes.WildcardToRegex(filter), RegexOptions.IgnoreCase | RegexOptions.Compiled).Value == filename;
	}

	private static string WildcardToRegex(string wildcard)
	{
		if (wildcard == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = wildcard.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '*')
			{
				stringBuilder.Append(".*");
			}
			else if (array[i] == '?')
			{
				stringBuilder.Append(".?");
			}
			else if ("+()^$.{}[]|\\".IndexOf(array[i]) != -1)
			{
				stringBuilder.Append('\\').Append(array[i]);
			}
			else
			{
				stringBuilder.Append(array[i]);
			}
		}
		return stringBuilder.ToString().ToLower();
	}
}

using System.Security.Cryptography;
using System.Text;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public class Global
{
	public static string ToMD5(string str)
	{
		Encoder encoder = Encoding.Unicode.GetEncoder();
		byte[] array = new byte[str.Length * 2];
		encoder.GetBytes(str.ToCharArray(), 0, str.Length, array, 0, flush: true);
		byte[] array2 = new MD5CryptoServiceProvider().ComputeHash(array);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array2.Length; i++)
		{
			stringBuilder.Append(array2[i].ToString("X2"));
		}
		return stringBuilder.ToString();
	}
}

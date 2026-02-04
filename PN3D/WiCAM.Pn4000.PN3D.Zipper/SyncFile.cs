using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WiCAM.Pn4000.PN3D.Zipper;

internal class SyncFile
{
	public static async Task<string> Upload(string file, MetaInfo metaInfo)
	{
		string abasId = metaInfo.AbasId;
		string installationId = "default";
		string type = "3d";
		string pnVersion = metaInfo.PnVersion;
		string user = metaInfo.UserName;
		string hostName = metaInfo.MachineName;
		HttpClient client = new HttpClient
		{
			Timeout = TimeSpan.FromMinutes(10L)
		};
		string url = "https://cora.wicam.com/upload/snapshot";
		byte[] fileContent = await File.ReadAllBytesAsync(file);
		string bundleId = Guid.NewGuid().ToString();
		int chunkSize = 5000000;
		if (chunkSize > fileContent.Length)
		{
			chunkSize = fileContent.Length;
		}
		int totalChunks = fileContent.Length / chunkSize;
		if (fileContent.Length % totalChunks != 0)
		{
			totalChunks++;
		}
		int sizeLastChunk = fileContent.Length - (totalChunks - 1) * chunkSize;
		byte[] buffer = new byte[chunkSize];
		string text = "";
		int srcOffset = 0;
		for (int i = 1; i <= totalChunks; i++)
		{
			if (i > 1)
			{
				srcOffset += chunkSize;
			}
			if (i == totalChunks)
			{
				if (fileContent.Length < chunkSize)
				{
					sizeLastChunk = fileContent.Length;
				}
				buffer = new byte[sizeLastChunk];
				Buffer.BlockCopy(fileContent, srcOffset, buffer, 0, sizeLastChunk);
				ByteArrayContent content = new ByteArrayContent(buffer, 0, buffer.Length);
				text = await (await client.PostAsync(url, new MultipartFormDataContent
				{
					{
						new StringContent(abasId),
						"abasId"
					},
					{
						new StringContent(installationId),
						"InstallationId"
					},
					{
						new StringContent(type),
						"type"
					},
					{
						new StringContent(pnVersion),
						"pnVersion"
					},
					{
						new StringContent(user),
						"user"
					},
					{
						new StringContent(hostName),
						"hostName"
					},
					{
						new StringContent(bundleId),
						"bundleId"
					},
					{
						new StringContent(i.ToString()),
						"current"
					},
					{
						new StringContent(totalChunks.ToString()),
						"max"
					},
					{ content, "file", "filename" }
				})).Content.ReadAsStringAsync();
				Console.WriteLine($"Chunk {i} of {totalChunks}");
				Console.WriteLine("Response: " + text);
			}
			else
			{
				Buffer.BlockCopy(fileContent, srcOffset, buffer, 0, chunkSize);
				ByteArrayContent content2 = new ByteArrayContent(buffer, 0, buffer.Length);
				text = await (await client.PostAsync(url, new MultipartFormDataContent
				{
					{
						new StringContent(abasId),
						"abasId"
					},
					{
						new StringContent(installationId),
						"InstallationId"
					},
					{
						new StringContent(type),
						"type"
					},
					{
						new StringContent(pnVersion),
						"pnVersion"
					},
					{
						new StringContent(user),
						"user"
					},
					{
						new StringContent(hostName),
						"hostName"
					},
					{
						new StringContent(bundleId),
						"bundleId"
					},
					{
						new StringContent(i.ToString()),
						"current"
					},
					{
						new StringContent(totalChunks.ToString()),
						"max"
					},
					{ content2, "file", "filename" }
				})).Content.ReadAsStringAsync();
				Console.WriteLine($"Chunk {i} of {totalChunks}");
				Console.WriteLine("Response: " + text);
			}
		}
		return text;
	}

	public static async Task Download(string downloadUrl, string targetFile)
	{
		HttpClient client = new HttpClient
		{
			Timeout = TimeSpan.FromMinutes(10L)
		};
		List<string> list = JsonConvert.DeserializeObject<List<string>>(await (await client.GetAsync(downloadUrl)).Content.ReadAsStringAsync());
		SortedList<int, byte[]> sortedBlobs = new SortedList<int, byte[]>();
		foreach (string item in list)
		{
			int key = int.Parse(item.Substring(item.LastIndexOf('-') + 1));
			Stream stream = await (await client.GetAsync("https://cora.wicam.com/download/snapshot?fileId=" + item)).Content.ReadAsStreamAsync();
			using MemoryStream ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			sortedBlobs.Add(key, ms.ToArray());
		}
		byte[] array = new byte[sortedBlobs.Values.Sum((byte[] a) => a.Length)];
		int num = 0;
		foreach (KeyValuePair<int, byte[]> item2 in sortedBlobs)
		{
			Buffer.BlockCopy(item2.Value, 0, array, num, item2.Value.Length);
			num += item2.Value.Length;
		}
		using MemoryStream mems = new MemoryStream(array);
		mems.Position = 0L;
		await using FileStream outputFileStream = new FileStream(targetFile, FileMode.Create);
		await mems.CopyToAsync(outputFileStream);
	}
}

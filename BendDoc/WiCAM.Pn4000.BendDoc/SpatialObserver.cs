using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiCAM.Pn4000.PN3D.Assembly;

namespace WiCAM.Pn4000.BendDoc;

internal class SpatialObserver
{
	private int? _bodies;

	private bool? _isAssembly;

	private Task _observationTask;

	private HashSet<int> _processedBodies = new HashSet<int>();

	private CancellationTokenSource _cancellationTokenSource;

	public void Run(string path, Assembly assembly, CancellationToken cancellationToken)
	{
		this._cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		CancellationToken linkedToken = this._cancellationTokenSource.Token;
		this._observationTask = Task.Run(async delegate
		{
			await this.ObserveFileAsync(path, assembly, linkedToken);
		}, linkedToken);
	}

	public void Stop()
	{
		this._cancellationTokenSource?.Cancel();
		this._cancellationTokenSource?.Dispose();
	}

	private async Task ObserveFileAsync(string filePath, Assembly assembly, CancellationToken cancellationToken)
	{
		TimeSpan initialDelay = TimeSpan.FromMilliseconds(100L, 0L);
		TimeSpan maxDelay = TimeSpan.FromSeconds(2L);
		int backoffFactor = 2;
		TimeSpan currentDelay = initialDelay;
		long lastFileSize = 0L;
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				long currentFileSize = new FileInfo(filePath).Length;
				if (currentFileSize != lastFileSize)
				{
					await this.ProcessFileChanges(filePath, assembly);
					currentDelay = initialDelay;
					lastFileSize = currentFileSize;
				}
				else
				{
					currentDelay = TimeSpan.FromMilliseconds(Math.Min(currentDelay.TotalMilliseconds * (double)backoffFactor, maxDelay.TotalMilliseconds));
				}
				await Task.Delay(currentDelay, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
			}
			catch (TaskCanceledException)
			{
			}
			catch
			{
				await Task.Delay(currentDelay);
			}
		}
		try
		{
			await this.ProcessFileChanges(filePath, assembly);
		}
		catch
		{
		}
	}

	private async Task ProcessFileChanges(string filePath, Assembly assembly)
	{
		_ = 1;
		try
		{
			string content;
			await using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using StreamReader reader = new StreamReader(stream);
				content = await reader.ReadToEndAsync();
			}
			var enumerable = from x in content.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
				select new
				{
					Timestamp = DateTime.ParseExact(x.Substring(0, 23), "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal),
					Message = x.Substring(33, x.Length - 33)
				};
			if (!this._isAssembly.HasValue)
			{
				var list = enumerable.Where(x => x.Message.StartsWith("num bodies:")).ToList();
				if (list.Count > 1)
				{
					this._isAssembly = true;
					assembly.RaiseOnSpatialProgress(new SpatialImportProgress
					{
						Timestamp = list.Last().Timestamp,
						IsAssembly = this._isAssembly.Value
					});
				}
			}
			if (!this._bodies.HasValue)
			{
				var anon = enumerable.FirstOrDefault(x => x.Message.StartsWith("total bodies:"));
				if (anon == null)
				{
					return;
				}
				this._bodies = int.Parse(anon.Message.Split(":")[1]);
				this._isAssembly = this._bodies != 1;
				assembly.RaiseOnSpatialProgress(new SpatialImportProgress
				{
					Timestamp = anon.Timestamp,
					IsAssembly = this._isAssembly.Value,
					TotalParts = this._bodies.Value
				});
			}
			foreach (var item in enumerable)
			{
				if (item.Message.StartsWith("Processed body hd "))
				{
					int num = item.Message.IndexOf("Faces");
					int num2 = item.Message.IndexOf("Triangles");
					int num3 = int.Parse(item.Message.Substring(18, num - 1 - 18));
					if (!this._processedBodies.Contains(num3))
					{
						string message = item.Message;
						int num4 = num + 7;
						int faces = int.Parse(message.Substring(num4, num2 - 1 - num4));
						string message2 = item.Message;
						num4 = num2 + 11;
						int triangles = int.Parse(message2.Substring(num4, message2.Length - num4));
						assembly.RaiseOnSpatialProgress(new SpatialImportProgress
						{
							Timestamp = item.Timestamp,
							IsAssembly = this._isAssembly.Value,
							TotalParts = this._bodies.Value,
							PartId = num3,
							Faces = faces,
							Triangles = triangles
						});
						this._processedBodies.Add(num3);
					}
				}
			}
		}
		catch
		{
		}
	}
}

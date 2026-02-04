using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.Loader.Loader;

namespace WiCAM.Pn4000.BendModel.Loader;

internal class SpatialAssemblyLoader : ModelLoader, ISpatialAssemblyLoader
{
	private readonly ISpatialLoader _spatialLoader;

	public SpatialAssemblyLoader(ISpatialLoader spatialLoader)
	{
		this._spatialLoader = spatialLoader;
	}

	public Model LoadSpatialAssemblyFile(string filename, bool lowTess, AnalyzeConfig analyzeConfig, bool loadModels = true)
	{
		Model model = new Model();
		Dictionary<int, Model> modelMap = new Dictionary<int, Model>();
		string directoryName = Path.GetDirectoryName(filename);
		foreach (AssemblyNode item in this.LoadSpatialAssemblyFile(filename))
		{
			this.ReadNode(item, model, modelMap, lowTess, directoryName, analyzeConfig, loadModels);
		}
		return model;
	}

	public List<AssemblyNode> LoadSpatialAssemblyFile(string filename)
	{
		List<AssemblyNode> list = SpatialAssemblyLoader.LoadAssemblyNodes(filename);
		using (StreamWriter textWriter = new StreamWriter(Path.Combine(Path.GetDirectoryName(filename), "assembly.json")))
		{
			using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.Formatting = Formatting.Indented;
			jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
			jsonSerializer.TypeNameHandling = TypeNameHandling.None;
			jsonSerializer.Culture = new CultureInfo("en-US");
			jsonSerializer.Serialize(jsonWriter, list.Select((AssemblyNode n) => ConvertToExt(n)).ToList());
			return list;
		}
		static AssemblyNodeExt ConvertToExt(AssemblyNode n)
		{
			return new AssemblyNodeExt
			{
				Name = n.Name,
				RefName = n.RefName,
				UID = n.UID,
				Hidden = n.Hidden,
				Suppressed = n.Suppressed,
				Transform = n.Transform,
				Bodies = n.Bodies.Select((AssemblyBody b) => new AssemblyBodyExt
				{
					BodyId = b.BodyId
				}).ToList(),
				Children = n.Children.Select((AssemblyNode c) => ConvertToExt(c)).ToList()
			};
		}
	}

	public static List<AssemblyNode> LoadAssemblyNodes(string filename)
	{
		using FileStream stream = new FileStream(filename, FileMode.Open);
		using StreamReader reader = new StreamReader(stream);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return new JsonSerializer
		{
			Formatting = Formatting.None,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			Culture = new CultureInfo("en-US")
		}.Deserialize<List<AssemblyNode>>(reader2);
	}

	private void ReadNode(AssemblyNode n, Model model, Dictionary<int, Model> modelMap, bool lowTess, string path, AnalyzeConfig analyzeConfig, bool loadModels)
	{
		Model model2 = new Model(model)
		{
			Name = n.RefName,
			Enabled = n.Suppressed
		};
		Matrix4d identity = Matrix4d.Identity;
		identity[0, 0] = n.Transform[0];
		identity[0, 1] = n.Transform[1];
		identity[0, 2] = n.Transform[2];
		identity[0, 3] = n.Transform[3];
		identity[1, 0] = n.Transform[4];
		identity[1, 1] = n.Transform[5];
		identity[1, 2] = n.Transform[6];
		identity[1, 3] = n.Transform[7];
		identity[2, 0] = n.Transform[8];
		identity[2, 1] = n.Transform[9];
		identity[2, 2] = n.Transform[10];
		identity[2, 3] = n.Transform[11];
		identity[3, 0] = n.Transform[12];
		identity[3, 1] = n.Transform[13];
		identity[3, 2] = n.Transform[14];
		identity[3, 3] = n.Transform[15];
		model2.Transform = identity;
		foreach (AssemblyBody item in n.Bodies.Where((AssemblyBody x) => x.BodyId.HasValue))
		{
			if (!modelMap.TryGetValue(item.BodyId.Value, out var value))
			{
				GeometryType bodyDimension = GeometryType.Volume;
				if (item.Dimension.HasValue)
				{
					bodyDimension = item.Dimension.Value switch
					{
						1 => GeometryType.Contour, 
						2 => GeometryType.Surface, 
						_ => GeometryType.Volume, 
					};
				}
				value = this.LoadModelPart(path, lowTess, item.BodyId, bodyDimension, n.RefName, n.Name, analyzeConfig);
				if (value != null)
				{
					modelMap.Add(item.BodyId.Value, value);
				}
			}
			model2.ReferenceModel.Add(new ModelInstance(value, model2));
		}
		foreach (AssemblyNode child in n.Children)
		{
			this.ReadNode(child, model2, modelMap, lowTess, path, analyzeConfig, loadModels);
		}
	}

	public Model LoadModelPart(string path, bool lowTess, int? bodyId, GeometryType bodyDimension, string refName, string geometryName, AnalyzeConfig analyzeConfig)
	{
		string filename = path + "/" + (lowTess ? "lowTess_" : "") + (int?)bodyId/*cast due to .constrained prefix*/ + ".txt";
		Model model = this._spatialLoader.LoadSpatialFile(filename, "", !lowTess, analyzeConfig);
		if (model != null)
		{
			model.Name = refName;
			model.GeometryName = geometryName;
		//	model.IsRefernceModel = true;
			model.PartInfo.GeometryType = bodyDimension;
			foreach (Shell shell in model.Shells)
			{
			//	shell.CreateCollisionTree();
			}
		}
		return model;
	}
}

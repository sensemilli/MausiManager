using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class DisassemblySimpleModel
{
	public class StructureNode : IDisposable
	{
		public string Name { get; set; }

		public TreeViewItem Item { get; internal set; }

		public AssemblyPartInfo CurrPart { get; internal set; }

		public List<StructureNode> SubNodes { get; set; } = new List<StructureNode>();

		public int Level { get; set; }

		public void Dispose()
		{
			foreach (StructureNode subNode in this.SubNodes)
			{
				subNode.Dispose();
			}
			this.SubNodes.Clear();
			this.CurrPart = null;
			this.Item = null;
		}
	}

	private Dictionary<int, TreeViewItem> _latestLevelNode = new Dictionary<int, TreeViewItem>();

	private Dictionary<TreeViewItem, StructureNode> _itemNodeDictionary;

	private int _treeLevel;

	private List<AssemblyPartInfo> _partsTmp;

	public List<AssemblyPartInfo> Parts { get; private set; }

	public MinMax ModelBoundary { get; private set; }

	public Model Assembly { get; private set; }

	public StructureNode StructureRootNode { get; private set; }

	public void GetModelFromDisassemblySimpleModel(DisassemblySimpleModel mainModel, int iD)
	{
		this.Parts = new List<AssemblyPartInfo>();
		AssemblyPartInfo item = new AssemblyPartInfo();
		this.Parts.Add(item);
		Pair<Vector3d, Vector3d> boundary = mainModel.Parts.FirstOrDefault((AssemblyPartInfo p) => p.ID == iD).PartModel.GetBoundary(Matrix4d.Identity);
		this.ModelBoundary = new MinMax
		{
			Min = boundary.Item1,
			Max = boundary.Item2
		};
	}

	public void Load3DModel(string orgFileName, ISpatialAssemblyLoader spatialAssemblyLoader, IGlobals globals)
	{
		try
		{
			this._latestLevelNode = new Dictionary<int, TreeViewItem>();
			this._partsTmp = new List<AssemblyPartInfo>();
			this._treeLevel = 0;
			this._itemNodeDictionary = new Dictionary<TreeViewItem, StructureNode>();
			this.Assembly = spatialAssemblyLoader.LoadSpatialAssemblyFile("cad3d2pn\\assemblyInternal.bin", lowTess: true, null);
			Pair<Vector3d, Vector3d> boundary = this.Assembly.GetBoundary(Matrix4d.Identity);
			this.ModelBoundary = new MinMax
			{
				Min = boundary.Item1,
				Max = boundary.Item2
			};
			this.StructureRootNode = new StructureNode
			{
				Level = 0,
				Name = orgFileName
			};
			this.ProcedureTree(this.Assembly, null, isReferenceModel: false);
			foreach (ModelInstance sm in this.Assembly.GetAllSubModelsWithSelf().Where(delegate(Model x)
			{
				List<ModelInstance> referenceModel = x.ReferenceModel;
				return referenceModel != null && referenceModel.Count > 0;
			}).SelectMany((Model y) => y.ReferenceModel))
			{
				this._partsTmp.FirstOrDefault((AssemblyPartInfo x) => sm.Reference.BodyId == x.ID)?.Matrixes.Add(sm.Parent.WorldMatrix);
			}
			this.Parts = new List<AssemblyPartInfo>();
			foreach (AssemblyPartInfo item in this._partsTmp)
			{
				foreach (Model item2 in this.Assembly.GetAllSubModelsWithSelf().Where(delegate(Model x)
				{
					List<ModelInstance> referenceModel2 = x.ReferenceModel;
					return referenceModel2 != null && referenceModel2.Count > 0;
				}))
				{
					DisassemblySimpleModel.GetRefModel(item2, item);
				}
				this.Parts.Add(item);
			}
		}
		catch (Exception e)
		{
			globals.logCenterService.CatchRaport(e);
		}
	}

	private static void GetRefModel(Model model, AssemblyPartInfo p)
	{
		foreach (Model item in model.ReferenceModel.Select((ModelInstance x) => x.Reference))
		{
			if (item.BodyId == p.ID)
			{
				p.PartModel = item;
				break;
			}
		}
	}

	private static string GetPathString(StructureNode structureRootNode, StructureNode currentNode)
	{
		string name = structureRootNode.Name;
		if (structureRootNode == currentNode)
		{
			return name;
		}
		return DisassemblySimpleModel.FindNode(name, structureRootNode, currentNode);
	}

	private static string FindNode(string path, StructureNode testRootNode, StructureNode currentNode)
	{
		foreach (StructureNode subNode in testRootNode.SubNodes)
		{
			string text = path + "\\" + subNode.Name;
			if (subNode == currentNode)
			{
				return text;
			}
			string text2 = DisassemblySimpleModel.FindNode(text, subNode, currentNode);
			if (text2 != string.Empty)
			{
				return text2;
			}
		}
		return string.Empty;
	}

	private static AssemblyPartInfo AddPart(string v, IList<AssemblyPartInfo> parts, string path)
	{
		if (!int.TryParse(v, out var result))
		{
			return null;
		}
		AssemblyPartInfo assemblyPartInfo = null;
		int num = 0;
		while (assemblyPartInfo == null && num < parts.Count)
		{
			if (parts[num].ID == result)
			{
				assemblyPartInfo = parts[num];
			}
			else
			{
				num++;
			}
		}
		if (assemblyPartInfo != null)
		{
			assemblyPartInfo.Count++;
		}
		else
		{
			assemblyPartInfo = new AssemblyPartInfo
			{
				ID = result,
				Count = 1
			};
			parts.Add(assemblyPartInfo);
		}
		assemblyPartInfo.Instances.Add(path);
		return assemblyPartInfo;
	}

	private void ProcedureTree(Model model, TreeViewItem parentItem, bool isReferenceModel)
	{
		TreeViewItem treeViewItem = new TreeViewItem();
		StructureNode structureNode = null;
		if (parentItem != null)
		{
			structureNode = this._itemNodeDictionary?[parentItem];
		}
		string text = ((model.Shells.Count > 0) ? model.GeometryName : model.Name);
		if (parentItem != null && structureNode != null && this._treeLevel > 1)
		{
			parentItem.Header = model.Name;
			structureNode.Name = model.Name;
		}
		else if (string.IsNullOrEmpty(text))
		{
			text = ((this._treeLevel == 0) ? this.StructureRootNode.Name : this._treeLevel.ToString());
		}
		StructureNode structureNode2 = new StructureNode
		{
			Item = treeViewItem,
			Level = this._treeLevel,
			Name = text
		};
		if (!isReferenceModel)
		{
			this._treeLevel++;
		}
		if (structureNode2.Level == 0)
		{
			this.StructureRootNode = structureNode2;
		}
		else
		{
			structureNode?.SubNodes.Add(structureNode2);
		}
		structureNode2.CurrPart = DisassemblySimpleModel.AddPart((!string.IsNullOrEmpty(model.FileName)) ? model.FileName.Split('_').Last().Split('.')
			.First() : "", this._partsTmp, DisassemblySimpleModel.GetPathString(this.StructureRootNode, structureNode2));
		if (structureNode2.CurrPart != null)
		{
			structureNode2.CurrPart.AssemblyName = model.Name;
			structureNode2.CurrPart.Name = model.GeometryName;
			structureNode2.CurrPart.GeometryType = model.PartInfo.GeometryType;
			structureNode2.CurrPart.PartModel = model;
		}
		treeViewItem.Header = structureNode2.Name;
		this._itemNodeDictionary.Add(treeViewItem, structureNode2);
		if (this._latestLevelNode.ContainsKey(structureNode2.Level - 1))
		{
			this._latestLevelNode[structureNode2.Level - 1].Items.Add(treeViewItem);
		}
		if (this._latestLevelNode.ContainsKey(structureNode2.Level))
		{
			this._latestLevelNode.Remove(structureNode2.Level);
		}
		this._latestLevelNode.Add(structureNode2.Level, treeViewItem);
		foreach (Model subModel in model.SubModels)
		{
			int treeLevel = this._treeLevel;
			this.ProcedureTree(subModel, treeViewItem, isReferenceModel: false);
			this._treeLevel = treeLevel;
		}
		if (model.ReferenceModel.Count <= 0)
		{
			return;
		}
		foreach (Model item in model.ReferenceModel.Select((ModelInstance m) => m.Reference))
		{
			this.ProcedureTree(item, treeViewItem, isReferenceModel: true);
		}
	}
}

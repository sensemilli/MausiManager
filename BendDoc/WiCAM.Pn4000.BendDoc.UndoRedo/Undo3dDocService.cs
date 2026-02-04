using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.BendDoc.UndoRedo;

internal class Undo3dDocService : IUndo3dDocService
{
	private readonly IDoc3d _doc;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IConfigProvider _configProvider;

	private readonly IAutoMode _autoMode;

	private List<IUndo3dSave> _undo = new List<IUndo3dSave>();

	private IUndo3dSave? _current;

	private List<IUndo3dSave> _redo = new List<IUndo3dSave>();

	private SModel? _inputModel;

	private SModel? _entryModel;

	public event Action SavesChanged;

	public Undo3dDocService(IDoc3d doc, ICurrentDocProvider currentDocProvider, IConfigProvider configProvider, IAutoMode autoMode)
	{
		this._doc = doc;
		this._currentDocProvider = currentDocProvider;
		this._configProvider = configProvider;
		this._autoMode = autoMode;
	}

	public void Reset()
	{
		this._current = null;
		this._inputModel = null;
		this._entryModel = null;
		this._undo.Clear();
		this._redo.Clear();
		this.SavesChanged?.Invoke();
	}

	public void Save(string messageLastAction)
	{
		if (this.Activated(out var undoAmount))
		{
			SPnBndDoc sPnBndDoc = this._doc.Factorio.Resolve<IDocConverter>().ConvertDoc(this._doc, this._inputModel != null);
			if (this._inputModel == null)
			{
				this._inputModel = sPnBndDoc.InputModel;
				this._entryModel = sPnBndDoc.EntryModel;
			}
			else
			{
				sPnBndDoc.InputModel = this._inputModel;
				sPnBndDoc.EntryModel = this._entryModel;
			}
			if (this._current != null)
			{
				this._undo.Add(this._current);
			}
			this._current = new Undo3dSave(messageLastAction, sPnBndDoc);
			this._redo.Clear();
			if (0 < undoAmount && 0 < this._undo.Count - undoAmount)
			{
				this._undo.RemoveRange(0, this._undo.Count - undoAmount);
			}
			this.SavesChanged?.Invoke();
		}
	}

	public bool Undo()
	{
		if (this._undo.Count > 0)
		{
			if (this._current != null)
			{
				this._redo.Add(this._current);
			}
			this._current = this._undo.Last();
			this._undo.Remove(this._current);
			this.Load(this._current);
			this.SavesChanged?.Invoke();
			return true;
		}
		return false;
	}

	public bool Redo()
	{
		if (this._redo.Count > 0)
		{
			if (this._current != null)
			{
				this._undo.Add(this._current);
			}
			this._current = this._redo.Last();
			this._redo.Remove(this._current);
			this.Load(this._current);
			this.SavesChanged?.Invoke();
			return true;
		}
		return false;
	}

	public bool Goto(IUndo3dSave save)
	{
		int num = this._undo.IndexOf(save);
		if (num >= 0)
		{
			if (this._current != null)
			{
				this._redo.Add(this._current);
			}
			this._current = save;
			this._redo.AddRange(this._undo.Skip(num + 1).Reverse());
			this._undo.RemoveRange(num, this._undo.Count - num);
			this.Load(this._current);
			this.SavesChanged?.Invoke();
			return true;
		}
		num = this._redo.IndexOf(save);
		if (num >= 0)
		{
			if (this._current != null)
			{
				this._undo.Add(this._current);
			}
			this._current = save;
			this._undo.AddRange(this._redo.Skip(num + 1).Reverse());
			this._redo.RemoveRange(num, this._redo.Count - num);
			this.Load(this._current);
			this.SavesChanged?.Invoke();
			return true;
		}
		return false;
	}

	private void Load(IUndo3dSave save)
	{
		if (save.Data is SPnBndDoc sDoc)
		{
			this._doc.Factorio.Resolve<IDocConverter>().ConvertDoc(sDoc, this._doc, "");
		}
	}

	public IEnumerable<IUndo3dSave> GetSavesUndo()
	{
		return this._undo;
	}

	public IEnumerable<IUndo3dSave> GetSavesRedo()
	{
		return this._redo;
	}

	private bool Activated(out int undoAmount)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		undoAmount = generalUserSettingsConfig.Undo3dAmount ?? 30;
		if (this._autoMode.PopupsEnabled && (generalUserSettingsConfig.UseUndo3d ?? true))
		{
			return undoAmount != 0;
		}
		return false;
	}
}

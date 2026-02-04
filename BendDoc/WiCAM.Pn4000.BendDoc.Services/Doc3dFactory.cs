using System;
using System.IO;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.BendDoc.Services;

public class Doc3dFactory : IInternalDocFactory, IDoc3dFactory
{
	private readonly IDocManagerInternal _docManager;

	private IDoc3d _emptyDoc;

	public IDoc3d EmptyDoc => this._emptyDoc ?? (this._emptyDoc = this.CreateDoc(null, isAssemblyLoading: false, null, isDevImport: false));

	public Doc3dFactory(IDocManagerInternal docManager)
	{
		this._docManager = docManager;
	}

	public IDoc3d CreateDoc(string filename, bool isAssemblyLoading, string? assemblyGuid, bool isDevImport)
	{
		IDoc3d doc3d = this.CreateDoc();
		doc3d.Init(filename, isAssemblyLoading, assemblyGuid, isDevImport);
		return doc3d;
	}

	public IDoc3d CreateDoc()
	{
		return this._docManager.CreateNewScope().Resolve<IDoc3d>();
	}

	public IDoc3d CreateEmptyDoc(string filename)
	{
		IDoc3d doc3d = this.CreateDoc(filename, isAssemblyLoading: false, null, isDevImport: false);
		doc3d.DiskFile.Header.ImportedFilename = filename;
		try
		{
			doc3d.DiskFile.Header.ModelName = new FileInfo(filename).Name;
		}
		catch (Exception)
		{
		}
		return doc3d;
	}

	public Doc3d InternalCreateDoc()
	{
		return (Doc3d)this.CreateDoc();
	}

	public Doc3d InternalCreateDoc(string filename)
	{
		Doc3d doc3d = this.InternalCreateDoc();
		doc3d.Init(filename);
		return doc3d;
	}
}

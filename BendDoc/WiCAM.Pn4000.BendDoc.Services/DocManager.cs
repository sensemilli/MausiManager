using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.BendDoc.Services;

public class DocManager : IDocManagerInternal, IDocManager
{
	private class DocWrapper
	{
		private WeakReference<IDoc3d> _doc;

		private WeakReference<IScopedFactorio> _scopedFactorio;

		public bool IsAlive
		{
			get
			{
				IDoc3d target;
				return this._doc.TryGetTarget(out target);
			}
		}

		public IDoc3d? Doc
		{
			get
			{
				if (!this._doc.TryGetTarget(out IDoc3d target))
				{
					return null;
				}
				return target;
			}
		}

		public IScopedFactorio? ScopedFactorio
		{
			get
			{
				if (!this._scopedFactorio.TryGetTarget(out IScopedFactorio target))
				{
					return null;
				}
				return target;
			}
		}

		public DocWrapper(IDoc3d doc, IScopedFactorio scopedFactorio)
		{
			this._doc = new WeakReference<IDoc3d>(doc);
			this._scopedFactorio = new WeakReference<IScopedFactorio>(scopedFactorio);
		}
	}

	private class ScopeNotFoundException : Exception
	{
		public string Id { get; }

		public ScopeNotFoundException(string id)
			: base("Id (" + id + ") not found!")
		{
			this.Id = id;
		}
	}

	private readonly IServiceProvider _serviceProvider;

	private readonly ConcurrentDictionary<string, DocWrapper> _wrappers = new ConcurrentDictionary<string, DocWrapper>();

	public IEnumerable<IDoc3d> Documents => from x in this._wrappers.Values
		where x.IsAlive
		select x.Doc;

	public IEnumerable<IScopedFactorio> ScopedFactorios => from x in this._wrappers.Values
		where x.IsAlive
		select x.ScopedFactorio;

	public DocManager(IServiceProvider serviceProvider)
	{
		this._serviceProvider = serviceProvider;
	}

	public IScopedFactorio GetScope(IPnBndDoc doc)
	{
		if (this._wrappers.TryGetValue(doc?.Id, out DocWrapper value))
		{
			return value.ScopedFactorio;
		}
		throw new ScopeNotFoundException(doc?.Id);
	}

	IScopedFactorio IDocManagerInternal.CreateNewScope()
	{
		IScopedFactorio requiredService = this._serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IScopedFactorio>();
		IDoc3d doc3d = requiredService.Resolve<IDoc3d>();
		DocWrapper value = new DocWrapper(doc3d, requiredService);
		this._wrappers.TryAdd(doc3d.Id, value);
		this.CleanupScopes();
		return requiredService;
	}

	private void CleanupScopes()
	{
		foreach (KeyValuePair<string, DocWrapper> wrapper in this._wrappers)
		{
			if (!wrapper.Value.IsAlive)
			{
				this._wrappers.Remove(wrapper.Key, out var _);
			}
		}
	}

	public void CloseAllDocuments()
	{
		ImmutableArray<IDoc3d>.Enumerator enumerator = this.Documents.ToImmutableArray().GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Close();
		}
	}
}

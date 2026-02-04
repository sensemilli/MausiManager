using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Helpers;

public class ClassCompare
{
	internal class SpecialPair
	{
		private readonly object _o1;

		private readonly object _o2;

		public SpecialPair(object o1, object o2)
		{
			this._o1 = o1;
			this._o2 = o2;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(this._o1.GetHashCode(), this._o2.GetHashCode());
		}

		public override bool Equals(object? obj)
		{
			if (!(obj is SpecialPair specialPair))
			{
				return false;
			}
			if (this._o1 == specialPair._o1)
			{
				return this._o2 == specialPair._o2;
			}
			return false;
		}
	}

	public class Difference
	{
		private readonly PropertyInfo _propertyInfo;

		private readonly object _valueSelf;

		private readonly object _valueTo;

		public Difference(PropertyInfo propertyInfo, object valueSelf, object valueTo)
		{
			this._propertyInfo = propertyInfo;
			this._valueSelf = valueSelf;
			this._valueTo = valueTo;
		}
	}

	private HashSet<SpecialPair> _alreadyInspected;

	private HashSet<string> _typeIgnore;

	private HashSet<string> _nameIgnore;

	private List<Difference> _differences;

	public ClassCompare(string[] typeIgnore, string[] nameIgnore)
	{
		this._typeIgnore = new HashSet<string>(typeIgnore);
		this._nameIgnore = new HashSet<string>(nameIgnore);
	}

	public List<Difference> ClassesEqual<T>(T self, T to) where T : class
	{
		this._alreadyInspected = new HashSet<SpecialPair>();
		this._differences = new List<Difference>();
		this.ClassesEqualInternal(self, to);
		return this._differences;
	}

	private void ClassesEqualInternal<T>(T self, T to) where T : class
	{
		if (self != null && to != null)
		{
			if (this._alreadyInspected.Contains(new SpecialPair(self, to)))
			{
				return;
			}
			this._alreadyInspected.Add(new SpecialPair(self, to));
			Type type = self.GetType();
			Type type2 = to.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo propertyInfo in properties)
			{
				string name = propertyInfo.PropertyType.Name;
				if (this._typeIgnore.Contains(name) || this._nameIgnore.Contains(propertyInfo.Name))
				{
					continue;
				}
				PropertyInfo property = type.GetProperty(propertyInfo.Name);
				PropertyInfo property2 = type2.GetProperty(propertyInfo.Name);
				if (self is IEnumerable)
				{
					IEnumerable obj = (IEnumerable)self;
					IEnumerable enumerable = (IEnumerable)to;
					IEnumerator enumerator = obj.GetEnumerator();
					IEnumerator enumerator2 = enumerable.GetEnumerator();
					while (true)
					{
						bool flag = enumerator.MoveNext();
						bool flag2 = enumerator2.MoveNext();
						if (flag != flag2)
						{
							this._differences.Add(new Difference(property, self, to));
						}
						else if (!flag && !flag2)
						{
							break;
						}
						if (flag == flag2)
						{
							name = enumerator.Current.GetType().Name;
							if (!this._typeIgnore.Contains(name))
							{
								this.ClassesEqualInternal(enumerator.Current, enumerator2.Current);
							}
						}
					}
					continue;
				}
				if (property.GetIndexParameters().Length != 0)
				{
					if (!(self is Vector2d) && !(self is Vector3d))
					{
						throw new Exception();
					}
					continue;
				}
				object obj2 = null;
				object obj3 = null;
				if (property2 == null)
				{
					this._differences.Add(new Difference(property, property, property2));
					continue;
				}
				try
				{
					obj2 = property.GetValue(self, null);
					obj3 = property2.GetValue(to, null);
				}
				catch (TargetInvocationException)
				{
					continue;
				}
				if (propertyInfo.PropertyType.IsInterface || (propertyInfo.PropertyType.IsClass && !propertyInfo.PropertyType.Module.ScopeName.Equals("CommonLanguageRuntimeLibrary")))
				{
					this.ClassesEqualInternal(obj2, obj3);
				}
				else if (obj2 != obj3 && (obj2 == null || !obj2.Equals(obj3)))
				{
					this._differences.Add(new Difference(property, obj2, obj3));
				}
			}
		}
		else if (self != to)
		{
			this._differences.Add(new Difference(null, self, to));
		}
	}
}

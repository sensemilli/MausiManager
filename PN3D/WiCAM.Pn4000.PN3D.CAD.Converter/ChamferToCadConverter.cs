using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.CadGeo;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.CAD.Converter;

public class ChamferToCadConverter : MacroToCadConverterBase
{
	[CompilerGenerated]
	private sealed class _003CProjectOutline_003Ed__2 : IEnumerable<Vector2d>, IEnumerable, IEnumerator<Vector2d>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Vector2d _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> points;

		public List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> _003C_003E3__points;

		private global::WiCAM.Pn4000.BendModel.Base.Matrix4d transform;

		public global::WiCAM.Pn4000.BendModel.Base.Matrix4d _003C_003E3__transform;

		private List<global::WiCAM.Pn4000.BendModel.Base.Vector3d>.Enumerator _003C_003E7__wrap1;

		Vector2d IEnumerator<Vector2d>.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return this._003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CProjectOutline_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			this._003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = this._003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					this._003C_003Em__Finally1();
				}
			}
			this._003C_003E7__wrap1 = default(List<global::WiCAM.Pn4000.BendModel.Base.Vector3d>.Enumerator);
			this._003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (this._003C_003E1__state)
				{
				default:
					return false;
				case 0:
					this._003C_003E1__state = -1;
					this._003C_003E7__wrap1 = this.points.GetEnumerator();
					this._003C_003E1__state = -3;
					break;
				case 1:
					this._003C_003E1__state = -3;
					break;
				}
				if (this._003C_003E7__wrap1.MoveNext())
				{
					global::WiCAM.Pn4000.BendModel.Base.Vector3d v = this._003C_003E7__wrap1.Current;
					this.transform.TransformInPlace(ref v);
					this._003C_003E2__current = new Vector2d(v.X, v.Y);
					this._003C_003E1__state = 1;
					return true;
				}
				this._003C_003Em__Finally1();
				this._003C_003E7__wrap1 = default(List<global::WiCAM.Pn4000.BendModel.Base.Vector3d>.Enumerator);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			this._003C_003E1__state = -1;
			((IDisposable)this._003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Vector2d> IEnumerable<Vector2d>.GetEnumerator()
		{
			_003CProjectOutline_003Ed__2 _003CProjectOutline_003Ed__;
			if (this._003C_003E1__state == -2 && this._003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				this._003C_003E1__state = 0;
				_003CProjectOutline_003Ed__ = this;
			}
			else
			{
				_003CProjectOutline_003Ed__ = new _003CProjectOutline_003Ed__2(0);
			}
			_003CProjectOutline_003Ed__.points = this._003C_003E3__points;
			_003CProjectOutline_003Ed__.transform = this._003C_003E3__transform;
			return _003CProjectOutline_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Vector2d>)this).GetEnumerator();
		}
	}

	public static bool AddCadGeoElements(Chamfer chamfer, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix, Cad2DDatabase db2D, IConfigProvider configProvider, ref int counter, bool showChamferLine)
	{
		Macro3DConfig macro3DConfig = configProvider.InjectOrCreate<Macro3DConfig>();
		double num = chamfer.UpperChamferAngle * 180.0 / Math.PI;
		double num2 = chamfer.LowerChamferAngle * 180.0 / Math.PI;
		double num3 = chamfer.UpperChamferHeight;
		double num4 = chamfer.LowerChamferHeight;
		global::WiCAM.Pn4000.BendModel.Base.Matrix4d transform = worldMatrix;
		List<List<global::WiCAM.Pn4000.BendModel.Base.Vector3d>> list;
		List<List<global::WiCAM.Pn4000.BendModel.Base.Vector3d>> list2;
		switch (MacroToCadConverterBase.GetOrientation(chamfer.Orientation, worldMatrix))
		{
		case OrientationTypes.Top:
			list = chamfer.UpperConnectingLine;
			list2 = chamfer.LowerConnectingLine;
			break;
		case OrientationTypes.Bottom:
		{
			list = chamfer.LowerConnectingLine;
			list2 = chamfer.UpperConnectingLine;
			double num5 = num2;
			double num6 = num4;
			double num7 = num;
			num4 = num3;
			num2 = num7;
			num3 = num6;
			num = num5;
			break;
		}
		default:
			return false;
		}
		HashSet<CadGeoElement> hashSet = new HashSet<CadGeoElement>();
		HashSet<CadGeoElement> hashSet2 = new HashSet<CadGeoElement>();
		if (showChamferLine)
		{
			foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> item in list)
			{
				foreach (CadGeoElement item2 in MacroToCadConverterBase.GetContour(ChamferToCadConverter.ProjectOutline(item, transform).ToList(), macro3DConfig.ChamferPosColorPn, isOpenContour: true))
				{
					hashSet.Add(item2);
				}
			}
			foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> item3 in list2)
			{
				foreach (CadGeoElement item4 in MacroToCadConverterBase.GetContour(ChamferToCadConverter.ProjectOutline(item3, transform).ToList(), macro3DConfig.ChamferNegColorPn, isOpenContour: true))
				{
					hashSet2.Add(item4);
				}
			}
		}
		foreach (List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> item5 in chamfer.TextLine)
		{
			foreach (CadGeoElement item6 in MacroToCadConverterBase.GetContour(ChamferToCadConverter.ProjectOutline(item5, transform).ToList(), macro3DConfig.MacroPosColorPn, isOpenContour: true))
			{
				counter++;
				if (item6 is CadGeoLine cadGeoLine)
				{
					db2D.AddText(ChamferToCadConverter.GetCadTxt(cadGeoLine.StartPoint, 270.0, counter, num, num2, num3, num4));
					db2D.AddText(ChamferToCadConverter.GetCadTxt(cadGeoLine.EndPoint, 360.0, counter, num, num2, num3, num4));
				}
				else if (item6 is CadGeoCircle cadGeoCircle)
				{
					Vector2d point = ChamferToCadConverter.CalculateCirclePoint(cadGeoCircle, cadGeoCircle.StartAngle);
					Vector2d point2 = ChamferToCadConverter.CalculateCirclePoint(cadGeoCircle, cadGeoCircle.EndAngle);
					db2D.AddText(ChamferToCadConverter.GetCadTxt(point, 270.0, counter, num, num2, num3, num4));
					db2D.AddText(ChamferToCadConverter.GetCadTxt(point2, 360.0, counter, num, num2, num3, num4));
				}
			}
		}
		db2D.AddInnerLine(hashSet);
		db2D.AddInnerLine(hashSet2);
		global::WiCAM.Pn4000.BendModel.Base.Vector3d v = chamfer.AnchorPoint;
		worldMatrix.TransformInPlace(ref v);
		return true;
	}

	private static CadTxtText GetCadTxt(Vector2d point, double textAngle, int id, double upperAngle, double lowerAngle, double upperHeight, double lowerHeight)
	{
        var text = string.Format(CultureInfo.InvariantCulture,
         "#FASE_#{0}_{1:0.0}_{2:0.0}_{3:0.0}_{4:0.0}",
         id,
         upperAngle,
         upperHeight,
         lowerAngle,
         lowerHeight);

        return new CadTxtText
        {
            Text = text,
            Angle = textAngle,
            Color = 61,
            Height = 1.0,
            Position = new Vector2d(point.X, point.Y)
        };
    }

	[IteratorStateMachine(typeof(_003CProjectOutline_003Ed__2))]
	private static IEnumerable<Vector2d> ProjectOutline(List<global::WiCAM.Pn4000.BendModel.Base.Vector3d> points, global::WiCAM.Pn4000.BendModel.Base.Matrix4d transform)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CProjectOutline_003Ed__2(-2)
		{
			_003C_003E3__points = points,
			_003C_003E3__transform = transform
		};
	}

	private static Vector2d CalculateCirclePoint(CadGeoCircle eCircle, double angle)
	{
		return new Vector2d(eCircle.Center.X + eCircle.Radius * Math.Cos(angle * Math.PI / 180.0), eCircle.Center.Y + eCircle.Radius * Math.Sin(angle * Math.PI / 180.0));
	}

	public static ChamfereXml GetXmlElement(Chamfer chamfer, global::WiCAM.Pn4000.BendModel.Base.Matrix4d worldMatrix)
	{
		return new ChamfereXml
		{
			ID = chamfer.ID,
			Length = chamfer.Length,
			LowerChamferAngle = chamfer.LowerChamferAngle * 180.0 / Math.PI,
			LowerChamferHeight = chamfer.LowerChamferHeight,
			UpperChamferAngle = chamfer.UpperChamferAngle * 180.0 / Math.PI,
			UpperChamferHeight = chamfer.UpperChamferHeight
		};
	}
}

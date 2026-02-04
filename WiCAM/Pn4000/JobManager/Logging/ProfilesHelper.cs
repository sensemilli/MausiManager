#region Assembly BendDataBase, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Standort unbekannt
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.Modifiers;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Geometry2D;
using WiCAM.Pn4000.BendModel.Base.Geometry2D.PnGeometry;

namespace BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Helper;

public class ProfilesHelper : BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces.IProfilesHelper
{
    public bool Serialize(string path, PreferredProfiles profiles)
    {
        return Xml.SerializeToXml(profiles, path);
    }

    public PreferredProfiles Deserialize(string path)
    {
        return Xml.DeserializeFromXml<PreferredProfiles>(path);
    }

    public (Vector2d, Vector2d) GetStartEnd(object geo)
    {
        if (geo is Line2D)
        {
            Line2D line2D = (Line2D)geo;
            return (line2D.P0, line2D.P1);
        }

        CircleSegment2D circleSegment2D = (CircleSegment2D)geo;
        return (circleSegment2D.Start, circleSegment2D.End);
    }

    public object FlipGeo(object geo)
    {
        if (geo is Line2D)
        {
            Line2D line2D = (Line2D)geo;
            return new Line2D(line2D.P1, line2D.P0);
        }

        CircleSegment2D circleSegment2D = (CircleSegment2D)geo;
        return new CircleSegment2D(circleSegment2D.Radius, circleSegment2D.Center, circleSegment2D.End, circleSegment2D.Start, !circleSegment2D.IsCCW, CircleSegmentCreationMode.KeepCenterAndRadius);
    }

    public object CopyGeo(object geo)
    {
        if (geo is Line2D)
        {
            return ((Line2D)geo).Clone();
        }

        return ((CircleSegment2D)geo).Clone();
    }

    public double LinesAngle(Line2D line)
    {
        double num = Math.Atan2(line.P1.Y - line.P0.Y, line.P1.X - line.P0.X) * (180.0 / Math.PI);
        if (num <= 0.0)
        {
            num += 360.0;
        }

        return num;
    }

    public double RelativeAngle(Line2D lineaA, Line2D lineB)
    {
        double num = LinesAngle(lineaA);
        if (num > 180.0)
        {
            num -= 180.0;
        }

        double num2 = LinesAngle(lineB);
        if (num2 > 180.0)
        {
            num2 -= 180.0;
        }

        double num3 = Math.Abs(num - num2);
        if (num3 > 180.0)
        {
            num3 = 360.0 - num3;
        }

        return num3;
    }

    public List<object> GetGeoInOrder(List<object> elems, CadGeoInfo cadGeo, bool farthestFromOriginStart = false)
    {
        List<object> list = new List<object>();
        bool[] array = new bool[elems.Count];
        int num = 0;
        double num2 = (farthestFromOriginStart ? double.MinValue : double.MaxValue);
        for (int i = 0; i < elems.Count; i++)
        {
            (Vector2d, Vector2d) startEnd = GetStartEnd(elems[i]);
            double num3 = Math.Pow(startEnd.Item1.X + Math.Abs(Math.Min(cadGeo.TopLeft.X, cadGeo.BottomRight.X)), 2.0) + Math.Pow(startEnd.Item1.Y + Math.Abs(Math.Min(cadGeo.TopLeft.Y, cadGeo.BottomRight.Y)), 2.0);
            if (farthestFromOriginStart ? (num3 > num2) : (num3 < num2))
            {
                num2 = num3;
                num = i;
            }
        }

        list.Add(elems[num]);
        array[num] = true;
        for (int j = 0; j < elems.Count - 1; j++)
        {
            object geo = list.Last();
            (Vector2d, Vector2d) startEnd2 = GetStartEnd(geo);
            (double, int, bool) tuple = (double.MaxValue, -1, false);
            for (int k = 0; k < elems.Count; k++)
            {
                if (!array[k])
                {
                    object geo2 = elems[k];
                    (Vector2d, Vector2d) startEnd3 = GetStartEnd(geo2);
                    double num4 = Math.Pow(startEnd2.Item2.X - startEnd3.Item1.X, 2.0) + Math.Pow(startEnd2.Item2.Y - startEnd3.Item1.Y, 2.0);
                    double num5 = Math.Pow(startEnd2.Item2.X - startEnd3.Item2.X, 2.0) + Math.Pow(startEnd2.Item2.Y - startEnd3.Item2.Y, 2.0);
                    if (num4 <= tuple.Item1)
                    {
                        tuple = (num4, k, false);
                    }

                    if (num5 <= tuple.Item1)
                    {
                        tuple = (num5, k, true);
                    }
                }
            }

            if (tuple.Item1 != double.MaxValue)
            {
                list.Add(tuple.Item3 ? FlipGeo(elems[tuple.Item2]) : CopyGeo(elems[tuple.Item2]));
                array[tuple.Item2] = true;
            }
        }

        if (list.Count != elems.Count)
        {
            return null;
        }

        double num6 = 0.0;
        foreach (object item in list)
        {
            (Vector2d, Vector2d) startEnd4 = GetStartEnd(item);
            num6 += (startEnd4.Item2.X - startEnd4.Item1.X) * (startEnd4.Item2.Y + startEnd4.Item1.Y);
        }

        if (num6 < 0.0)
        {
            List<object> list2 = new List<object>();
            for (int l = 0; l < list.Count; l++)
            {
                list2.Add(FlipGeo(list[list.Count - 1 - l]));
            }

            list = list2;
        }

        return list;
    }
}
#if false // Dekompilierungsprotokoll
381 Elemente im Cache
------------------
Auflösen: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.dll
------------------
Auflösen: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Einzelne Assembly gefunden: Microsoft.Extensions.DependencyInjection.Abstractions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
Laden von: C:\Users\TBraig\.nuget\packages\microsoft.extensions.dependencyinjection.abstractions\9.0.8\lib\net9.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll
------------------
Auflösen: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.dll
------------------
Auflösen: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Contracts, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: WiCAM.Pn4000.Contracts
------------------
Auflösen: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Collections, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Collections.dll
------------------
Auflösen: WiCAM.Pn4000.Materials, Version=1.30.3.31, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.Materials, Version=1.30.3.31, Culture=neutral, PublicKeyToken=null
Laden von: C:\u\pn\run\WiCAM.Pn4000.Materials.dll
------------------
Auflösen: System.Xml.ReaderWriter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Xml.ReaderWriter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Xml.ReaderWriter.dll
------------------
Auflösen: System.Xml.XmlSerializer, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Xml.XmlSerializer, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Xml.XmlSerializer.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Laden von: C:\u\pn\run\WiCAM.Pn4000.BendModel.dll
------------------
Auflösen: WiCAM.Pn4000.Comon, Version=3.27.1.14, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Comon, Version=3.27.1.14, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\u\pn\run\WiCAM.Pn4000.Comon.dll
------------------
Auflösen: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: WindowsBase, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\WindowsBase.dll
------------------
Auflösen: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ObjectModel, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ObjectModel.dll
------------------
Auflösen: System.ComponentModel.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.Primitives.dll
------------------
Auflösen: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: PresentationFramework, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\PresentationFramework.dll
------------------
Auflösen: System.Data.OleDb, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Data.OleDb, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Users\TBraig\.nuget\packages\system.data.oledb\5.0.0\ref\netstandard2.0\System.Data.OleDb.dll
------------------
Auflösen: System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Data.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Data.Common.dll
------------------
Auflösen: Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
Einzelne Assembly gefunden: Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed
Laden von: C:\Users\TBraig\.nuget\packages\newtonsoft.json\13.0.3\lib\net6.0\Newtonsoft.Json.dll
------------------
Auflösen: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Linq, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Linq.dll
------------------
Auflösen: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Einzelne Assembly gefunden: System.Xaml, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Xaml.dll
------------------
Auflösen: System.ComponentModel.TypeConverter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.ComponentModel.TypeConverter, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.ComponentModel.TypeConverter.dll
------------------
Auflösen: PresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Einzelne Assembly gefunden: PresentationCore, Version=9.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\PresentationCore.dll
------------------
Auflösen: System.Diagnostics.Process, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Diagnostics.Process, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Diagnostics.Process.dll
------------------
Auflösen: WiCAM.Pn4000.MdbImporter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Der Name "WiCAM.Pn4000.MdbImporter, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" wurde nicht gefunden.
------------------
Auflösen: WiCAM.Pn4000.Encodings, Version=2.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.Encodings, Version=2.0.3.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\u\pn\run\WiCAM.Pn4000.Encodings.dll
------------------
Auflösen: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Einzelne Assembly gefunden: WiCAM.Pn4000.BendModel.Loader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=7fe869221f42e6fe
Laden von: C:\u\pn\run\WiCAM.Pn4000.BendModel.Loader.dll
------------------
Auflösen: System.Console, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Console, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Console.dll
------------------
Auflösen: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Runtime.InteropServices, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.InteropServices.dll
------------------
Auflösen: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.IO.Packaging, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.IO.Packaging.dll
------------------
Auflösen: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Security.Permissions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Security.Permissions.dll
------------------
Auflösen: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Windows.Extensions, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Program Files\dotnet\packs\Microsoft.WindowsDesktop.App.Ref\9.0.4\ref\net9.0\System.Windows.Extensions.dll
------------------
Auflösen: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Security.AccessControl, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Security.AccessControl.dll
------------------
Auflösen: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Einzelne Assembly gefunden: System.Drawing.Common, Version=9.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
Laden von: C:\Users\TBraig\.nuget\packages\system.drawing.common\9.0.8\lib\net9.0\System.Drawing.Common.dll
------------------
Auflösen: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Einzelne Assembly gefunden: System.Drawing.Primitives, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Drawing.Primitives.dll
------------------
Auflösen: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null
Einzelne Assembly gefunden: System.Runtime.CompilerServices.Unsafe, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
Laden von: C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\9.0.4\ref\net9.0\System.Runtime.CompilerServices.Unsafe.dll
#endif

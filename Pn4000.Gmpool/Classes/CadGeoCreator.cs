using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.Gmpool;
using WiCAM.Pn4000.Materials;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal class CadGeoCreator
	{
		private readonly RestPlateFormType _formType;

		private readonly StockMaterialInfo _plate;

		public CadGeoCreator(RestPlateFormType formType, StockMaterialInfo plate)
		{
			this._formType = formType;
			this._plate = plate;
		}

		private CadGeoInfoBase Create(RestPlateFormType formType, StockMaterialInfo plate)
		{
			if (formType == RestPlateFormType.Circle)
			{
				return this.Round(plate);
			}
			return this.Rectangle(plate);
		}

		public void CreateCadGeo()
		{
			string str = ArchiveStructureHelper.Instance.PathByType(this._plate.PlName, 90, ArchiveFolderType.c2d);
			if (ApplicationConfigurationInfo.Instance.PlatesArchiv != null)
			{
				str = ArchiveStructureHelper.Instance.PathByType(this._plate.PlName, ApplicationConfigurationInfo.Instance.PlatesArchiv.Number, ArchiveFolderType.c2d);
			}
			CadGeoInfoBase cadGeoInfoBase = this.Create(this._formType, this._plate);
			(new CadGeoHelper()).Write(cadGeoInfoBase, str);
		}

		private CadGeoInfoBase Rectangle(StockMaterialInfo plate)
		{
			CadGeoInfoBase cadGeoInfoBase = new CadGeoInfoBase();
			cadGeoInfoBase.GeoElements.Add(new GeoLineInfo()
			{
				X0 = 0,
				Y0 = 0,
				X1 = plate.MaxX,
				Y1 = 0
			});
			cadGeoInfoBase.GeoElements.Add(new GeoLineInfo()
			{
				X0 = plate.MaxX,
				Y0 = 0,
				X1 = plate.MaxX,
				Y1 = plate.MaxY
			});
			cadGeoInfoBase.GeoElements.Add(new GeoLineInfo()
			{
				X0 = plate.MaxX,
				Y0 = plate.MaxY,
				X1 = 0,
				Y1 = plate.MaxY
			});
			cadGeoInfoBase.GeoElements.Add(new GeoLineInfo()
			{
				X0 = 0,
				Y0 = plate.MaxY,
				X1 = 0,
				Y1 = 0
			});
			return cadGeoInfoBase;
		}

		private CadGeoInfoBase Round(StockMaterialInfo plate)
		{
			CadGeoInfoBase cadGeoInfoBase = new CadGeoInfoBase();
			cadGeoInfoBase.GeoElements.Add(new GeoArcInfo()
			{
				BeginAngle = 0,
				EndAngle = 360,
				Direction = 1,
				Diameter = plate.MaxX,
				X0 = plate.MaxX / 2,
				Y0 = plate.MaxX / 2
			});
			return cadGeoInfoBase;
		}
	}
}
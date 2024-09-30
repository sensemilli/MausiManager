using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PpCommon;

namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal class LabelCreator
{
	private readonly JobInfo _job;

	private readonly string _jobPath;

	public LabelCreator(JobInfo job)
	{
		_job = job;
		_jobPath = Path.GetDirectoryName(_job.Path);
	}

	public void Create()
	{
		Logger.Info("LabelCreator : create");
		try
		{
			foreach (PlateInfo plate in _job.Plates)
			{
				ModifyPlateLabels(_job, plate);
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	private void ModifyPlateLabels(JobInfo job, PlateInfo plate)
	{
		SchbasInfo schbasInfo = SerializeHelper.DeserialiseFromXml<SchbasInfo>(Path.Combine(_jobPath, $"PLAT_{plate.PLATE_NUMBER:D3}.xml"));
		if (schbasInfo != null)
		{
			string content = ModifyLabelContent(plate, schbasInfo);
			IOHelper.FileWriteAllText(Path.Combine(_jobPath, $"PLPLB_{plate.PLATE_NUMBER}.LAB"), content);
		}
	}

	private string ModifyLabelContent(PlateInfo plate, SchbasInfo schbas)
	{
		List<LabelModifyReference> list = CreateReference(plate, schbas);
		PartLabelContentModifier partLabelContentModifier = new PartLabelContentModifier();
		StringBuilder stringBuilder = new StringBuilder(10000000);
		foreach (LabelModifyReference item in list)
		{
			stringBuilder.AppendLine(partLabelContentModifier.Modify(item, _jobPath));
		}
		return stringBuilder.ToString();
	}

	private List<LabelModifyReference> CreateReference(PlateInfo plate, SchbasInfo schbas)
	{
		List<LabelModifyReference> list = new List<LabelModifyReference>();
		SchbasPlateInfo schbasPlateInfo = schbas.Plates[0];
		LabelModifyReference labelModifyReference = new LabelModifyReference();
		foreach (SchbasPartInfo part in schbasPlateInfo.Parts)
		{
			if (labelModifyReference.Part == null)
			{
				InitializeReference(labelModifyReference, plate, part);
				continue;
			}
			if (IsPartEqual(labelModifyReference.Part.Part, part))
			{
				labelModifyReference.Amount++;
				continue;
			}
			list.Add(labelModifyReference);
			labelModifyReference = new LabelModifyReference();
			InitializeReference(labelModifyReference, plate, part);
		}
		list.Add(labelModifyReference);
		return list;
	}

	private void InitializeReference(LabelModifyReference reference, PlateInfo plate, SchbasPartInfo part)
	{
		reference.Job = plate.JobReference;
		PlatePartInfo part2 = plate.PlateParts.Find((PlatePartInfo x) => IsPartEqual(x.Part, part));
		reference.Part = part2;
		reference.Amount = 1;
	}

	private bool IsPartEqual(PartInfo part, SchbasPartInfo schPart)
	{
		if (!IsEqual(part.PART_NAME, schPart.DSBNA0))
		{
			return false;
		}
		if (!IsEqual(part.PART_ORDER, schPart.CSBFA0))
		{
			return false;
		}
		if (!IsEqual(part.PART_POSITION, schPart.CSBPN0))
		{
			return false;
		}
		if (!IsEqual(part.PART_REMARK, schPart.CSBBM0))
		{
			return false;
		}
		return true;
	}

	private bool IsEqual(string s1, string s2)
	{
		if (s1 == null)
		{
			s1 = string.Empty;
		}
		if (s2 == null)
		{
			s2 = string.Empty;
		}
		return s1.Equals(s2, StringComparison.CurrentCultureIgnoreCase);
	}
}

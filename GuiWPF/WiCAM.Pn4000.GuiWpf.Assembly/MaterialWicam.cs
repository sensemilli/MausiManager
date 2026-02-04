using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public class MaterialWicam
{
	private readonly IMaterialArt _material;

	public int Id => _material?.Number ?? (-1);

	public string Desc
	{
		get
		{
			if (_material != null)
			{
				return $"{_material.Number} - {_material.Name} - {_material.Description}";
			}
			return "-";
		}
	}

	public MaterialWicam(IMaterialArt material)
	{
		_material = material;
	}
}

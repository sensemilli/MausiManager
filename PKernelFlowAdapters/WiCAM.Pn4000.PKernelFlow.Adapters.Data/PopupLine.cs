using System.Collections.Generic;

namespace WiCAM.Pn4000.PKernelFlow.Adapters.Data;

public class PopupLine
{
	public int idx;

	public List<object> ctrls = new List<object>();

	public int typ;

	public int sel;

	public int pyn;

	public int id1;

	public int id2;

	public int id3;

	public int id4;

	public double real1;

	public double real2;

	public double real3;

	public double real4;

	public int inte1;

	public int inte2;

	public int inte3;

	public int inte4;

	public int box1_x;

	public int box1_y;

	public int box1_w;

	public int box1_h;

	public int box2_x;

	public int box2_y;

	public int box2_w;

	public int box2_h;

	public string text;

	public string ntext;

	public string htext;

	public string ltext;

	public List<string> multitext;

	public bool is_change;

	public bool is_disable;

	public bool is_presentation_mode;
}

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.UI;

public class DragDropEventArgs
{
	public object Sender { get; set; }

	public object MovedItem { get; set; }

	public int InsertIndex { get; set; }

	public DragDropEventArgs(object sender, object movedItem, int insertIndex)
	{
		this.Sender = sender;
		this.MovedItem = movedItem;
		this.InsertIndex = insertIndex;
	}
}

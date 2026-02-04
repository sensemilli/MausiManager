namespace WiCAM.Pn4000.GuiWpf.UiBasic;

public class DragDropEventArgs
{
	public object Sender { get; set; }

	public object MovedItem { get; set; }

	public int InsertIndex { get; set; }

	public DragDropEventArgs(object sender, object movedItem, int insertIndex)
	{
		Sender = sender;
		MovedItem = movedItem;
		InsertIndex = insertIndex;
	}
}

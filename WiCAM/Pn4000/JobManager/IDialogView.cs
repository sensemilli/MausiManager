namespace WiCAM.Pn4000.JobManager;

public interface IDialogView : IView
{
    bool? DialogResult();
    void DialogResult(bool? value);

    bool? ShowDialog();

	void Close();
}

namespace WiCAM.Pn4000.JobManager;

public interface IView
{
    object DataContext();
    void DataContext(object value);
}

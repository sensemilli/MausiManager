using System.Windows.Input;

namespace WiCAM.Pn4000.JobManager;

public interface IJobDataViewModel : IViewModel, IMachineStateObserver, IFilter, IFilterPlates, IFilterParts
{
	int JobsAmount { get; }

	double FontSize { get; set; }

	bool HasToIgnoreEnter { get; set; }

	ICommand DeleteJobCommand { get; }

	ICommand DeleteProducedJobsCommand { get; }
    ICommand AddToBlechOptCommand { get; }

    ICommand ProduceJobCommand { get; }

	ICommand ProducePlateCommand { get; }

	ICommand RejectPartCommand { get; }

	ICommand ReloadJobsCommand { get; }
    ICommand ReloadWithFinishedJobsCommand { get; }


    ICommand SaveProducedJobsCommand { get; }

	ICommand StornoPlateCommand { get; }

	ICommand ResetPlateCommand { get; }

	ICommand PrintJobLabelsCommand { get; }

	ICommand PrintPlateLabelsCommand { get; }

	ICommand PrintPartLabelsCommand { get; }
    ICommand ExportCommand { get; }
	ICommand FreigabeLoeschenCommand { get; }

    void LoadJobs();

	void SaveSettings();

	void ProduceJobUsingBarcode(string jobName);

	void ProducePlateUsingBarcode(string ncProgramNumber);
}

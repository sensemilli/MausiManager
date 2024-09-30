using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WiCAM.Pn4000.JobManager;

public class JobToolTip : ToolTip
{
	public static readonly DependencyProperty JobNameProperty = DependencyProperty.Register("JobName", typeof(string), typeof(JobToolTip));

	public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(JobToolTip));

	public static readonly DependencyProperty NoPlatesMessageProperty = DependencyProperty.Register("NoPlatesMessage", typeof(string), typeof(JobToolTip));

	public static readonly DependencyProperty StatusMessageProperty = DependencyProperty.Register("StatusMessage", typeof(string), typeof(JobToolTip));

	public static readonly DependencyProperty MessageBrushProperty = DependencyProperty.Register("MessageBrush", typeof(Brush), typeof(JobToolTip));

	public static readonly DependencyProperty PlateErrorsProperty = DependencyProperty.Register("PlateErrors", typeof(string), typeof(JobToolTip));

	public static readonly DependencyProperty JobProperty = DependencyProperty.Register("Job", typeof(JobInfo), typeof(JobToolTip), new PropertyMetadata(null, JobChanged));

	public string JobName
	{
		get
		{
			return (string)GetValue(JobNameProperty);
		}
		set
		{
			SetValue(JobNameProperty, value);
		}
	}

	public string ErrorMessage
	{
		get
		{
			return (string)GetValue(ErrorMessageProperty);
		}
		set
		{
			SetValue(ErrorMessageProperty, value);
		}
	}

	public string NoPlatesMessage
	{
		get
		{
			return (string)GetValue(NoPlatesMessageProperty);
		}
		set
		{
			SetValue(NoPlatesMessageProperty, value);
		}
	}

	public string StatusMessage
	{
		get
		{
			return (string)GetValue(StatusMessageProperty);
		}
		set
		{
			SetValue(StatusMessageProperty, value);
		}
	}

	public Brush MessageBrush
	{
		get
		{
			return (Brush)GetValue(MessageBrushProperty);
		}
		set
		{
			SetValue(MessageBrushProperty, value);
		}
	}

	public string PlateErrors
	{
		get
		{
			return (string)GetValue(PlateErrorsProperty);
		}
		set
		{
			SetValue(PlateErrorsProperty, value);
		}
	}

	public JobInfo Job
	{
		get
		{
			return (JobInfo)GetValue(JobProperty);
		}
		set
		{
			SetValue(JobProperty, value);
		}
	}

	public JobToolTip()
	{
		JobName = string.Empty;
	}

	private static void JobChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is JobToolTip jobToolTip)
		{
			jobToolTip.JobName = jobToolTip.Job.JOB_DATA_1;
			if (double.IsNaN(jobToolTip.Job.ProductionProgress))
			{
				jobToolTip.ErrorMessage = jobToolTip.NoPlatesMessage;
				jobToolTip.MessageBrush = Brushes.Red;
			}
			else if (string.IsNullOrEmpty(jobToolTip.Job.JobStatusMessage))
			{
				jobToolTip.ErrorMessage = string.Format(CultureInfo.InvariantCulture, jobToolTip.StatusMessage, jobToolTip.Job.Plates.Count, jobToolTip.Job.Parts.Count).Replace("|", Environment.NewLine);
				jobToolTip.MessageBrush = Brushes.Black;
			}
			else
			{
				jobToolTip.ErrorMessage = string.Format(CultureInfo.InvariantCulture, jobToolTip.StatusMessage, jobToolTip.Job.Plates.Count, jobToolTip.Job.Parts.Count).Replace("|", Environment.NewLine) + Environment.NewLine + Environment.NewLine + jobToolTip.Job.JobStatusMessage;
				jobToolTip.MessageBrush = Brushes.Red;
			}
		}
	}
}

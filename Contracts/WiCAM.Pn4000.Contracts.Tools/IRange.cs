namespace WiCAM.Pn4000.Contracts.Tools;

public interface IRange
{
	double Start { get; set; }

	double End { get; set; }

	double Length => this.End - this.Start;

	double Center => (this.Start + this.End) * 0.5;
}

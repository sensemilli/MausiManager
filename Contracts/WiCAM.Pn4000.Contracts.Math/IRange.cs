using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Math;

public interface IRange<T> where T : IVector
{
	T Start { get; set; }

	T End { get; set; }

	double Length => this.End.Subtract(this.Start).Length;

	T Center => (T)this.Start.Add(this.End).Multiply(0.5);
}

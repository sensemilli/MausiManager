namespace WiCAM.Pn4000.Contracts.Factorys;

public interface IModelFactory
{
	T Resolve<T>();

	T Resolve<T>(params object[] parameter);

	T Create<T>();

	T Create<T>(params object[] parameter);
}

namespace WiCAM.Pn4000.Contracts.BendDataBase;

public enum BendTableReturnValues
{
	OK = 0,
	DEFAULT_VALUE = 1,
	INTERPOLATED = 2,
	EXACT_VALUE = 3,
	NEAREST_RADIUS = 4,
	USER_DEFINED = 5,
	NO_VALUE_FOUND = 10
}

namespace Tekla.Structures
{
	public interface IConnection : ITransaction, IRunMacro, IPicker, ISelection, ISelectObject
	{
		bool IsActive { get; }
	}
}

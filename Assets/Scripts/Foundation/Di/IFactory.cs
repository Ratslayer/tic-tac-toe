namespace BB
{
	public interface IFactory<T> 
	{
		T Create();
	}
	public interface IDiFactory<T>
	{
		T Create(IResolver resolver);
	}
}
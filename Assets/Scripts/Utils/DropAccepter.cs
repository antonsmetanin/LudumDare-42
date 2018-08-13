namespace Utils
{
	public interface IDropAccepter<in T>
	{
		void Accept(T t, bool simulate = false);
	}
}

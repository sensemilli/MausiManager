namespace WiCAM.Pn4000.Contracts.TrumpfL26Service;

public interface IL26CachingService
{
	void Add(ICacheItem item);

	ICacheItem Get(string identifier);

	void Remove(string identifier);
}

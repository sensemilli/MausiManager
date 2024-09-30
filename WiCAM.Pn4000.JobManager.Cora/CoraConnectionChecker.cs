using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.JobManager.Cora;

internal class CoraConnectionChecker
{
	public async Task<bool> IsCoraReachable(string url)
	{
		string requestUri = url + "/coraconnection";
		return (await new HttpClient().GetAsync(requestUri)).StatusCode == (HttpStatusCode)418;
	}
}

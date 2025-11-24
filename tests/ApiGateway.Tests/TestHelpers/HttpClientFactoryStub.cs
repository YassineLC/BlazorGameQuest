using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiGateway.Tests.TestHelpers
{
  internal class FakeHandler : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var json = "{ \"Rooms\": [], \"Seed\": 123 }";
      var resp = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
      return Task.FromResult(resp);
    }
  }

  internal class SimpleHttpClientFactory : IHttpClientFactory
  {
    private readonly HttpClient _client;
    public SimpleHttpClientFactory()
    {
      _client = new HttpClient(new FakeHandler(), disposeHandler: false)
      {
        BaseAddress = new Uri("http://localhost")
      };
    }
    public HttpClient CreateClient(string name) => _client;
  }
}

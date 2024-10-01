using FluentAssertions;
using NUnit.Framework;

namespace CHG.Extensions.Security.Txt.IntegrationTests;

[TestFixture]
public class DemoWebApplication
{
	private readonly DemoWebApplicationFactory _webApplicationFactory;
	private HttpClient _httpClient;

	public DemoWebApplication()
	{
		_webApplicationFactory = new DemoWebApplicationFactory();
	}

	[SetUp]
	public void Setup()
	{
		_httpClient = _webApplicationFactory.CreateClient();
	}

	[OneTimeTearDown]
	public void ClassTearDown()
	{
		_webApplicationFactory?.Dispose();
	}

	[TearDown]
	public void TearDown()
	{
		_httpClient?.Dispose();
	}

	[Test]
	public async Task Check_WellKnown_Path_Redirect()
	{
		var response = await _httpClient.GetAsync("/security.txt");

		response.StatusCode.Should().Be(System.Net.HttpStatusCode.MovedPermanently);
		response.Headers.Location.Should().Be("https://securitytxt.org/.well-known/security.txt");
	}

	[Test]
	public async Task Check_Root_Path_Redirect()
	{
		var response = await _httpClient.GetAsync("/.well-known/security.txt");

		response.StatusCode.Should().Be(System.Net.HttpStatusCode.MovedPermanently);
		response.Headers.Location.Should().Be("https://securitytxt.org/.well-known/security.txt");
	}
}
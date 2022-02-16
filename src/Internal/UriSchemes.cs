namespace CHG.Extensions.Security.Txt.Internal;

internal static class UriSchemes
{
	/// <summary>
	/// Gets the default uri schemes to query information over a network
	/// </summary>
	/// <value>The default schemes to query information over a network.</value>
	public static string[] DefaultQuerySchemes { get; } = new[] {
			Uri.UriSchemeFile,
			Uri.UriSchemeFtp,
			Uri.UriSchemeGopher,
			Uri.UriSchemeHttp,
			Uri.UriSchemeHttps,
			Uri.UriSchemeNetPipe,
			Uri.UriSchemeNetTcp
		};
}

namespace CHG.Extensions.Security.Txt.Internal
{
	internal enum UriValidationOptions
	{
		/// <summary>
		/// Allows an unsecure uri scheme like http://
		/// </summary>
		AllowUnsecureScheme,

		/// <summary>
		/// Requires a secure uri scheme like https://
		/// </summary>
		RequiresSecureScheme
	}
}
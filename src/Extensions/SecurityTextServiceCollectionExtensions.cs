using CHG.Extensions.Security.Txt;


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up Security Text in an <see cref="IServiceCollection" />.
/// </summary>
public static class SecurityTextServiceCollectionExtensions
{
	/// <summary>
	/// Adds the security text configuration.
	/// </summary>
	/// <param name="services">The services.</param>
	/// <param name="builderSetup">Delegate to define the configuration.</param>
	/// <returns></returns>
	/// <exception cref="System.ArgumentNullException">
	/// services
	/// or
	/// builderSetup
	/// </exception>
	public static IServiceCollection AddSecurityText(this IServiceCollection services, Action<SecurityTextBuilder> builderSetup)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (builderSetup == null)
			throw new ArgumentNullException(nameof(builderSetup));

		var builder = new SecurityTextBuilder();
		builderSetup(builder);

		services.AddSingleton(builder.GetContainer());

		return services;
	}
}

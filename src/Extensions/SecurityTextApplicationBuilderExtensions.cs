using CHG.Extensions.Security.Txt.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Pipeline extension methods for adding SecurityText
/// </summary>
public static class SecurityTextApplicationBuilderExtensions
{
	private const string MAIN_URL = "/.well-known/security.txt";
	private const string FALLBACK_URL = "/security.txt";

	/// <summary>
	/// Adds SecurityText to the pipeline.
	/// </summary>
	/// <param name="app">The application.</param>
	/// <returns></returns>
	public static IApplicationBuilder UseSecurityText(this IApplicationBuilder app)
	{
		return UseSecurityText(app, true);
	}

	/// <summary>
	/// Adds SecurityText to the pipeline.
	/// </summary>
	/// <param name="app">The application.</param>
	/// <param name="registerRedirect">True to redirect "/security.txt" to "/.well-known/security.txt"</param>
	/// <returns></returns>
	public static IApplicationBuilder UseSecurityText(this IApplicationBuilder app, bool registerRedirect)
	{
		if (app == null)
			throw new ArgumentNullException(nameof(app));

		var container = app.ApplicationServices.GetService<SecurityTextContainer>();

		if (container == null)
			throw new InvalidOperationException($"Please call {nameof(SecurityTextServiceCollectionExtensions.AddSecurityText)}() within ConfigureServices() first, before using {nameof(UseSecurityText)}()!");

		if (container.ValidateValues)
			container.Validate();

		app.Map(MAIN_URL, builder =>
		{
			builder.Run(async context =>
			{
				if (container.HasRedirect)
				{
					context.Response.Redirect(container.RedirectUri, true);
				}
				else
				{
					context.Response.ContentType = "text/plain";
					await context.Response.WriteAsync(container.Build());
				}
			});
		});

		if (registerRedirect)
		{
			app.Map(FALLBACK_URL, builder =>
			{
				builder.Run(async context =>
				{
					if (container.HasRedirect)
					{
						context.Response.Redirect(container.RedirectUri, true);
					}
					else
					{
						context.Response.Redirect(MAIN_URL, true);
						await context.Response.WriteAsync(container.Build());
					}
				});
			});
		}

		return app;
	}
}

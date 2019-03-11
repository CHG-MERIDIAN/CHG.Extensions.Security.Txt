using System;
using System.Linq;
using System.Text;

namespace CHG.Extensions.Security.Txt.Internal
{
	/// <summary>
	/// Container for holding all defined information
	/// </summary>
	public class SecurityTextContainer
	{
		private const string COMMENT_PREFIX = "# ";

		/// <summary>
		/// Builds the security information text
		/// </summary>
		/// <returns></returns>
		public string Build()
		{
			if (!string.IsNullOrEmpty(Text))
			{
				return Text;
			}
			else
			{
				var builder = new StringBuilder();

				AddIntroduction(builder);
				AddContact(builder);
				AddEncryption(builder);
				AddSignature(builder);
				AddPolicy(builder);
				AddAcknowledgments(builder);
				AddHiring(builder);
				AddPermission(builder);

				return builder.ToString().TrimEnd();
			}
		}

		/// <summary>
		/// Validates the values.
		/// </summary>
		public void Validate()
		{
			// Don't validate information set by text
			if (!string.IsNullOrEmpty(Text))
				return;

			ValidateContact();
			ValidateAcknowledgments();
			ValidateEncryption();
			ValidateHiring();
			ValidatePermission();
			ValidatePolicy();
			ValidateSignature();
		}

		private void AddPermission(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Permission))
				AppendLine(builder, $"Permission: {Permission}");
		}

		private void AddHiring(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Hiring))
				AppendLine(builder, $"Hiring: {Hiring}");
		}

		private void AddAcknowledgments(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Acknowledgments))
				AppendLine(builder, $"Acknowledgments: {Acknowledgments}");
		}

		private void AddPolicy(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Policy))
				AppendLine(builder, $"Policy: {Policy}");
		}

		private void AddSignature(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Signature))
				AppendLine(builder, $"Signature: {Signature}");
		}

		private void AddEncryption(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Encryption))
				AppendLine(builder, $"Encryption: {Encryption}");
		}

		private void AddContact(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Contact))
				builder.Append(ExtractMultiple("Contact: ", Contact, NewLineString));
		}

		private void AddIntroduction(StringBuilder builder)
		{
			if (!string.IsNullOrEmpty(Introduction))
				AppendLine(builder, CreateComment(Introduction));
		}

		/// <summary>
		/// Validates the acknowledgments.
		/// </summary>
		private void ValidateAcknowledgments()
		{
			if (!string.IsNullOrEmpty(Acknowledgments))
				ValidateUrl(Acknowledgments, nameof(Acknowledgments));
		}

		/// <summary>
		/// Validates the Encryption.
		/// </summary>
		private void ValidateEncryption()
		{
			if (!string.IsNullOrEmpty(Encryption))
				ValidateUri(Encryption, nameof(Encryption));
		}

		/// <summary>
		/// Validates the Hiring.
		/// </summary>
		private void ValidateHiring()
		{
			if (!string.IsNullOrEmpty(Hiring))
				ValidateUri(Hiring, nameof(Hiring), UriSchemes.DefaultQuerySchemes);
		}

		/// <summary>
		/// Validates the Policy.
		/// </summary>
		private void ValidatePolicy()
		{
			if (!string.IsNullOrEmpty(Policy))
				ValidateUri(Policy, nameof(Policy), UriSchemes.DefaultQuerySchemes);
		}

		/// <summary>
		/// Validates the Signature.
		/// </summary>
		private void ValidateSignature()
		{
			if (!string.IsNullOrEmpty(Signature))
				ValidateUri(Signature, nameof(Signature), UriSchemes.DefaultQuerySchemes);
		}

		/// <summary>
		/// Validates the Permission.
		/// </summary>
		private void ValidatePermission()
		{
			if (!string.IsNullOrEmpty(Permission) && !string.Equals(Permission, "none", StringComparison.OrdinalIgnoreCase))
				throw new InvalidSecurityInformationException($"The value '{Permission}' for the {nameof(Permission)} field is invalid! This field MUST have a value which is REQUIRED to be set to the string \"none\". Other values MUST NOT be used.");
		}

		/// <summary>
		/// Validates the contact information.
		/// </summary>
		private void ValidateContact()
		{
			if (!string.IsNullOrEmpty(Contact))
			{
				foreach (var value in Contact.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
					ValidateContact(value);
			}
			else
			{
				throw new InvalidSecurityInformationException("The \"Contact: \" directive MUST always be present in a security.txt file.");
			}
		}

		/// <summary>
		/// Validates the contact information.
		/// </summary>
		private void ValidateContact(string value)
		{
			if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			{
				if (!IsValidUrl(value, UriValidationOptions.RequiresSecureScheme))
					throw new InvalidSecurityInformationException($"The value '{value}' for the {nameof(Contact)} field is not a valid url! It must begin with \"https://\".");
			}
			else if (value.Contains("@") || value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
			{
				if (!value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
					throw new InvalidSecurityInformationException($"The value '{value}' for the {nameof(Contact)} field was recognized as email address but does not start with \"mailto\" URI scheme.");
				else if (!IsValidEmail(value))
					throw new InvalidSecurityInformationException($"The value '{value}' for the {nameof(Contact)} field is not a valid email address.");
			}
			else if (!value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidSecurityInformationException($"The value '{value}' for the {nameof(Contact)} field is not a valid value. Please provide an url, email address or phone number.");
			}
		}

		private void ValidateUri(string value, string fieldName, params string[] uriSchemeRestriction)
		{
			if (!IsValidUri(value, uriSchemeRestriction))
				throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field is not a valid uri!");

			if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !IsValidUrl(value, UriValidationOptions.RequiresSecureScheme))
				throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field is not a valid url! It must be begin with \"https://\".");
		}

		private bool IsValidUri(string value, params string[] uriSchemeRestriction)
		{
			if (Uri.TryCreate(value, UriKind.Absolute, out var validatedUri))
			{
				if (uriSchemeRestriction?.Length == 0)
					return true;

				return uriSchemeRestriction.Contains(validatedUri.Scheme);
			}

			return false;
		}

		private void ValidateUrl(string value, string fieldName)
		{
			if (!IsValidUrl(value, UriValidationOptions.AllowUnsecureScheme))
				throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field is not a valid url!");
		}

		private bool IsValidUrl(string value, UriValidationOptions options)
		{
			if (Uri.TryCreate(value, UriKind.Absolute, out var validatedUri))
			{
				if (options == UriValidationOptions.AllowUnsecureScheme)
					return validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps;

				return validatedUri.Scheme == Uri.UriSchemeHttps;
			}

			return false;
		}

		private bool IsValidEmail(string email)
		{
			if (email.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
				email = email.Substring(7);

			try
			{
				var address = new System.Net.Mail.MailAddress(email);
				return address != null;
			}
			catch
			{
				return false;
			}
		}

		private string CreateComment(string value)
		{
			return CreateComment(value, NewLineString);
		}

		/// <summary>
		/// Creates a comment from the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal static string CreateComment(string value, string newLineString)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			return COMMENT_PREFIX +
				value.Replace("\r\n", "\n")
				.Replace("\r", "\n")
				.Replace("\n", newLineString + COMMENT_PREFIX);
		}

		private static string ExtractMultiple(string directive, string value, string newLineString)
		{
			var values = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var builder = new StringBuilder();

			foreach (var item in values)
				builder.Append($"{directive}{item}").Append(newLineString);

			return builder.ToString();
		}

		private void AppendLine(StringBuilder builder, string value)
		{
			builder.Append(value).Append(NewLineString);
		}

		/// <summary>
		/// Gets or sets the whole security text.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the contact information.        
		/// </summary>
		/// <example>mailto:security@example.com or tel:+1-201-555-0123 or https://example.com/security-contact.html</example>        
		public string Contact { get; set; }

		/// <summary>
		/// Gets or sets the encryption information.
		/// </summary>
		/// <example>https://example.com/pgp-key.txt</example>
		public string Encryption { get; set; }

		/// <summary>
		/// Gets or sets the hiring directive, this is for linking to the vendor’s security-related job positions..
		/// </summary>
		/// <example>https://example.com/jobs.html</example>
		public string Hiring { get; set; }

		/// <summary>
		/// Gets or sets the permission directive,  this field MUST have a value which is REQUIRED to be set to the string "none".
		/// </summary>
		/// <example>none</example>
		public string Permission { get; set; }

		/// <summary>
		/// Gets or sets the acknowledgments.
		/// </summary>
		/// <example>https://example.com/hall-of-fame.html</example>
		public string Acknowledgments { get; set; }

		/// <summary>
		/// Gets or sets the policy.
		/// </summary>        
		/// <example>https://example.com/security-policy.html</example>
		public string Policy { get; set; }

		/// <summary>
		/// Gets or sets the signature.
		/// </summary>
		/// <example>https://example.com/.well-known/security.txt.sig</example>
		public string Signature { get; set; }

		/// <summary>
		/// Gets or sets the introduction.
		/// </summary>
		/// <example>The ACME Security information.</example>
		public string Introduction { get; set; }

		/// <summary>
		/// Gets or sets whether the values should be validated
		/// </summary>
		public bool ValidateValues { get; set; } = true;

		/// <summary>
		/// Gets or sets the string to define a new line
		/// </summary>
		public string NewLineString { get; set; } = Environment.NewLine;

	}
}

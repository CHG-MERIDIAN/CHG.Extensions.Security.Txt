using System;
using System.Collections.Generic;
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
        /// Builds the security information text
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            if (!string.IsNullOrEmpty(Text))
                return Text;
            else
            {
                var builder = new StringBuilder();

                if (!string.IsNullOrEmpty(Introduction))
                    builder.AppendLine(CreateComment(Introduction));

                if (!string.IsNullOrEmpty(Contact))
                    builder.Append(ExtractMultiple("Contact: ", Contact));

                if (!string.IsNullOrEmpty(Encryption))
                    builder.AppendLine($"Encryption: {Encryption}");

                if (!string.IsNullOrEmpty(Signature))
                    builder.AppendLine($"Signature: {Signature}");

                if (!string.IsNullOrEmpty(Policy))
                    builder.AppendLine($"Policy: {Policy}");

                if (!string.IsNullOrEmpty(Acknowledgments))
                    builder.AppendLine($"Acknowledgments: {Acknowledgments}");

                if (!string.IsNullOrEmpty(Hiring))
                    builder.AppendLine($"Hiring: {Hiring}");

                if (!string.IsNullOrEmpty(Permission))
                    builder.AppendLine($"Permission: {Permission}");

                return builder.ToString().TrimEnd();
            }
        }

        /// <summary>
        /// Validates the values.
        /// </summary>
        public void Validate()
        {
            if (!string.IsNullOrEmpty(Contact))
                ValidateContact();
            else
                throw new InvalidSecurityInformationException("The \"Contact: \" directive MUST always be present in a security.txt file.");

            if (!string.IsNullOrEmpty(Acknowledgments))
                ValidateAcknowledgments();

            if (!string.IsNullOrEmpty(Encryption))
                ValidateEncryption();

            if (!string.IsNullOrEmpty(Hiring))
                ValidateHiring();

            if (!string.IsNullOrEmpty(Permission))
                ValidatePermission();

            if (!string.IsNullOrEmpty(Policy))
                ValidatePolicy();

            if (!string.IsNullOrEmpty(Signature))
                ValidateSignature();
        }

        /// <summary>
        /// Validates the acknowledgments.
        /// </summary>
        private void ValidateAcknowledgments()
        {
            CheckValidUrl(Acknowledgments, nameof(Acknowledgments));
        }

        /// <summary>
        /// Validates the Encryption.
        /// </summary>
        private void ValidateEncryption()
        {
            ValidateUri(Encryption, nameof(Encryption), true);
        }

        /// <summary>
        /// Validates the Hiring.
        /// </summary>
        private void ValidateHiring()
        {
            ValidateUri(Hiring, nameof(Hiring), false);         
        }

        /// <summary>
        /// Validates the Policy.
        /// </summary>
        private void ValidatePolicy()
        {
            ValidateUri(Policy, nameof(Policy), false);
        }

        /// <summary>
        /// Validates the Signature.
        /// </summary>
        private void ValidateSignature()
        {
            ValidateUri(Signature, nameof(Signature), false);
        }

        private void ValidateUri(string value, string fieldName, bool allowAllUriSchemes)
        {
            CheckValidUri(value, fieldName, allowAllUriSchemes);

            if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !IsValidInternetUrl(value, false))
                throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field is not a valid url! It must be begin with \"https://\".");
        }

        /// <summary>
        /// Validates the Permission.
        /// </summary>
        private void ValidatePermission()
        {
            if (!string.Equals(Permission, "none", StringComparison.OrdinalIgnoreCase))
                throw new InvalidSecurityInformationException($"The value '{Permission}' for the {nameof(Permission)} field is invalid! This field MUST have a value which is REQUIRED to be set to the string \"none\". Other values MUST NOT be used.");
        }

        /// <summary>
        /// Validates the contact information.
        /// </summary>
        private void ValidateContact()
        {
            foreach (var value in Contact.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                ValidateContact(value);
        }

        /// <summary>
        /// Validates the contact information.
        /// </summary>
        private void ValidateContact(string value)
        {
            if (value.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsValidInternetUrl(value, false))
                    throw new InvalidSecurityInformationException($"The value '{value}' for the {nameof(Contact)} field is not a valid url! It must be begin with \"https://\".");
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

        private void CheckValidUrl(string value, string fieldName)
        {
            if (!IsValidInternetUrl(value, true))
                throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field, is not a valid url!");
        }

        private void CheckValidUri(string value, string fieldName, bool allowAllUriSchemes)
        {
            if (!IsValidUri(value, allowAllUriSchemes))
                throw new InvalidSecurityInformationException($"The value '{value}' for the {fieldName} field, is not a valid uri!");
        }

        private bool IsValidInternetUrl(string value, bool allowNonSecureScheme)
        {
            Uri validatedUri;

            if (Uri.TryCreate(value, UriKind.Absolute, out validatedUri))
            {
                return (allowNonSecureScheme && validatedUri.Scheme == Uri.UriSchemeHttp) || validatedUri.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        private bool IsValidUri(string value, bool allowAllUriSchemes)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var validatedUri))
            {
                return allowAllUriSchemes ||
                    validatedUri.Scheme == Uri.UriSchemeFile ||
                    validatedUri.Scheme == Uri.UriSchemeFtp ||
                    validatedUri.Scheme == Uri.UriSchemeGopher ||
                    validatedUri.Scheme == Uri.UriSchemeHttp ||
                    validatedUri.Scheme == Uri.UriSchemeHttps ||
                    validatedUri.Scheme == Uri.UriSchemeNetPipe ||
                    validatedUri.Scheme == Uri.UriSchemeNetTcp;
            }

            return false;
        }

        private bool IsValidEmail(string email)
        {
            if (email.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                email = email.Substring(7);
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a comment from the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        internal static string CreateComment(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return COMMENT_PREFIX +
                value.Replace("\r\n", "~~")
                .Replace("\r", "~~")
                .Replace("\n", "~~")
                .Replace("~~", Environment.NewLine + COMMENT_PREFIX);
        }

        private static string ExtractMultiple(string directive, string value)
        {
            var values = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();

            foreach (var item in values)
                builder.AppendLine($"{directive}{item}");

            return builder.ToString();
        }
    }
}

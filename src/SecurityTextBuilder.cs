using System;
using System.IO;
using CHG.Extensions.Security.Txt.Internal;
using Microsoft.Extensions.FileProviders;

namespace CHG.Extensions.Security.Txt
{
    /// <summary>
    /// Builder class to define the security text information
    /// </summary>
    public class SecurityTextBuilder
    {        
        private readonly SecurityTextContainer _container = new SecurityTextContainer();        

        /// <summary>
        /// Reads the security text from file.
        /// </summary>
        /// <param name="fileInfo">The file information.</param>
        /// <remarks>No content validation is done.</remarks>
        public void ReadFromFile(IFileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            if (!fileInfo.Exists)
                throw new ArgumentException($"Defined file {fileInfo.PhysicalPath} does not exist.", nameof(fileInfo));

            // StreamReader disposes the stream also
            using(var reader = new StreamReader(fileInfo.CreateReadStream()))
                ReadFromFile(reader);            
        }       

        /// <summary>
        /// Reads the security text from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <remarks>No content validation is done.</remarks>
        public void ReadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var fileInfo = new System.IO.FileInfo(filePath);

            if (!fileInfo.Exists)
                throw new ArgumentException($"Defined file {fileInfo.Name} does not exist.", nameof(filePath));

            using (var stream = fileInfo.OpenText())
                ReadFromFile(stream);
        }

        /// <summary>
        /// Reads the security text from configuration.
        /// </summary>
        /// <param name="section">The section containing the key/value pairs.</param>        
        public SecurityTextBuilder ReadFromConfiguration(Microsoft.Extensions.Configuration.IConfigurationSection section)
        {
            foreach(var subKey in section.GetChildren())
            {
                switch (subKey.Key.ToLower())
                {
                    case "contact":
                        SetContact(subKey.Value);
                        break;
                    case "encryption":
                        SetEncryption(subKey.Value);
                        break;
                    case "signature":
                        SetSignature(subKey.Value);
                        break;
                    case "policy":
                        SetPolicy(subKey.Value);
                        break;
                    case "acknowledgments":
                        SetAcknowledgments(subKey.Value);
                        break;
                    case "hiring":
                        SetHiring(subKey.Value);
                        break;
                    case "introduction":
                        SetIntroduction(subKey.Value);
                        break;                        
                    case "validatevalues":
                        SetValidateValues(bool.Parse(subKey.Value));
                        break;
                    case "permission":
                        SetPermission(subKey.Value);
                        break;
                    default:
                        break;
                }
            }

            return this; 
        }

        /// <summary>
        /// Sets whether the values should be validated afterwards.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetValidateValues(bool value)
        {
            _container.ValidateValues = value;
            return this;
        }

        /// <summary>
        /// Disables the value validation
        /// </summary>
        /// <returns></returns>
        public SecurityTextBuilder DisableValidation()
        {
            return SetValidateValues(false);
        }

        /// <summary>
        /// Sets the value for Contact.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetContact(string value)
        {
            _container.Contact = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Hiring.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetHiring(string value)
        {
            _container.Hiring = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Permission.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetPermission(string value)
        {
            _container.Permission = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Acknowledgment.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetAcknowledgments(string value)
        {
            _container.Acknowledgments = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Policy.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetPolicy(string value)
        {
            _container.Policy = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Signature.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetSignature(string value)
        {
            _container.Signature = value;
            return this;
        }

        /// <summary>
        /// Sets the value for Encryption.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetEncryption(string value)
        {
            _container.Encryption = value;
            return this;
        }

        /// <summary>
        /// Sets the text to use it as complete information text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>No content validation is done.</remarks>
        public SecurityTextBuilder SetText(string value)
        {
            _container.Text = value;
            return this;
        }

        /// <summary>
        /// Sets the value for security text introduction.
        /// </summary>
        /// <param name="value">The introduction.</param>
        /// <returns></returns>
        public SecurityTextBuilder SetIntroduction(string value)
        {
            _container.Introduction = value;
            return this;
        }
        
        /// <summary>
        /// Builds the security information text
        /// </summary>
        /// <returns></returns>
        public SecurityTextContainer GetContainer()
        {
            return _container;
        }

        private void ReadFromFile(StreamReader stream)
        {
            SetText(stream.ReadToEnd());
        }
    }
}

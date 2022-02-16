using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Moq;
using NUnit.Framework;



namespace CHG.Extensions.Security.Txt.Tests;

[TestFixture]
public class SecurityTextBuilderTests
{
	protected SecurityTextBuilder _builder;

	[SetUp]
	public virtual void Setup()
	{
		_builder = new SecurityTextBuilder();
	}

	public class ReadFromFileMethod : SecurityTextBuilderTests
	{
		private Mock<IFileInfo> _fileInfo;

		public override void Setup()
		{
			base.Setup();

			_fileInfo = new Mock<IFileInfo>();
		}

		[Test]
		public void Throws_When_File_Does_Not_Exist()
		{
			_fileInfo.Setup(i => i.Exists).Returns(false);

			Action action = () => _builder.ReadFromFile(_fileInfo.Object);

			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void Throws_When_FileInfo_Is_Null()
		{
			Action action = () => _builder.ReadFromFile((IFileInfo)null);

			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void Throws_When_FilePath_Does_Not_Exist()
		{
			Action action = () => _builder.ReadFromFile(@"c:\thisisatestpath\securityinfo.text");

			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void Throws_When_FilePath_Is_Null()
		{
			Action action = () => _builder.ReadFromFile((string)null);

			action.Should().Throw<ArgumentException>();
		}

		[Test]
		public void Reads_All_Content_From_FileInfo()
		{
			var expected = "this is a long text\r\nwith linebreak";
			_fileInfo.Setup(i => i.Exists).Returns(true);
			_fileInfo.Setup(i => i.CreateReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(expected)));

			_builder.ReadFromFile(_fileInfo.Object);

			_builder.GetContainer().Build().Should().Be(expected);
		}

		[Test]
		public void Reads_All_Content_From_FilePath()
		{
			var expected = "this is a long text\r\nwith linebreak";

			var fileName = System.IO.Path.GetTempFileName();
			try
			{
				File.WriteAllText(fileName, expected);

				_builder.ReadFromFile(fileName);

				_builder.GetContainer().Build().Should().Be(expected);
			}
			finally
			{
				File.Delete(fileName);
			}
		}
	}

	public class ReadFromConfigurationMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Reads_Contact_Information()
		{
			var config = CreateConfig("Contact", "mailto:security@example.com");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Contact: mailto:security@example.com");
		}

		[Test]
		public void Reads_Encryption_Information()
		{
			var config = CreateConfig("Encryption", "dns:5d2d3ceb7abe552344276d47d36._openpgpkey.example.com?type=OPENPGPKEY");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Encryption: dns:5d2d3ceb7abe552344276d47d36._openpgpkey.example.com?type=OPENPGPKEY");
		}

		[Test]
		public void Reads_Signature_Information()
		{
			var config = CreateConfig("Signature", "https://example.com/.well-known/security.txt.sig");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Signature: https://example.com/.well-known/security.txt.sig");
		}

		[Test]
		public void Reads_Policy_Information()
		{
			var config = CreateConfig("Policy", "https://example.com/security-policy.html");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Policy: https://example.com/security-policy.html");
		}

		[Test]
		public void Reads_Acknowledgments_Information()
		{
			var config = CreateConfig("Acknowledgments", "https://example.com/hall-of-fame.html");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Acknowledgments: https://example.com/hall-of-fame.html");
		}

		[Test]
		public void Reads_Hiring_Information()
		{
			var config = CreateConfig("Hiring", "https://example.com/jobs.html");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Hiring: https://example.com/jobs.html");
		}

		[Test]
		public void Reads_Permission_Information()
		{
			var config = CreateConfig("Permission", "none");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("Permission: none");
		}

		[Test]
		public void Reads_ValidateValues_Information()
		{
			var config = CreateConfig("ValidateValues", "false");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().ValidateValues.Should().Be(false);
		}

		[Test]
		public void Reads_Introduction_Information()
		{
			var config = CreateConfig("Introduction", "The ACME Security information.");

			_builder.ReadFromConfiguration(config);

			_builder.GetContainer().Build().Should().Be("# The ACME Security information.");
		}

		private static IConfigurationSection CreateConfig(string key, string value)
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new List<KeyValuePair<string, string>>
				{
						new KeyValuePair<string, string>("SecurityText:"+key, value),
				})
				.Build()
				.GetSection("SecurityText");
		}
	}

	public class SetContactMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Contact_Value()
		{
			_builder.SetContact("mailto:security@example.com")
				.GetContainer().Build().Should().Be("Contact: mailto:security@example.com");
		}

		[Test]
		public void Can_Set_Multiple_Values()
		{
			_builder.SetContact("mailto:security@example.com;tel:+1-201-555-0123")
				.GetContainer().Build().Should().Be("Contact: mailto:security@example.com\r\nContact: tel:+1-201-555-0123");
		}
	}

	public class SetAcknowledgmentsMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Acknowledgments_Value()
		{
			_builder.SetAcknowledgments("https://example.com/hall-of-fame.html")
				.GetContainer().Build().Should().Be("Acknowledgments: https://example.com/hall-of-fame.html");
		}
	}

	public class SetEncryptionMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Encryption_Value()
		{
			_builder.SetEncryption("https://example.com/pgp-key.txt")
				.GetContainer().Build().Should().Be("Encryption: https://example.com/pgp-key.txt");
		}
	}

	public class SetHiringMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Hiring_Value()
		{
			_builder.SetHiring("https://example.com/jobs.html")
				.GetContainer().Build().Should().Be("Hiring: https://example.com/jobs.html");
		}
	}

	public class SetPermissionMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Permission_Value()
		{
			_builder.SetPermission("none")
				.GetContainer().Build().Should().Be("Permission: none");
		}
	}

	public class SetPolicyMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Policy_Value()
		{
			_builder.SetPolicy("https://example.com/security-policy.html")
				.GetContainer().Build().Should().Be("Policy: https://example.com/security-policy.html");
		}
	}

	public class SetSignatureMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Signature_Value()
		{
			_builder.SetSignature("https://example.com/.well-known/security.txt.sig")
				.GetContainer().Build().Should().Be("Signature: https://example.com/.well-known/security.txt.sig");
		}
	}

	public class SetTextMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Whole_Text()
		{
			_builder.SetText("this is the information")
				.GetContainer().Build().Should().Be("this is the information");
		}
	}

	public class SetValidateValuesMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_ValidateValues_Property()
		{
			_builder.SetValidateValues(false)
				.GetContainer().ValidateValues.Should().BeFalse();
		}
	}

	public class SetIntroductionMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Set_Introduction_Value()
		{
			_builder.SetIntroduction("The ACME Security information.")
				.GetContainer().Build().Should().Be("# The ACME Security information.");
		}
	}

	public class UseWindowsStyleNewLineMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Sets_Line_Style()
		{
			_builder.UseWindowsStyleNewLine()
				.GetContainer().NewLineString.Should().Be("\r\n");
		}
	}

	public class UseUnixStyleNewLineMethod : SecurityTextBuilderTests
	{
		[Test]
		public void Sets_Line_Style()
		{
			_builder.UseUnixStyleNewLine()
				.GetContainer().NewLineString.Should().Be("\n");
		}
	}
}
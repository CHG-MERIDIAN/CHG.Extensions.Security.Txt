using CHG.Extensions.Security.Txt.Internal;
using FluentAssertions;
using NUnit.Framework;


namespace CHG.Extensions.Security.Txt.Tests;

[TestFixture]
public class SecurityTextContainerTests
{
	private SecurityTextContainer _container;

	[SetUp]
	public void Setup()
	{
		_container = new SecurityTextContainer
		{
			Contact = "mailto:test@test.com"
		};
	}

	public class ValidateValuesProperty : SecurityTextContainerTests
	{
		[Test]
		public void Is_True_By_Default()
		{
			_container.ValidateValues.Should().BeTrue();
		}
	}

	public class BuildMethod : SecurityTextContainerTests
	{
		[Test]
		public void Returns_Text_When_Given()
		{
			_container.Text = "Test\n123";
			_container.Build().Should().Be("Test\n123");
		}

		[Test]
		public void Returns_Always_Text_When_Given()
		{
			_container.Text = "Test\n123";
			_container.Contact = "Test@test.com";
			_container.Build().Should().Be("Test\n123");
		}

		[TestCase("\n")]
		[TestCase("\r\n")]
		public void Returns_Security_Infos(string newLineStyle)
		{
			_container.NewLineString = newLineStyle;
			_container.Contact = "mailto:security@example.com";
			_container.Encryption = "https://example.com/pgp-key.txt";
			_container.Hiring = "https://example.com/jobs.html";
			_container.Acknowledgments = "https://example.com/hall-of-fame.html";
			_container.Policy = "https://example.com/security-policy.html";
			_container.Signature = "https://example.com/.well-known/security.txt.sig";
			_container.Introduction = "The ACME Security information.";
			_container.Permission = "none";

			_container.Build().Should().Be("# The ACME Security information." + newLineStyle +
"Contact: mailto:security@example.com" + newLineStyle +
"Encryption: https://example.com/pgp-key.txt" + newLineStyle +
"Signature: https://example.com/.well-known/security.txt.sig" + newLineStyle +
"Policy: https://example.com/security-policy.html" + newLineStyle +
"Acknowledgments: https://example.com/hall-of-fame.html" + newLineStyle +
"Hiring: https://example.com/jobs.html" + newLineStyle +
"Permission: none");
		}
	}

	public class CreateCommentMethod : SecurityTextContainerTests
	{
		[Test]
		public void Returns_Empty_For_EmptyValue()
		{
			SecurityTextContainer.CreateComment("", Environment.NewLine).Should().BeEmpty();
		}

		[Test]
		public void Returns_Empty_For_Null_Value()
		{
			SecurityTextContainer.CreateComment(null, Environment.NewLine).Should().BeEmpty();
		}

		[Test]
		public void Returns_Single_Line()
		{
			SecurityTextContainer.CreateComment("test", Environment.NewLine).Should().Be("# test");
		}

		[Test]
		public void Returns_Multiline_With_Prefix()
		{
			SecurityTextContainer.CreateComment("test\r\nother line", Environment.NewLine).Should().Be("# test\r\n# other line");
		}

		[Test]
		public void Returns_Multiline_With_Prefix_With_Mixed_NewLine_Style()
		{
			SecurityTextContainer.CreateComment("test\r\nother line\nanother line", Environment.NewLine).Should().Be("# test\r\n# other line\r\n# another line");
		}
	}

	public class ValidateMethod : SecurityTextContainerTests
	{
		[Test]
		public void Throws_No_Exception_For_Valid_Acknowledgments()
		{
			_container.Acknowledgments = "https://example.com/hall-of-fame.html";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_For_When_Acknowledgments_Is_Not_An_Url()
		{
			_container.Acknowledgments = "mail:sdfds";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Acknowledgments_Is_No_Url()
		{
			_container.Acknowledgments = "test@secure.com";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Contact_Url_Is_Not_HTTPS()
		{
			_container.Contact = "http://contact.security.com/test";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Contact_Url_Is_Invalid()
		{
			_container.Contact = "https://123_df<sd";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_For_HTTPS_Contact()
		{
			_container.Contact = "https://contact.security.com/test";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_No_Exception_For_Missing_Contact_When_Set_By_Text()
		{
			_container.Contact = null;
			_container.Text = "Contact: mailto:test@example.com.au\r\n";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_No_Exception_If_Contact_Is_Valid_Email_Address()
		{
			_container.Contact = "mailto:security@example.com";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Contact_Is_Invalid_Email_Address()
		{
			_container.Contact = "mailto:securityt.a";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Contact_Email_Do_Not_Use_Scheme()
		{
			_container.Contact = "security@example.com";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_If_Contact_Is_Valid_Phone_number()
		{
			_container.Contact = "tel:+1-201-555-0123";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_No_Exception_If_Contact_Are_Valid_Values()
		{
			_container.Contact = "mailto:security@example.com;tel:+1-201-555-0123";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Contact_Are_Invalid_Values()
		{
			_container.Contact = "mailto:security@example.com;+1-201-555-0123";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Contact_Use_Unknown_Scheme()
		{
			_container.Contact = "aab:test.de";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_No_Contact_Is_Present()
		{
			_container.Contact = "";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>().WithMessage("The \"Contact: \" directive MUST always be present in a security.txt file.");
		}

		[Test]
		public void Throws_Exception_If_Encryption_Uri_Is_Invalid()
		{
			_container.Encryption = "asd";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_If_Encryption_Url_Is_HTTPS()
		{
			_container.Encryption = "https://example.com/pgp-key.txt";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_No_Exception_If_Encryption_Url_Is_Valid_Dns()
		{
			_container.Encryption = "dns:5d2d37ab76d47d36._openpgpkey.example.com?type=OPENPGPKEY";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}


		[Test]
		public void Throws_No_Exception_If_Encryption_Url_Is_Valid_Fingerprint()
		{
			_container.Encryption = "openpgp4fpr:5f2de5521c63a801ab59ccb603d49de44b29100f";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Encryption_Url_Is_Not_HTTPS()
		{
			_container.Encryption = "http://example.com/pgp-key.txt";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Hiring_Uri_Is_Invalid()
		{
			_container.Hiring = "ttt:sdfsdf";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_If_Hiring_Url_Is_HTTPS()
		{
			_container.Hiring = "https://example.com/jobs.html";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Hiring_Url_Is_Not_HTTPS()
		{
			_container.Hiring = "http://example.com/jobs.html";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Permission_Is_Other_Than_None()
		{
			_container.Permission = "full";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Policy_Uri_Is_Invalid()
		{
			_container.Policy = "ttt:sdfsdf";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_If_Policy_Url_Is_HTTPS()
		{
			_container.Policy = "https://example.com/security-policy.html";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Policy_Url_Is_Not_HTTPS()
		{
			_container.Policy = "http://example.com/security-policy.html";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_Signature_Uri_Is_Invalid()
		{
			_container.Signature = "ttt:sdfsdf";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_No_Exception_If_Signature_Url_Is_HTTPS()
		{
			_container.Signature = "https://example.com/.well-known/security.txt.sig";
			Action action = () => _container.Validate();
			action.Should().NotThrow();
		}

		[Test]
		public void Throws_Exception_If_Signature_Url_Is_Not_HTTPS()
		{
			_container.Signature = "http://example.com/.well-known/security.txt.sig";
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_RedirectUrl_Empty()
		{
			_container.RedirectUrl = string.Empty;
			_container.HasRedirect = true;
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_RedirectUrl_Is_Not_HTTPS()
		{
			_container.RedirectUrl = "http://example.com/.well-known/security.txt";
			_container.HasRedirect = true;
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_RedirectUrl_Is_Invalid()
		{
			_container.RedirectUrl = "ttt:sdfsdf";
			_container.HasRedirect = true;
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}

		[Test]
		public void Throws_Exception_If_RedirectUrl_Path_Is_Invalid()
		{
			_container.RedirectUrl = "http://example.com/other-path/security.txt";
			_container.HasRedirect = true;
			Action action = () => _container.Validate();
			action.Should().Throw<InvalidSecurityInformationException>();
		}
	}
}

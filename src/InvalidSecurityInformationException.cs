namespace CHG.Extensions.Security.Txt;

public class InvalidSecurityInformationException : Exception
{
	public InvalidSecurityInformationException()
	{
	}

	public InvalidSecurityInformationException(string message)
		: base(message)
	{
	}

	public InvalidSecurityInformationException(string message, Exception inner)
		: base(message, inner)
	{
	}

}

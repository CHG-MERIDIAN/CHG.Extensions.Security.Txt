using System.Runtime.Serialization;

namespace CHG.Extensions.Security.Txt;

[Serializable]
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

	protected InvalidSecurityInformationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}

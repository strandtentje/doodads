using System;
using System.Runtime.Serialization;

namespace Ziewaar.RAD.Doodads.RKOP.Exceptions;

[Serializable]
public class ReferenceException : Exception
{
    public ReferenceException()
    {
    }

    public ReferenceException(string message) : base(message)
    {
    }

    public ReferenceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ReferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

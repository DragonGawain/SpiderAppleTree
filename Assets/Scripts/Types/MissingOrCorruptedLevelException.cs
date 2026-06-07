using System;

public class MissingOrCorruptedLevelException : Exception
{
    public MissingOrCorruptedLevelException() { }

    public MissingOrCorruptedLevelException(string message)
        : base(message) { }

    public MissingOrCorruptedLevelException(string message, Exception inner)
        : base(message, inner) { }
}

using System;

public class NonUniqueLevelElementException : Exception
{
    public NonUniqueLevelElementException() { }

    public NonUniqueLevelElementException(string message)
        : base(message) { }

    public NonUniqueLevelElementException(string message, Exception inner)
        : base(message, inner) { }
}

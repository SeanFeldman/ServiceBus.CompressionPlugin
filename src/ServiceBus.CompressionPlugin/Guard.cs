using System;

static class Guard
{
    public static void AgainstEmpty(string argumentName, string value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(argumentName);
        }
    }
    public static void AgainstNull(string argumentName, object value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    public static void AgainstNegativeOrZero(string argumentName, int value)
    {
        if (value <= 0)
        {
            throw new ArgumentException($"Value cannot be negative or zero. Value was: {value}.", argumentName);
        }
    }
}

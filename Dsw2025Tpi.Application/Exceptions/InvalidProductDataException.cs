using System;

namespace Dsw2025Tpi.Application.Exceptions
{
    public class InvalidProductDataException : Exception
    {
        public InvalidProductDataException(string message) : base(message)
        {
        }

        public InvalidProductDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
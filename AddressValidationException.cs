using System;
using System.Net;

public class AddressValidationException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public int RetryAttempts { get; }

    public AddressValidationException(string message, HttpStatusCode? statusCode, int retryAttempts = 0, Exception? innerException = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RetryAttempts = retryAttempts;
    }
}
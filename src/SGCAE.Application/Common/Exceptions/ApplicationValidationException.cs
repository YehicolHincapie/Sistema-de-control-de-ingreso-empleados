namespace SGCAE.Application.Common.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ApplicationValidationException(IDictionary<string, string[]> errors)
        : base("Se produjeron uno o más errores de validación en la solicitud.")
    {
        Errors = errors;
    }
}


// In TruckFreightSystem.Application/Common/Exceptions/ValidationException.cs
using System.Runtime.Serialization;

namespace TruckFreightSystem.Application.Common.Exceptions
{
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException() : base("One or more validation errors occurred.") { }

        public ValidationException(string message) : base(message) { }

        public ValidationException(string message, Exception innerException) : base(message, innerException) { }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        // Optional: Constructor to pass validation failures
        public ValidationException(IDictionary<string, string[]> errors) : this("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}



namespace TruckFreightSystem.Application.Common.Exceptions
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("The requested resource was not found.") { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
        protected NotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}


namespace TruckFreightSystem.Application.Common.Exceptions
{
    [Serializable]
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException() : base("A business logic error occurred.") { }
        public BusinessLogicException(string message) : base(message) { }
        public BusinessLogicException(string message, Exception innerException) : base(message, innerException) { }
        protected BusinessLogicException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}



namespace TruckFreightSystem.Application.Common.Exceptions
{
    [Serializable]
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException() : base("An error occurred while communicating with an external service.") { }
        public ExternalServiceException(string message) : base(message) { }
        public ExternalServiceException(string message, Exception innerException) : base(message, innerException) { }
        protected ExternalServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}


namespace TruckFreightSystem.Application.Common.Exceptions
{
    [Serializable]
    public class DuplicateEntryException : Exception
    {
        public DuplicateEntryException() : base("A record with similar unique identifier already exists.") { }
        public DuplicateEntryException(string message) : base(message) { }
        public DuplicateEntryException(string message, Exception innerException) : base(message, innerException) { }
        protected DuplicateEntryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
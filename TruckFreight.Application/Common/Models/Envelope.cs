using System.Collections.Generic;

namespace TruckFreight.Application.Common.Models
{
    public class Envelope<T>
    {
        public T Result { get; }
        public bool IsSuccess { get; }
        public List<string> Errors { get; }
        public int StatusCode { get; }

        private Envelope(T result, bool isSuccess, List<string> errors, int statusCode)
        {
            Result = result;
            IsSuccess = isSuccess;
            Errors = errors;
            StatusCode = statusCode;
        }

        public static Envelope<T> Success(T result, int statusCode = 200)
        {
            return new Envelope<T>(result, true, new List<string>(), statusCode);
        }

        public static Envelope<T> Failure(List<string> errors, int statusCode = 400)
        {
            return new Envelope<T>(default, false, errors, statusCode);
        }

        public static Envelope<T> NotFound(List<string> errors = null)
        {
            return new Envelope<T>(default, false, errors ?? new List<string> { "Resource not found" }, 404);
        }

        public static Envelope<T> Unauthorized(List<string> errors = null)
        {
            return new Envelope<T>(default, false, errors ?? new List<string> { "Unauthorized access" }, 401);
        }
    }
} 
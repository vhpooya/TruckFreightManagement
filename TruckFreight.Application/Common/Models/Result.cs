using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Models
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; }

        internal Result(bool succeeded, IEnumerable<string> errors)
        {
            Succeeded = succeeded;
            Errors = errors.ToArray();
        }

        public static Result Success()
        {
            return new Result(true, new string[] { });
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }

        public static async Task<Result> SuccessAsync()
        {
            return await Task.FromResult(Success());
        }

        public static async Task<Result> FailAsync(string error)
        {
            return await Task.FromResult(Failure(new[] { error }));
        }

        public static async Task<Result> FailAsync(IEnumerable<string> errors)
        {
            return await Task.FromResult(Failure(errors));
        }
    }

    public class Result<T> : Result
    {
        public T Data { get; set; }

        internal Result(bool succeeded, T data, IEnumerable<string> errors)
            : base(succeeded, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(true, data, new string[] { });
        }

        public static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, default, errors);
        }

        public static async Task<Result<T>> SuccessAsync(T data)
        {
            return await Task.FromResult(Success(data));
        }

        public static async Task<Result<T>> FailAsync(string error)
        {
            return await Task.FromResult(Failure(new[] { error }));
        }

        public static async Task<Result<T>> FailAsync(IEnumerable<string> errors)
        {
            return await Task.FromResult(Failure(errors));
        }
    }
} 
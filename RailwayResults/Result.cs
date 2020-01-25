using System;

namespace RailwayResults
{
    public class Result<TError>
    {
        private readonly TError _error;

        public TError Error => IsFailure
            ? _error
            : throw new InvalidOperationException("Попытка доступа к ошибке при успешном результате");
        
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, TError error)
        {
            IsSuccess = isSuccess;
            _error = error;
        }

        public static Result<TError> Fail(TError error)
        {
            return new Result<TError>(false, error);
        }

        public static Result<TError> Success()
        {
            return new Result<TError>(true, default);
        }

        public static Result<TError> Combine(params Result<TError>[] results)
        {
            foreach (var result in results)
                if (result.IsFailure)
                    return result;

            return Success();
        }
    }

    public class Result<TValue, TError> : Result<TError>
    {
        private readonly TValue _value;
        public TValue Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("Попытка доступа к значению при ошибочном результате");

        protected Result(bool success, TValue value, TError error)
            : base(success, error)
        {
            _value = value;
        }

        public static Result<TValue, TError> Success(TValue value)
        {
            return new Result<TValue, TError>(true, value, default);
        }

        public static new Result<TValue, TError> Fail(TError error)
        {
            return new Result<TValue, TError>(false, default, error);
        }
    }
}
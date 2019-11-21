namespace RailwayResults
{
    public class Result<TError>
    {
        public TError Error { get; }
        
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, TError error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result<TError> Fail(TError error)
        {
            return new Result<TError>(false, error);
        }

        public static Result<TError> Success()
        {
            return new Result<TError>(true, default(TError));
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
        public TValue Value { get; }

        protected Result(bool success, TValue value, TError error)
            : base(success, error)
        {
            Value = value;
        }

        public static Result<TValue, TError> Success(TValue value)
        {
            return new Result<TValue, TError>(true, value, default(TError));
        }

        public new static Result<TValue, TError> Fail(TError error)
        {
            return new Result<TValue, TError>(false, default(TValue), error);
        }
    }
}
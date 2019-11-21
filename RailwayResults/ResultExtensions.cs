namespace RailwayResults
{
    public static class ResultExtensions
    {
        public static Result<TValue, TError> Success<TValue, TError>(this TValue value)
        {
            return Result<TValue, TError>.Success(value);
        }
    }
}

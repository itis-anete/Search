using System.Net;

namespace Search.Core.Extensions
{
    public static class HttpStatusCodeExtensions
    {
        public static int ToInt(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode;
        }

        public static bool IsServerError(this HttpStatusCode httpStatusCode)
        {
            return httpStatusCode.ToInt() / 100 == HttpStatusCode.InternalServerError.ToInt() / 100;
        }
    }
}

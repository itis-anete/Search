using System.Net;

namespace Search.Core.Extensions
{
    public static class HttpStatusCodeExtensions
    {
        public static int ToInt(this HttpStatusCode httpStatusCode)
        {
            return (int)httpStatusCode;
        }
    }
}

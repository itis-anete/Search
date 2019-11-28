using System;
using System.Collections.Generic;
using System.Text;

namespace Search.Core.Entities
{
    public class QueueModel
    {
        /// <summary>
        /// Link for indexing
        /// </summary>
        public Uri Uri { get; set; }
        /// <summary>
        /// Date that the request came
        /// </summary>
        public DateTime RequestTime { get; set; }
    }
}

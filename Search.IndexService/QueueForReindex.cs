using System;
using System.Collections.Generic;
using System.Text;

namespace Search.IndexService
{
    public class QueueForReindex
    {
        private int ReindexTime { get; set; }

        public QueueForReindex()
        {
            ReindexTime = 7;
        }
    }
}

using System;
using System.Collections.Generic;

namespace FS.Core.Models
{
    public class RescueAppResult : Result
    {
        public List<WniosekResult> Applications { get; set; } = new List<WniosekResult>();
        public TimeSpan Elapsed { get; set; }
    }
}

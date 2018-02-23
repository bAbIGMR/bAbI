using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class Time
    {
        public readonly static List<string> TimeStamp = new List<string> { "yesterday", "this morning", "this afternoon", "this evening" };
        List<KeyValuePair<string, List<ProcessingUnit>>> processingUnitByTime = new List<KeyValuePair<string, List<ProcessingUnit>>>();

    }
}

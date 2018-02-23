using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class RuleNameMatching
    {
        List<string> toReplace = new List<string>();
        Dictionary<string, int> toReplaceIndex = new Dictionary<string, int>();
        TargetRuleStructure target;
        public RuleNameMatching(List<string> replace, Dictionary<string, int> replaceIndex, TargetRuleStructure targetStructure)
        {
            toReplace = replace;
            toReplaceIndex = replaceIndex;
            target = targetStructure;
        }
        public List<string> ReplaceList { get { return toReplace; } }
        public Dictionary<string, int> RepalceIndex { get { return toReplaceIndex; } }
        public TargetRuleStructure Target { get { return target; } }
    }

}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class Equivalence
    {
        public static Dictionary<string, List<string>> allEquivalence = new Dictionary<string, List<string>>();
        static Equivalence()
        {
            PredicateClustering obj = new PredicateClustering();
            allEquivalence = obj.ClusterPredicate();
            #region predicate clustering sample
            //allEquivalence["IS_IN"] = new List<string> { "moved to", "went to", "went back to", "travelled to", "journeyed to" };
            //allEquivalence["WITH"] = new List<string> { "got", "took", "picked up", "grabbed" };
            //allEquivalence["NOT_WITH"] = new List<string> { "left", "put down", "discarded", "dropped" };
            //allEquivalence["NORTH"] = new List<string> { "is north of" };
            //allEquivalence["WEST"] = new List<string> { "is west of" };
            //allEquivalence["EAST"] = new List<string> { "is east of" };
            //allEquivalence["SOUTH"] = new List<string> { "is south of" };
            //allEquivalence["GAVE"] = new List<string> { "gave", "handed", "passed" };
            //allEquivalence["TO"] = new List<string> { "to" };
            //allEquivalence["ISIN"] = new List<string> { "is in" };
            //allEquivalence["IS_NOT_IN"] = new List<string> { "is not in", "is no longer in" };
            //allEquivalence["AFRAID"] = new List<string> { "are afraid of" };
            //allEquivalence["IS_A"] = new List<string> { "is a" };
            //allEquivalence["IS"] = new List<string> { "is" };
            //allEquivalence["ABOVE"] = new List<string> { "is above" };
            //allEquivalence["LEFT"] = new List<string> { "is to left of" };
            //allEquivalence["BELOW"] = new List<string> { "is below" };
            //allEquivalence["RIGHT"] = new List<string> { "is to right of" };
            //allEquivalence["FIT"] = new List<string> { "fits inside" };
            //allEquivalence["BIGGER"] = new List<string> { "is bigger than" };
            #endregion
        }
        public Dictionary<string, List<string>> EqulivalenceSet { get { return allEquivalence; } }
    }
}

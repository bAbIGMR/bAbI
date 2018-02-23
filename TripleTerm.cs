using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class TripleTerm
    {
        public string SubjectValue { get; set; }
        public string PredicateValue { get; set; }
        public string ObjectValue { get; set; }

        private TripleTerm()
        { }
        public TripleTerm(string subjectValue, string predicateValue, string objectValue)
        {
            SubjectValue = subjectValue;
            PredicateValue = predicateValue;
            ObjectValue = objectValue;
        }
    }
}

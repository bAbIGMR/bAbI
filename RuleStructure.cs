using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class RuleStructure
    {
        List<TripleTerm> _tripleStructure;
        List<KeyValuePair<string, VariableType>> _qaStructure;
        int _questionLength;
        private RuleStructure() { }
        public RuleStructure(List<TripleTerm> triple, List<KeyValuePair<string, VariableType>> qa, int questionLength)
        {
            _tripleStructure = triple;
            _qaStructure = qa;
            _questionLength = questionLength;
        }
        public List<TripleTerm> Triple { get { return _tripleStructure; } set { _tripleStructure = value; } }
        public List<KeyValuePair<string, VariableType>> QAStructure { get { return _qaStructure; } set { _qaStructure = value; } }
        public int QuestionLength { get { return _questionLength; } }
    }
}

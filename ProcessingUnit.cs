using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class ProcessingUnit
    {
        TripleType _tripleType;
        string _subjectValue;
        string _predicateValue;
        string _objectValue;
        List<string> _question;
        List<string> _answer;
        List<int> _answerRelated;
        private ProcessingUnit() { }
        public ProcessingUnit(TripleType tripleType, string subjectValue, string predicateValue, string objectValue, List<string> question, List<string> answer, List<int> answerRelated)
        {
            _tripleType = tripleType;
            _subjectValue = subjectValue;
            _predicateValue = predicateValue;
            _objectValue = objectValue;
            _question = question;
            _answer = answer;
            _answerRelated = answerRelated;
        }
        public TripleType GetTripleType { get { return _tripleType; } }
        public string GetSubjectValue { get { return _subjectValue; } }
        public string GetPredicateValue { get { return _predicateValue; } }
        public string GetObjectValue { get { return _objectValue; } }
        public List<string> GetQuestion { get { return _question; } }
        public List<string> GetAnswer { get { return _answer; } }
        public List<int> GetAnswerRelated { get { return _answerRelated; } }
    }
}

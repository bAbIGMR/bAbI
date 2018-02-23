using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class RuleSetUnit
    {
        List<KeyValuePair<string, VariableType>> _qaStructure;
        List<RuleStorage> _factExpression;
        int _questionLength = Variable.VariableZero;
        private RuleSetUnit() { }
        public RuleSetUnit(List<KeyValuePair<string, VariableType>> QAStructure, List<RuleStorage> factExpressionSet, int questionLength)
        {
            _qaStructure = QAStructure;
            _factExpression = factExpressionSet;
            _questionLength = questionLength;
        }
        public List<KeyValuePair<string, VariableType>> RuleName { get { return _qaStructure; } }
        public List<RuleStorage> FactExpressionSet { get { return _factExpression; } }
        public int QuestionLength { get { return _questionLength; } }
    }

    public class RuleStorage
    {
        int currentRuleSetFactCount;
        List<FactUnit> _factsCount2ExpressRule;
        private RuleStorage() { }
        public RuleStorage(int factCount, List<FactUnit> factCount2Facts)
        {
            currentRuleSetFactCount = factCount;
            _factsCount2ExpressRule = factCount2Facts;
        }
        public List<FactUnit> FactsCount2Facts { get { return _factsCount2ExpressRule; } }
        public int FactCount { get { return currentRuleSetFactCount; } }
    }
    public class FactUnit
    {
        List<TripleTerm> _factUnit;
        private FactUnit() { }
        public FactUnit(List<TripleTerm> factUnit) { _factUnit = factUnit; }
        public List<TripleTerm> GetFactUnit { get { return _factUnit; } }
        public bool IsEqual(FactUnit factUnitObj)
        {
            var tripleTermList = factUnitObj.GetFactUnit;
            if (_factUnit.Count != tripleTermList.Count)
                return false;
            for (int i = Variable.VariableZero; i < _factUnit.Count; i++)
            {
                if (tripleTermList[i].SubjectValue != _factUnit[i].SubjectValue ||
                    tripleTermList[i].PredicateValue != _factUnit[i].PredicateValue ||
                    tripleTermList[i].ObjectValue != _factUnit[i].ObjectValue)
                    return false;
            }
            return true;
        }
    }

}

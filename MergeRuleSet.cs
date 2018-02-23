using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class MergeRuleSet
    {
        public List<RuleSetUnit> StoreRuleSet(List<List<RuleStructure>> ruleSet, List<RuleSetUnit> ruleCollection)
        {
            List<RuleSetUnit> result = new List<RuleSetUnit>(ruleCollection);
            foreach (var currentSession in ruleSet)
            {
                foreach (var currentRule in currentSession)
                {
                    var currentQAStructure = currentRule.QAStructure;
                    var tripleList = currentRule.Triple;
                    FactUnit currentFact = new FactUnit(tripleList);
                    var ruleNameIndex = RuleNameMatching(currentQAStructure, result);
                    if (ruleNameIndex == Variable.NegInit)//the rule name appear first time
                    {
                        RuleStorage currentRuleStorage = new RuleStorage(tripleList.Count, new List<FactUnit> { currentFact });
                        RuleSetUnit resultObj = new RuleSetUnit(currentQAStructure, new List<RuleStorage> { currentRuleStorage }, currentRule.QuestionLength);
                        result.Add(resultObj);
                    }
                    else
                    {
                        var factIndex = CurrentFactsMatching(currentFact, result, ruleNameIndex);
                        var currentRuleSetUnit = result[ruleNameIndex];
                        if (factIndex == Variable.NegInit)
                        {
                            RuleStorage currentRuleStorage = new RuleStorage(tripleList.Count, new List<FactUnit> { currentFact });
                            var factExpressionSet = currentRuleSetUnit.FactExpressionSet;
                            factExpressionSet.AddRange(new List<RuleStorage> { currentRuleStorage });
                            result[ruleNameIndex] = new RuleSetUnit(currentQAStructure, factExpressionSet, currentRule.QuestionLength);
                        }
                        else
                        {
                            var currentRuleStorage = currentRuleSetUnit.FactExpressionSet[factIndex];
                            var currentFactUnitList = currentRuleStorage.FactsCount2Facts;
                            var flag = FactUnitMatching(currentFact, currentFactUnitList);
                            if (!flag)
                            {
                                List<FactUnit> temp = new List<FactUnit>(currentFactUnitList);
                                temp.Add(currentFact);
                                RuleStorage tempRule = new RuleStorage(currentRuleStorage.FactCount, temp);
                                var tt = result[ruleNameIndex].FactExpressionSet;
                                List<RuleStorage> rr = new List<RuleStorage>(tt);
                                rr[factIndex] = tempRule;
                                result[ruleNameIndex] = new RuleSetUnit(currentQAStructure, rr, currentRule.QuestionLength);
                            }
                        }
                    }
                }
            }
            return result;
        }
        private bool FactUnitMatching(FactUnit currentFactUnit, List<FactUnit> factUnitList)
        {
            foreach (var unit in factUnitList)
                if (unit.IsEqual(currentFactUnit))
                    return true;
            return false;
        }
        private int CurrentFactsMatching(FactUnit currentFact, List<RuleSetUnit> result, int nameIndex)
        {
            var currentUnit = result[nameIndex];
            var ruleStorageList = currentUnit.FactExpressionSet;
            int factIndex = Variable.NegInit;
            for (int i = Variable.VariableZero; i < ruleStorageList.Count; i++)
            {
                if (ruleStorageList[i].FactCount == currentFact.GetFactUnit.Count)
                {
                    factIndex = i;
                    break;
                }
            }
            return factIndex;
        }
        private int RuleNameMatching(List<KeyValuePair<string, VariableType>> ruleName, List<RuleSetUnit> result)
        {
            for (int i = Variable.VariableZero; i < result.Count; i++)
            {
                var currentRuleUnit = result[i];
                var ruleNameInUnit = currentRuleUnit.RuleName;
                if (ruleName.Count != ruleNameInUnit.Count)
                    continue;
                else
                {
                    bool flag = true;
                    for (int j = Variable.VariableZero; j < ruleName.Count; j++)
                        if (ruleNameInUnit[j].Key != ruleName[j].Key)
                            flag = false;
                    if (flag)
                        return i;
                }
            }
            return Variable.NegInit;
        }
    }
}

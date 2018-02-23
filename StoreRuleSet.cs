using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Trinity.Storage;
namespace GMR
{
    public class StoreRuleSet
    {
        public static void StoreRuleCollection(Dictionary<string, string> predicateDic, HashSet<string> variableSet, List<RuleSetUnit> ruleCollection)
        {
            foreach (var predicate in predicateDic)
            {
                var predicateCell = new PredicateSymbolDic(predicate.Key, predicate.Value);
                Global.LocalStorage.SavePredicateSymbolDic(predicateCell);
            }
            foreach (var variable in variableSet)
            {
                var variableCell = new VariableHistorical(variable);
                Global.LocalStorage.SaveVariableHistorical(variableCell);
            }
            foreach (var ruleUnit in ruleCollection)
            {
                var qaCombination = ruleUnit.RuleName;
                List<QAUnit> qaStore = new List<QAUnit>();
                foreach (var nameUnit in qaCombination)
                    qaStore.Add(new QAUnit(nameUnit.Key, nameUnit.Value.ToString()));
                var questionLength = ruleUnit.QuestionLength;
                List<RuleStorageStruct> factExpression = new List<RuleStorageStruct>();
                var sourceRuleStorageList = ruleUnit.FactExpressionSet;
                foreach (var ruleStorage in sourceRuleStorageList)
                {
                    int factCount = ruleStorage.FactCount;
                    var factExpressionList = ruleStorage.FactsCount2Facts;
                    List<FactUnitStruct> factExpressionStructList = new List<FactUnitStruct>();
                    foreach (var factunit in factExpressionList)
                    {
                        List<TripleTermStruct> tripleTermList = new List<TripleTermStruct>();
                        var tripleListTerm = factunit.GetFactUnit;
                        foreach (var triple in tripleListTerm)
                            tripleTermList.Add(new TripleTermStruct(triple.SubjectValue, triple.PredicateValue, triple.ObjectValue));
                        factExpressionStructList.Add(new FactUnitStruct(tripleTermList));
                    }
                    factExpression.Add(new RuleStorageStruct(factCount, factExpressionStructList));
                }
                RuleUnitCell ruleUnitCellObj = new RuleUnitCell(qaStore, factExpression, questionLength);
                Global.LocalStorage.SaveRuleUnitCell(ruleUnitCellObj);
            }
            Global.LocalStorage.SaveStorage();
        }
    }
}

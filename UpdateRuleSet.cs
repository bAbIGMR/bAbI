using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class UpdateRuleSet
    {
        public static List<RuleSetUnit> Update(HashSet<string> variableSet, List<RuleSetUnit> ruleCollection)
        {
            List<RuleSetUnit> result = new List<RuleSetUnit>();
            var input = RuleSet2Input(ruleCollection);
            var outPut = Update(variableSet, input);
            result = OutPut2RuleSet(outPut);
            return result;
        }
        private static List<RuleSetUnit> OutPut2RuleSet(List<List<RuleStructure>> output)
        {
            List<RuleSetUnit> result = new List<RuleSetUnit>();
            List<List<RuleStructure>> outputCopy = new List<List<RuleStructure>>(output);
            foreach (var ruleStructureList in outputCopy)
            {
                foreach (var rulestructure in ruleStructureList)
                {
                    var currentQA = rulestructure.QAStructure;
                    var currentTripleList = rulestructure.Triple;
                }
            }
            MergeRuleSet mergeObj = new MergeRuleSet();
            result = mergeObj.StoreRuleSet(outputCopy, new List<RuleSetUnit>());
            return result;
        }
        private static List<List<RuleStructure>> RuleSet2Input(List<RuleSetUnit> ruleCollection)
        {
            List<List<RuleStructure>> input = new List<List<RuleStructure>>();
            foreach (var ruleSetUnit in ruleCollection)
            {
                List<RuleStructure> ruleStructureList = new List<RuleStructure>();
                var ruleName = ruleSetUnit.RuleName;
                var facts = ruleSetUnit.FactExpressionSet;
                foreach (var ruleStorage in facts)
                {
                    var factList = ruleStorage.FactsCount2Facts;
                    foreach (var tripleList in factList)
                        ruleStructureList.Add(new RuleStructure(tripleList.GetFactUnit, ruleName, ruleSetUnit.QuestionLength));
                }
                input.Add(ruleStructureList);
            }
            return input;
        }
        public static List<List<RuleStructure>> Update(HashSet<string> variableSet, List<List<RuleStructure>> ruleSet)
        {
            List<List<RuleStructure>> result = new List<List<RuleStructure>>();
            int temp = Variable.VariableZero;

            foreach (var currentSessionRule in ruleSet)
            {
                List<RuleStructure> newRuleStructureList = new List<RuleStructure>();
                foreach (var currentRuleStructure in currentSessionRule)
                {
                    temp++;
                    var tripleTermList = currentRuleStructure.Triple;
                    var qaStructure = currentRuleStructure.QAStructure;

                    var qaCopy = new List<KeyValuePair<string, VariableType>>(qaStructure);
                    qaCopy.RemoveRange(Variable.VariableZero, currentRuleStructure.QuestionLength);
                    if (qaCopy[Variable.VariableZero].Value == VariableType.Unknown && variableSet.Contains(qaCopy[Variable.VariableZero].Key))
                    {
                        newRuleStructureList.Add(currentRuleStructure);
                        continue;
                    }

                    Variable _variable = new Variable();
                    var allVariable = _variable.GetAllVariable;
                    HashSet<string> currentVariableCounter = new HashSet<string>();
                    foreach (var triple in tripleTermList)
                    {
                        var currentSubject = triple.SubjectValue;
                        var currentPreicate = triple.PredicateValue;
                        var currentObject = triple.ObjectValue;
                        foreach (var variable in allVariable)
                        {
                            if (currentSubject.Contains(variable))
                                currentVariableCounter.Add(variable);
                            if (currentObject.Contains(variable))
                                currentVariableCounter.Add(variable);
                        }
                    }
                    int max = Variable.VariableZero;
                    foreach (var ele in currentVariableCounter)
                    {
                        var index = _variable.GetVariableIndex(ele);
                        if (index > max)
                            max = index;
                    }
                    Variable newVariable = new Variable(max + Variable.VariableSingle);
                    List<TripleTerm> newTripleTermList = new List<TripleTerm>();
                    Dictionary<string, string> newVariableDic = new Dictionary<string, string>();
                    foreach (var triple in tripleTermList)
                    {
                        var currentSubject = triple.SubjectValue;
                        var currentPreicate = triple.PredicateValue;
                        var currentObject = triple.ObjectValue;

                        foreach (var variable in variableSet)
                        {
                            if (currentSubject.Contains(variable))
                            {
                                if (Tools.IsWell(currentSubject, variable))
                                    if (!newVariableDic.ContainsKey(variable))
                                    {
                                        var newSymbol = newVariable.GetAvailiableVariableSymbol;
                                        newVariableDic[variable] = newSymbol;
                                        currentSubject = currentSubject.Replace(variable, newSymbol);
                                    }
                                    else
                                        currentSubject = currentSubject.Replace(variable, newVariableDic[variable]);
                            }
                            if (currentObject.Contains(variable))
                            {
                                if (Tools.IsWell(currentObject, variable))
                                    if (!newVariableDic.ContainsKey(variable))
                                    {
                                        var newSymbol = newVariable.GetAvailiableVariableSymbol;
                                        newVariableDic[variable] = newSymbol;
                                        currentObject = currentObject.Replace(variable, newSymbol);
                                    }
                                    else
                                        currentObject = currentObject.Replace(variable, newVariableDic[variable]);
                            }
                        }
                        newTripleTermList.Add(new TripleTerm(currentSubject, currentPreicate, currentObject));
                    }
                    int newLength = Variable.VariableZero;
                    var newQa = ReConstructQA(qaStructure, newVariableDic, variableSet, currentRuleStructure.QuestionLength, out newLength, currentVariableCounter.Count);
                    newRuleStructureList.Add(new RuleStructure(newTripleTermList, newQa, newLength));
                }
                result.Add(newRuleStructureList);
            }
            return result;
        }
        private static List<KeyValuePair<string, VariableType>> ReConstructQA(List<KeyValuePair<string, VariableType>> qa, Dictionary<string, string> currentVariableDic, HashSet<string> variableHistorical, int questionLength, out int newLength, int variableCounter)
        {
            List<KeyValuePair<string, VariableType>> result = new List<KeyValuePair<string, VariableType>>();

            Variable allVariable = new Variable();
            var _allVariable = allVariable.GetAllVariable;
            string qString = Variable.NullString;
            for (int i = Variable.VariableZero; i < questionLength; i++)
                qString += qa[i].Key + Variable.SpaceString;
            qString = qString.Trim();
            string aString = Variable.NullString;
            for (int i = questionLength; i < qa.Count; i++)
                aString += qa[i].Key + Variable.SpaceString;
            aString = aString.Trim();

            for (int i = Variable.VariableZero; i < Variable.Threshold; i++)
                foreach (var variable in variableHistorical)
                {
                    if (qString.Contains(variable))
                        if (currentVariableDic.ContainsKey(variable))
                            qString = qString.Replace(variable, currentVariableDic[variable]);
                        else
                        {
                            int max = Variable.VariableZero;
                            Variable variableObj = new Variable();
                            foreach (var e in currentVariableDic)
                            {
                                int index = variableObj.GetVariableIndex(e.Value);
                                if (index > max)
                                    max = index;
                            }
                            Variable _variable = new Variable(max + Variable.VariableSingle + variableCounter);
                            currentVariableDic[variable] = _variable.GetAvailiableVariableSymbol;
                            qString = qString.Replace(variable, currentVariableDic[variable]);
                        }
                    if (aString.Contains(variable))
                        if (currentVariableDic.ContainsKey(variable))
                            aString = aString.Replace(variable, currentVariableDic[variable]);
                        else
                        {
                            int max = Variable.VariableZero;
                            Variable variableObj = new Variable();
                            foreach (var e in currentVariableDic)
                            {
                                int index = variableObj.GetVariableIndex(e.Value);
                                if (index > max)
                                    max = index;
                            }
                            Variable _variable = new Variable(max + Variable.VariableSingle + variableCounter);
                            currentVariableDic[variable] = _variable.GetAvailiableVariableSymbol;
                            aString = aString.Replace(variable, currentVariableDic[variable]);
                        }
                }
            result = ReBuildQA(qString, _allVariable);
            newLength = result.Count;
            var A = ReBuildQA(aString, _allVariable);
            result.AddRange(A);
            return result;
        }
        private static List<KeyValuePair<string, VariableType>> ReBuildQA(string qString, List<string> _allVariable)
        {
            List<KeyValuePair<string, VariableType>> result = new List<KeyValuePair<string, VariableType>>();
            string[] qaArray = qString.Split(new char[] { Variable.SpaceChar}, StringSplitOptions.RemoveEmptyEntries);
            List<string> temp = new List<string>();
            for (int i = Variable.VariableZero; i < qaArray.Length; i++)
            {
                if (!_allVariable.Contains(qaArray[i]))
                    temp.Add(qaArray[i]);
                else
                {
                    if (temp.Count == Variable.VariableZero)
                        result.Add(new KeyValuePair<string, VariableType>(qaArray[i], VariableType.Subject));
                    else
                    {
                        string tt = Variable.NullString;
                        foreach (var ele in temp)
                            tt += ele + Variable.SpaceString;
                        result.Add(new KeyValuePair<string, VariableType>(tt.Trim(), VariableType.Unknown));
                        result.Add(new KeyValuePair<string, VariableType>(qaArray[i], VariableType.Subject));
                        temp.Clear();
                    }
                }
            }

            if (temp.Count != Variable.VariableZero)
            {
                string lastConstant = Variable.NullString;
                foreach (var ele in temp)
                    lastConstant += ele + Variable.SpaceString;
                result.Add(new KeyValuePair<string, VariableType>(lastConstant.Trim(), VariableType.Unknown));
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class SpecificCount
    {
        string qaPair = Variable.NullString;
        List<List<TriplePresentation>> eachFile = new List<List<TriplePresentation>>();
        public SpecificCount(string QAPair, List<List<TriplePresentation>> EachFile)
        {
            qaPair = QAPair;
            eachFile = EachFile;
        }
        public string QAPair { get { return qaPair; } }
        public List<List<TriplePresentation>> EachFile { get { return eachFile; } }
    }
    public class HandleUnit
    {
        Dictionary<int, List<List<TriplePresentation>>> dicFile = new Dictionary<int, List<List<TriplePresentation>>>();
        Dictionary<string, string> predicateDic = new Dictionary<string, string>();
        string qaPair = Variable.NullString;
        public HandleUnit(Dictionary<int, List<List<TriplePresentation>>> DicFile, Dictionary<string, string> PredicateDic, string QAPair)
        {
            dicFile = DicFile;
            predicateDic = PredicateDic;
            qaPair = QAPair;
        }
        public Dictionary<int, List<List<TriplePresentation>>> DicFile { get { return dicFile; } }
        public Dictionary<string, string> PredicateDic { get { return predicateDic; } }
        public string QAPair { get { return qaPair; } }
    }
    class PredicateClustering
    {
        public Dictionary<string, List<string>> ClusterPredicate()
        {
            LoadRuleSet loadData = new LoadRuleSet();
            var data = loadData.GetRuleRepresentation();
            var ruleDir = loadData.RuleSetFileDirectory;
            return PipeLine(data, ruleDir);
        }
        private Dictionary<string, List<string>> PipeLine(List<RuleFileStructure> dataList, string ruleDir)
        {
            Dictionary<string, List<string>> finalResult = new Dictionary<string, List<string>>();
            var checker = CheckConflict(dataList);
            List<HandleUnit> allDicFile = new List<HandleUnit>();
            foreach (var fileData in dataList)
                allDicFile.Add(HandleFile(fileData, ruleDir));
            int initCount = Variable.VariableDouble;
            Dictionary<string, HashSet<string>> currentClustering = new Dictionary<string, HashSet<string>>();
            Dictionary<string, HashSet<string>> clusteringHistory = new Dictionary<string, HashSet<string>>();
            var result = GetAbsoClustering(allDicFile, ref currentClustering, clusteringHistory);
            FormatResult(result, ref clusteringHistory);
            while (IsExist(allDicFile, initCount))
            {
                var specificCount = ExtractSpecificCount(allDicFile, initCount);
                var dicList = HandlePredicate(specificCount, ref currentClustering, ref clusteringHistory);
                Dictionary<string, HashSet<string>> temp = new Dictionary<string, HashSet<string>>();
                FormatResult(dicList, ref temp);
                clusteringHistory = UpdateClusterHistory(clusteringHistory, temp);
                initCount++;
            }
            finalResult = FormatFinalClustering(allDicFile, clusteringHistory);
            return finalResult;
        }
        private Dictionary<string, List<string>> FormatFinalClustering(List<HandleUnit> all, Dictionary<string, HashSet<string>> clustering)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            Dictionary<string, string> predicateDic = new Dictionary<string, string>();
            Dictionary<string, string> predicateDicReverse = new Dictionary<string, string>();
            foreach (var unit in all)
                foreach (var kv in unit.PredicateDic)
                    predicateDicReverse[kv.Key] = kv.Value;
            foreach (var kv in predicateDicReverse)
                predicateDic[kv.Value] = kv.Key;
            foreach (var element in clustering)
            {
                List<string> predicateList = new List<string>();
                foreach (var symbol in element.Value)
                    predicateList.Add(predicateDic[symbol]);
                result[element.Key] = predicateList;
            }
            HashSet<string> notFound = new HashSet<string>();
            HashSet<string> set = new HashSet<string>();
            foreach (var unit in clustering)
                set.UnionWith(unit.Value);
            foreach (var kv in predicateDic)
                if (!set.Contains(kv.Key))
                    notFound.Add(kv.Key);
            foreach (var predicate in notFound)
                result[(result.Count + Variable.VariableSingle).ToString()] = new List<string> { predicateDic[predicate] };
            return result;
        }
        private Dictionary<string, HashSet<string>> UpdateClusterHistory(Dictionary<string, HashSet<string>> clusteringHistory, Dictionary<string, HashSet<string>> currentClustering)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            foreach (var ele in clusteringHistory)
                result[ele.Key] = ele.Value;
            foreach (var ele in currentClustering)
                if (!IsSubSet(ele.Value, result.Values.ToList()))
                    result[(result.Count + Variable.VariableSingle).ToString()] = ele.Value;
            return result;
        }
        private bool IsSubSet(HashSet<string> element, List<HashSet<string>> set)
        {
            foreach (var subSet in set)
                if (SubSetCompare(element, subSet))
                    return true;
            return false;
        }
        private bool SubSetCompare(HashSet<string> first, HashSet<string> second)
        {
            if (first.Count <= second.Count)
            {
                foreach (var ele in first)
                    if (!second.Contains(ele))
                        return false;
                return true;
            }
            else
                return false;
        }
        private void FormatResult(List<Dictionary<string, HashSet<string>>> result, ref Dictionary<string, HashSet<string>> clusteringHistory)
        {
            List<HashSet<string>> format = new List<HashSet<string>>();
            foreach (var dic in result)
                foreach (var set in dic)
                    format.Add(set.Value);
            var setList = KeepMinSet(format);
            foreach (var set in setList)
            {
                var key = clusteringHistory.Count + Variable.VariableSingle;
                clusteringHistory[key.ToString()] = set;
            }
        }
        private List<HashSet<string>> KeepMinSet(List<HashSet<string>> format)
        {
            List<HashSet<string>> result = new List<HashSet<string>>();
            List<int> toRemove = new List<int>();
            for (int i = Variable.VariableZero; i < format.Count; i++)
                for (int j = Variable.VariableZero; j < format.Count; j++)
                {
                    if (format[i].Count < format[j].Count)
                    {
                        bool flag = true;
                        foreach (var symbol in format[i])
                            if (!format[j].Contains(symbol))
                                flag = false;
                        if (flag)
                            toRemove.Add(j);
                    }
                }
            for (int i = Variable.VariableZero; i < format.Count; i++)
                if (!toRemove.Contains(i))
                    result.Add(format[i]);
            return result;
        }
        private List<Dictionary<string, HashSet<string>>> GetAbsoClustering(List<HandleUnit> allDicFile, ref Dictionary<string, HashSet<string>> currentClustering, Dictionary<string, HashSet<string>> clusteringHistory)
        {
            var specificCount = ExtractSpecificCount(allDicFile, Variable.VariableSingle);
            return HandlePredicate(specificCount, ref currentClustering, ref clusteringHistory);
        }
        private List<Dictionary<string, HashSet<string>>> HandlePredicate(List<SpecificCount> data, ref Dictionary<string, HashSet<string>> currentClustering, ref Dictionary<string, HashSet<string>> clusteringHistory)
        {
            Dictionary<string, HashSet<string>> local = new Dictionary<string, HashSet<string>>();
            foreach (var kv in currentClustering)
                local[kv.Key] = kv.Value;
            List<Dictionary<string, HashSet<string>>> result = new List<Dictionary<string, HashSet<string>>>();
            foreach (var specificCount in data)
            {
                Dictionary<string, HashSet<string>> temp = new Dictionary<string, HashSet<string>>();
                var rulePres = specificCount.EachFile;
                var keyVariable = GetKeyVariable(specificCount);
                Dictionary<string, HashSet<string>> clusteringFromFP = new Dictionary<string, HashSet<string>>();
                FindFP(specificCount, ref clusteringFromFP);
                clusteringHistory = CheckClusterHistory(clusteringHistory, clusteringFromFP);
                UpdateCurrentClustering(rulePres, keyVariable, ref temp, clusteringHistory);
                AddElement(ref result, temp);
            }
            return result;
        }

        private Dictionary<string, HashSet<string>> CheckClusterHistory(Dictionary<string, HashSet<string>> clusteringHistory, Dictionary<string, HashSet<string>> clusteringFromFP)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            foreach (var ele in clusteringHistory)
                result[ele.Key] = ele.Value;
            foreach (var ele in clusteringFromFP)
                if (!IsSubSet(ele.Value, result.Values.ToList()))
                    if (!CheckConflict(ele.Value, result.Values.ToList()))
                        result[(result.Count + Variable.VariableSingle).ToString()] = ele.Value;
            return result;
        }
        private bool CheckConflict(HashSet<string> element, List<HashSet<string>> set)
        {
            foreach (var unit in set)
                foreach (var symbol in element)
                    if (unit.Contains(symbol))
                        return true;
            return false;
        }
        private void FindFP(SpecificCount data, ref Dictionary<string, HashSet<string>> clusteringHistory)
        {
            HashSet<string> temp = new HashSet<string>();
            foreach (var unit in data.EachFile)
                foreach (var triple in unit)
                    temp.Add(triple.Predicate);
            Dictionary<string, HashSet<string>> history = new Dictionary<string, HashSet<string>>();
            var eachFile = data.EachFile;
            foreach (var rule in eachFile)
                for (int i = Variable.VariableZero; i < rule.Count - Variable.VariableSingle; i++)
                    if (history.ContainsKey(rule[i].Predicate))
                        history[rule[i].Predicate].Add(rule[i + Variable.VariableSingle].Predicate);
                    else
                        history[rule[i].Predicate] = new HashSet<string> { rule[i + Variable.VariableSingle].Predicate };

            Dictionary<string, string> predicateDic = new Dictionary<string, string>();
            foreach (var record in history)
                if (record.Value.Count == Variable.VariableSingle)
                    predicateDic[record.Key] = record.Value.ToList()[Variable.VariableZero];
            Dictionary<string, HashSet<string>> predicateClustering = new Dictionary<string, HashSet<string>>();
            foreach (var kv in predicateDic)
                if (predicateClustering.ContainsKey(kv.Value))
                    predicateClustering[kv.Value].Add(kv.Key);
                else predicateClustering[kv.Value] = new HashSet<string> { kv.Key };
            foreach (var kv in predicateClustering)
                if (kv.Value.Count > Variable.VariableSingle)
                    clusteringHistory[(clusteringHistory.Count + Variable.VariableSingle).ToString()] = kv.Value;
        }
        private void AddElement(ref List<Dictionary<string, HashSet<string>>> set, Dictionary<string, HashSet<string>> element)
        {
            if (!IsExist(set, element))
                set.Add(element);
        }
        private bool IsExist(List<Dictionary<string, HashSet<string>>> set, Dictionary<string, HashSet<string>> element)
        {
            foreach (var ele in set)
                if (CompareDics(ele, element))
                    return true;
            return false;
        }
        private bool CompareDics(Dictionary<string, HashSet<string>> first, Dictionary<string, HashSet<string>> second)
        {
            if (first.Count == second.Count)
                foreach (var kv in first)
                    if (!IsExist(kv.Value, second))
                        return false;
            return true;
        }
        private bool IsExist(HashSet<string> element, Dictionary<string, HashSet<string>> set)
        {
            foreach (var ele in set)
            {
                if (ele.Value.Count == element.Count)
                {
                    bool flag = true;
                    foreach (var symbol in element)
                        if (!ele.Value.Contains(symbol))
                            flag = false;
                    if (flag)
                        return true;
                    else
                        continue;
                }
            }
            return false;
        }
        private void UpdateCurrentClustering(List<List<TriplePresentation>> currentFile, HashSet<string> keyVariable, ref Dictionary<string, HashSet<string>> currentClustering, Dictionary<string, HashSet<string>> clusteringHistory)
        {
            var currentCount = currentFile[Variable.VariableZero].Count;
            foreach (var first in currentFile)
                foreach (var second in currentFile)
                {
                    var isMatch = IsMatch(keyVariable, first, second);
                    if (isMatch)
                        UpdatePredicate(ref currentClustering, first, second, clusteringHistory);
                }
        }
        private void UpdatePredicate(ref Dictionary<string, HashSet<string>> currentClustering, List<TriplePresentation> first, List<TriplePresentation> second, Dictionary<string, HashSet<string>> clusteringHistory)
        {
            int index = first.Count;
            List<TriplePresentation> firstCopy = new List<TriplePresentation>();
            List<TriplePresentation> secondCopy = new List<TriplePresentation>();
            for (int i = Variable.VariableZero; i < index; i++)
            {
                firstCopy.Add(first[i]);
                secondCopy.Add(second[i]);
            }
            for (int j = Variable.VariableZero; j < index; j++)
            {
                first = new List<TriplePresentation>();
                second = new List<TriplePresentation>();
                for (int i = Variable.VariableZero; i < index; i++)
                {
                    first.Add(firstCopy[i]);
                    second.Add(secondCopy[i]);
                }
                ChangeOrder(j, ref first, ref second);
                List<string> firstPredicate = new List<string>();
                List<string> secondPrediacte = new List<string>();
                foreach (var triple in first)
                    firstPredicate.Add(triple.Predicate);
                foreach (var triple in second)
                    secondPrediacte.Add(triple.Predicate);
                for (int i = Variable.VariableZero; i < firstPredicate.Count; i++)
                {
                    bool flag = false;
                    if (i == firstPredicate.Count - Variable.VariableSingle)
                        AddPredicate(ref currentClustering, firstPredicate[i], secondPrediacte[i]);
                    else
                    {
                        foreach (var kv in clusteringHistory)
                            if ((kv.Value.Contains(firstPredicate[i]) && !kv.Value.Contains(secondPrediacte[i]))
                                || (!kv.Value.Contains(firstPredicate[i]) && kv.Value.Contains(secondPrediacte[i]))
                                || (!kv.Value.Contains(firstPredicate[i]) && !kv.Value.Contains(secondPrediacte[i])
                              && firstPredicate[i] != secondPrediacte[i]))
                            {
                                flag = true;
                                break;
                            }
                    }
                    if (flag)
                        break;
                }
            }
        }
        private void ChangeOrder(int index, ref List<TriplePresentation> first, ref List<TriplePresentation> second)
        {
            TriplePresentation firstSpe = first[index];
            TriplePresentation secondSpe = second[index];
            first.RemoveAt(index);
            second.RemoveAt(index);
            first.Add(firstSpe);
            second.Add(secondSpe);
        }
        private void AddPredicate(ref Dictionary<string, HashSet<string>> clustering, string first, string second)
        {
            string existKey = Variable.NullString;
            foreach (var cluster in clustering)
            {
                if (cluster.Value.Contains(first) || cluster.Value.Contains(second))
                {
                    existKey = cluster.Key;
                    break;
                }
            }
            if (existKey == Variable.NullString)
                clustering[(clustering.Count + Variable.VariableSingle).ToString()] = new HashSet<string>(new List<string> { first, second });
            else
            {
                clustering[existKey].Add(first);
                clustering[existKey].Add(second);
            }

        }
        private bool IsMatch(HashSet<string> keyVariable, List<TriplePresentation> first, List<TriplePresentation> second)
        {
            for (int i = Variable.VariableZero; i < first.Count; i++)
            {
                if (!((keyVariable.Contains(first[i].Subject) && keyVariable.Contains(second[i].Subject) && first[i].Subject == second[i].Subject
                    || !keyVariable.Contains(first[i].Subject) && !keyVariable.Contains(second[i].Subject)) &&
                    (keyVariable.Contains(first[i].Object) && keyVariable.Contains(second[i].Object) && first[i].Object == second[i].Object
                    || !keyVariable.Contains(first[i].Object) && !keyVariable.Contains(second[i].Object))))
                    return false;

            }
            return true;
        }
        private HashSet<string> GetKeyVariable(SpecificCount data)
        {
            HashSet<string> KeyVariable = new HashSet<string>();

            var _variable = new Variable();
            var allVariable = _variable.GetAllVariable;
            var qaPair = data.QAPair;
            qaPair = qaPair.Replace(Variable.TextSuffix, Variable.NullString);
            qaPair = qaPair.Replace(Variable.RuleSetFileDirectory, Variable.NullString);
            string[] subString = qaPair.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var str in subString)
                if (allVariable.Contains(Variable.StarString + str + Variable.StarString))
                    KeyVariable.Add(Variable.StarString + str + Variable.StarString);
            return KeyVariable;
        }
        private bool IsExist(List<HandleUnit> allDicFile, int count)
        {
            foreach (var handleUnit in allDicFile)
                if (handleUnit.DicFile.ContainsKey(count))
                    return true;
            return false;
        }
        private List<SpecificCount> ExtractSpecificCount(List<HandleUnit> allDicFile, int count)
        {
            List<SpecificCount> result = new List<SpecificCount>();
            foreach (var dic in allDicFile)
                if (dic.DicFile.ContainsKey(count))
                    result.Add(new SpecificCount(dic.QAPair, dic.DicFile[count]));
            return result;
        }
        private HandleUnit HandleFile(RuleFileStructure currentFile, string ruleDir)
        {
            var predicateDic = currentFile.PredicateSymbolDic;
            string qaPair = currentFile.FileName.Replace(ruleDir, Variable.NullString);
            qaPair = qaPair.Replace(Variable.TextSuffix, Variable.NullString);
            return new HandleUnit(GetRuleGraphDic(currentFile), predicateDic, currentFile.FileName);
        }
        private bool CheckConflict(List<RuleFileStructure> dataList)
        {
            List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
            foreach (var data in dataList)
                dicList.Add(data.PredicateSymbolDic);
            foreach (var first in dicList)
                foreach (var second in dicList)
                {
                    foreach (var kv in first)
                        if (second.ContainsKey(kv.Key) && kv.Value != second[kv.Key])
                            return false;
                    foreach (var kv in second)
                        if (first.ContainsKey(kv.Key) && kv.Value != first[kv.Key])
                            return false;
                }
            return true;
        }
        private Dictionary<int, List<List<TriplePresentation>>> GetRuleGraphDic(RuleFileStructure currentFile)
        {
            Dictionary<int, List<List<TriplePresentation>>> result = new Dictionary<int, List<List<TriplePresentation>>>();
            foreach (var ruleGraph in currentFile.RuleSet)
                if (result.ContainsKey(ruleGraph.Count))
                    result[ruleGraph.Count].Add(ruleGraph);
                else
                    result[ruleGraph.Count] = new List<List<TriplePresentation>> { ruleGraph };

            return result.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace GMR
{
    public class Tools
    {
        public static void PrintResult(Dictionary<string, string> predicateSymbolDic, List<List<RuleStructure>> ruleSet, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(Variable.ReDir + fileName + Variable.TextSuffix, FileMode.Create)))
            {
                foreach (var pre in predicateSymbolDic)
                    sw.WriteLine(pre.Key + Variable.TabChar.ToString() + pre.Value);
                sw.WriteLine(Variable.DoLine);
                foreach (var ele in ruleSet)
                {
                    foreach (var currentRuleStructure in ele)
                    {
                        foreach (var qa in currentRuleStructure.QAStructure)
                            sw.Write(qa.Key + Variable.TabChar.ToString());
                        sw.WriteLine();
                        foreach (var triple in currentRuleStructure.Triple)
                            sw.WriteLine(triple.SubjectValue + Variable.TabChar.ToString() + triple.PredicateValue + Variable.TabChar.ToString() + triple.ObjectValue);
                        sw.WriteLine(Variable.ConLine);
                    }
                    sw.WriteLine(Variable.StarLine);
                }
            }
        }
        public static bool IsWell(string big, string small)
        {
            var first = RelationCheck(big, small);
            var second = IsPartsOf(big, small);
            if (first && (!second))
                return true;
            else
                return false;
        }
        private static bool RelationCheck(string big, string small)
        {
            if (big == small)
                return true;
            if (big.Length > small.Length)
                if (big.Contains(Variable.SpaceString + Variable.VariableAnd + Variable.SpaceString) || big.Contains(Variable.SpaceString + Variable.VariableOr + Variable.SpaceString))
                    return true;
            return false;
        }
        private static bool IsPartsOf(string big, string small)
        {
            if (big.Contains(Variable.StarString))
                return false;
            if (big == small)
                return false;

            int index = big.IndexOf(small);
            if (big.EndsWith(small) && big.StartsWith(small))
                return false;
            if (big.EndsWith(small))
                if (big[index - Variable.VariableSingle] != Variable.SpaceChar)
                    return true;
                else return false;
            if (big.StartsWith(small))
                if (big[index + small.Length] != Variable.SpaceChar)
                    return true;
                else return false;
            if ((big[index - Variable.VariableSingle] == Variable.SpaceChar && big[index + small.Length] == Variable.SpaceChar))
                return false;
            else return true;
        }
        public static void PrintAll(Dictionary<string, string> predicateSymbolDic, List<RuleSetUnit> ruleSet)
        {
            foreach (var ruleSetUnit in ruleSet)
            {
                var ruleName = ruleSetUnit.RuleName;
                string fileName = Variable.NullString;
                foreach (var ele in ruleName)
                    fileName += ele.Key + Variable.SpaceString;
                fileName = fileName.Trim();
                var ruleStorageList = ruleSetUnit.FactExpressionSet;
                for (int i = Variable.VariableZero; i < Variable.Threshold; i++)
                    fileName = fileName.Replace(Variable.StarString, Variable.NullString);
                using (StreamWriter sw = new StreamWriter(new FileStream(Variable.RuleSetFileDirectory + fileName + Variable.TextSuffix, FileMode.Create)))
                {
                    foreach (var pre in predicateSymbolDic)
                        sw.WriteLine(pre.Key + Variable.TabChar.ToString() + pre.Value);
                    sw.WriteLine(Variable.DoLine);
                    foreach (var ele in ruleStorageList)
                    {
                        var factUnit = ele.FactsCount2Facts;
                        foreach (var currentTripleList in factUnit)
                        {
                            foreach (var triple in currentTripleList.GetFactUnit)
                                sw.WriteLine(triple.SubjectValue + Variable.TabChar.ToString() + triple.PredicateValue + Variable.TabChar.ToString() + triple.ObjectValue);
                            sw.WriteLine(Variable.ConLine);
                        }
                        sw.WriteLine(Variable.StarLine);
                    }
                    sw.WriteLine(Variable.NumLine);
                }
            }
        }
        public static IEnumerable<T[]> GetPermutationN<T>(T[] sourceArray, int permutationLength)
        {
            foreach (var index in getPermutationIndex(sourceArray.Length, permutationLength))
            {
                var rtnRow = new List<T>();
                foreach (var i in index)
                    rtnRow.Add(sourceArray[i]);
                if (rtnRow.Count == permutationLength)
                    yield return rtnRow.ToArray();
            }
        }
        private static IEnumerable<int[]> getPermutationIndex(int arrayLenght, int permLength)
        {
            for (int i = permLength; i > Variable.VariableZero; i--)
            {
                if (i > Variable.VariableSingle)
                {
                    var recursion = getPermutationIndex(arrayLenght, i - Variable.VariableSingle);
                    for (int j = Variable.VariableZero; j < arrayLenght; j++)
                        foreach (var item in recursion)
                        {
                            var rtnRow = new List<int>();
                            rtnRow.Add(j);
                            rtnRow.AddRange(item);
                            yield return rtnRow.ToArray();
                        }
                }
                else
                    for (int j = Variable.VariableZero; j < arrayLenght; j++)
                        yield return new int[] { j };
            }
        }
    }
}

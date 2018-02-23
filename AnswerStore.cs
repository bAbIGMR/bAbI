using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class AnswerResultCombination
    {
        List<KeyValuePair<string, VariableType>> _qaStructure = new List<KeyValuePair<string, VariableType>>();
        List<KeyValuePair<string, VariableType>> _tryingAnswerStructure = new List<KeyValuePair<string, VariableType>>();
        List<KeyValuePair<string, VariableType>> _tryingAnswer = new List<KeyValuePair<string, VariableType>>();
        List<int> _factSelectIndex = new List<int>();
        public List<KeyValuePair<string, VariableType>> QAStructure { get { return _qaStructure; } }
        public List<KeyValuePair<string, VariableType>> TryingAnswerStructure { get { return _tryingAnswerStructure; } }
        public List<KeyValuePair<string, VariableType>> TryingAnswer { get { return _tryingAnswer; } }
        public List<int> FactSelectIndex { get { return _factSelectIndex; } }
        public AnswerResultCombination(List<KeyValuePair<string, VariableType>> qa,
            List<KeyValuePair<string, VariableType>> anstructure,
            List<KeyValuePair<string, VariableType>> tryingAnswer, List<int> factSelectIndex)
        {
            _qaStructure = qa;
            _tryingAnswerStructure = anstructure;
            _tryingAnswer = tryingAnswer;
            _factSelectIndex = factSelectIndex;
        }
    }

    public class AnswerStore
    {
        Dictionary<AnswerResultCombination, List<List<GraphNode>>> _answerToSelect = new Dictionary<AnswerResultCombination, List<List<GraphNode>>>();
        public AnswerStore()
        { }
        private bool IsContainKey(List<KeyValuePair<string, VariableType>> currentKey)
        {
            foreach (var ele in _answerToSelect)
            {
                var flag = IsEqual(ele.Key.TryingAnswer, currentKey);
                if (flag)
                    return true;
            }
            return false;
        }
        private bool IsEqual(List<KeyValuePair<string, VariableType>> first, List<KeyValuePair<string, VariableType>> second)
        {
            if (first.Count != second.Count)
                return false;
            if (first.Count == second.Count)
                for (int i = Variable.VariableZero; i < first.Count; i++)
                    if (!(first[i].Key == second[i].Key && first[i].Value == second[i].Value))
                        return false;
            return true;
        }
        public void Add(AnswerResultCombination key, List<GraphNode> value)
        {
            var tryingAnswer = key.TryingAnswer;
            if (!IsContainKey(tryingAnswer))
                _answerToSelect[key] = new List<List<GraphNode>> { value };
        }
        private int GetIndex(List<KeyValuePair<AnswerResultCombination, List<List<GraphNode>>>> list, AnswerResultCombination key)
        {
            for (int i = Variable.VariableZero; i < list.Count; i++)
                if (IsEqual(key.TryingAnswer, list[i].Key.TryingAnswer))
                    return i;
            throw new Exception();
        }
        public List<KeyValuePair<string, VariableType>> SelectAnswer()
        {
            Dictionary<AnswerResultCombination, List<List<GraphNode>>> copy =
                    new Dictionary<AnswerResultCombination, List<List<GraphNode>>>(_answerToSelect);
            var list = copy.ToList();
            if (list.Count == Variable.VariableSingle)
                return list[Variable.VariableZero].Key.TryingAnswer;
            int maxIndex = Variable.VariableZero;
            double time = Variable.VariableZero;
            for (int i = Variable.VariableZero; i < list.Count; i++)
            {
                var timeStamp = CalcMaxTimeStamp(list[i].Key.QAStructure, list[i].Value, list[i].Key.FactSelectIndex);
                if (timeStamp > time)
                {
                    time = timeStamp;
                    maxIndex = i;
                }
            }
            if (list.Count == Variable.VariableZero)
                return new List<KeyValuePair<string, VariableType>>();
            return list[maxIndex].Key.TryingAnswer;
        }
        private double CalcMaxTimeStamp(List<KeyValuePair<string, VariableType>> qa, List<List<GraphNode>> graphList, List<int> factIndex)
        {
            var currentGraph = graphList[Variable.VariableZero];
            HashSet<string> keyVariable = new HashSet<string>();
            foreach (var ele in qa)
                if (ele.Value != VariableType.Unknown)
                    keyVariable.Add(ele.Key);
            HashSet<int> timeStampSum = new HashSet<int>();
            HashSet<int> keyTimeStampSum = new HashSet<int>();
            foreach (var node in currentGraph)
            {
                foreach (var link in node.OutLinks)
                {
                    timeStampSum.Add(link.TimeStamp);
                    if (keyVariable.Contains(node.Name) || keyVariable.Contains(link.NodeName))
                        keyTimeStampSum.Add(link.TimeStamp);
                }
                foreach (var link in node.InLinks)
                {
                    timeStampSum.Add(link.TimeStamp);
                    if (keyVariable.Contains(node.Name) || keyVariable.Contains(link.NodeName))
                        keyTimeStampSum.Add(link.TimeStamp);
                }
            }
            int factCount = factIndex.Count;
            HashSet<int> sum = new HashSet<int>();
            foreach (var ele in keyTimeStampSum)
                sum.Add(factIndex[ele - Variable.VariableSingle]);
            return sum.Sum();
        }
    }
}

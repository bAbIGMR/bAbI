using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class HandleFirstTask
    {
        int m_taskIndex;
        List<Session> m_sessionSet;
        List<int> sessionCountInEachTask = new List<int> {200,200,200,1000,200,
                                                          200,200,200,200,200,
                                                          200,200,200,200,250,
                                                          1000,125,198,1000,94};
        List<int> questionUpperPercent = new List<int> {10,10, 25, 25, 250,
                                                 10,200, 200, 10, 10,
                                                 10,10, 10, 10, 10,
                                                 15, 25, 10, 60, 10 };
        private HandleFirstTask()
        {
        }
        public HandleFirstTask(int taskIndex, List<Session> sessionSet)
        {
            m_taskIndex = taskIndex;
            m_sessionSet = sessionSet;
        }
        public int TaskIndex { get { return m_taskIndex; } }
        public List<Session> SessionSet { get { return m_sessionSet; } }
        public List<List<RuleStructure>> PipeLine(ref Dictionary<string, string> predicateSymbolDic, ref HashSet<string> variableHistorical, int taskId)
        {
            var preProcessedSession = PreProcessing(m_sessionSet);
            var processedQASession = FormatSession(preProcessedSession);
            var ruleSet = ProcessSessions(processedQASession, ref variableHistorical, taskId);
            PredicateProcessing(ref ruleSet, ref predicateSymbolDic);
            return ruleSet;
        }
        private void PredicateProcessing(ref List<List<RuleStructure>> ruleSet, ref Dictionary<string, string> predicateSymbolDic)
        {
            var result = PredicateTrans(ruleSet, ref predicateSymbolDic);
            ruleSet = result;
        }
        private List<List<RuleStructure>> PredicateTrans(List<List<RuleStructure>> ruleSet, ref Dictionary<string, string> predicateDic)
        {
            List<List<RuleStructure>> result = new List<List<RuleStructure>>();
            Predicate predicateObj = new Predicate(predicateDic.Count);
            foreach (var currentSessionRuleSet in ruleSet)
            {
                List<RuleStructure> newRuleStructureList = new List<RuleStructure>();
                foreach (var rule in currentSessionRuleSet)
                {
                    var tripleTermList = rule.Triple;
                    List<TripleTerm> newTripleTermList = new List<TripleTerm>();
                    foreach (var triple in tripleTermList)
                    {
                        var currentPredicate = triple.PredicateValue;
                        string transfedPredicate = currentPredicate;
                        if (predicateDic.ContainsKey(currentPredicate))
                            transfedPredicate = predicateDic[currentPredicate];
                        else
                        {
                            transfedPredicate = predicateObj.GetAvailiablePredicateSymbol;
                            predicateDic[currentPredicate] = transfedPredicate;
                        }
                        newTripleTermList.Add(new TripleTerm(triple.SubjectValue, transfedPredicate, triple.ObjectValue));
                    }
                    newRuleStructureList.Add(new RuleStructure(newTripleTermList, rule.QAStructure, rule.QuestionLength));
                }
                result.Add(newRuleStructureList);
            }
            return result;
        }

        private List<List<RuleStructure>> ProcessSessions(List<List<ProcessingUnit>> sessionSet, ref HashSet<string> variableSet, int taskId)
        {
            int sessionCounter = Variable.VariableZero;
            List<List<RuleStructure>> ruleSet = new List<List<RuleStructure>>();
            foreach (var session in sessionSet)
            {
                ++sessionCounter;
                Console.WriteLine(sessionCounter);
                HashSet<string> currentSessionVariableSet = new HashSet<string>();
                var currentSessionRuleSet = ProcessEachSession(new List<ProcessingUnit>(session), ref currentSessionVariableSet);
                ruleSet.Add(currentSessionRuleSet);
                variableSet.UnionWith(currentSessionVariableSet);
            }
            var result = UpdateRuleSet.Update(variableSet, ruleSet);
            return result;
        }


        private List<RuleStructure> ProcessEachSession(List<ProcessingUnit> currentSession, ref HashSet<string> variableHistorical)
        {
            List<RuleStructure> ruleSet = new List<RuleStructure>();
            HashSet<string> subjectSet = new HashSet<string>();
            HashSet<string> objectSet = new HashSet<string>();
            HashSet<string> predicateSet = new HashSet<string>();
            List<ProcessingUnit> accumulativeFact = new List<ProcessingUnit>();
            foreach (var unit in currentSession)
            {
                if (unit.GetTripleType == TripleType.Facts)
                {
                    var subject = unit.GetSubjectValue;
                    var predicate = unit.GetPredicateValue;
                    var objectValue = unit.GetObjectValue;
                    if (TripleRuleGeneration.checker.Contains(unit.GetSubjectValue))
                        subject = accumulativeFact[accumulativeFact.Count - Variable.VariableSingle].GetSubjectValue;
                    else
                        subject = unit.GetSubjectValue;
                    subjectSet.Add(subject);
                    predicateSet.Add(predicate);
                    objectSet.Add(objectValue);
                    accumulativeFact.Add(new ProcessingUnit(unit.GetTripleType, subject, predicate, objectValue, unit.GetQuestion, unit.GetAnswer, unit.GetAnswerRelated));
                }
                else
                {
                    List<string> question = new List<string>(unit.GetQuestion);
                    List<string> answer = new List<string>(unit.GetAnswer);
                    var combination = GetQACombination(subjectSet, objectSet, predicateSet, question, answer);
                    var currentQuestionAccumulation = GetCurrentQuestionAccumulation(accumulativeFact, combination);
                    Dictionary<string, string> currentAllVariableTrans = new Dictionary<string, string>();
                    var ruleStructure = GenerateRule(currentQuestionAccumulation, combination, ref variableHistorical, ref currentAllVariableTrans);
                    bool flag = true;
                    if (flag)
                        ruleSet.Add(ruleStructure);
                }
            }
            return ruleSet;
        }
      
        private bool IsProcessingUnitContains(List<ProcessingUnit> unitList, ProcessingUnit unit)
        {
            foreach (var ele in unitList)
                if (ele.GetSubjectValue == unit.GetSubjectValue && ele.GetPredicateValue == unit.GetPredicateValue
                    && ele.GetObjectValue == unit.GetObjectValue)
                    return true;
            return false;
        }
        private List<string> IsPerfect(RuleStructure currentRuleStructure, out bool flag)
        {
            flag = true;
            Variable variable = new Variable();
            var allVariable = variable.GetAllVariable;
            var triplelist = currentRuleStructure.Triple;
            HashSet<string> result = new HashSet<string>();
            foreach (var ele in triplelist)
            {
                var subjectValue = ele.SubjectValue;
                var objectValue = ele.ObjectValue;
                if (!subjectValue.Contains(Variable.SpaceString))
                    if (!allVariable.Contains(subjectValue))
                    {
                        flag = false;
                        result.Add(subjectValue);
                    }
                if (!objectValue.Contains(Variable.SpaceString))
                    if (!allVariable.Contains(objectValue))
                    {
                        flag = false;
                        result.Add(objectValue);
                    }
            }
            if (result.Count != Variable.VariableZero)
                return result.ToList();
            else return new List<string>();
        }

        private RuleStructure GenerateRule(List<ProcessingUnit> accumulation, QACombination currentQA, ref HashSet<string> variableHistorical, ref Dictionary<string, string> allVariableDic)
        {
            int questionLength = currentQA.Question.Count;
            List<KeyValuePair<string, VariableType>> qaStructure = new List<KeyValuePair<string, VariableType>>(currentQA.Question);
            qaStructure.AddRange(currentQA.Answer);
            Dictionary<string, string> variableTagger = new Dictionary<string, string>();
            Variable variable = new Variable();
            foreach (var ele in qaStructure)
            {
                if (ele.Value != VariableType.Unknown)
                {
                    if (!variableTagger.ContainsKey(ele.Key))
                        variableTagger[ele.Key] = variable.GetAvailiableVariableSymbol;
                    variableHistorical.Add(ele.Key);
                }
            }
            var ruleStructure = VariableTrans(variableTagger, accumulation, qaStructure, variableHistorical, questionLength, ref allVariableDic);
            return ruleStructure;
        }
        private RuleStructure VariableTrans(Dictionary<string, string> variableTagger, List<ProcessingUnit> accumulation, List<KeyValuePair<string, VariableType>> qaStructure, HashSet<string> variableHistorical, int questionLength, ref Dictionary<string, string> allVariableDic)
        {
            List<TripleTerm> targetTriple = new List<TripleTerm>();
            List<KeyValuePair<string, VariableType>> targetQA = new List<KeyValuePair<string, VariableType>>();
            int max = Variable.VariableZero;
            Variable variable = new Variable();
            foreach (var e in variableTagger)
            {
                int index = variable.GetVariableIndex(e.Value);
                if (index > max)
                    max = index;
            }
            Variable vObj = new Variable(max + Variable.VariableSingle);
            Dictionary<string, string> variableDic = new Dictionary<string, string>();
            foreach (var ele in accumulation)
            {
                var currentSubject = ele.GetSubjectValue;
                var currentObject = ele.GetObjectValue;
                var currentPredicate = ele.GetPredicateValue;
                for (int i = Variable.VariableZero; i < Variable.Threshold; i++)
                    foreach (var _variable in variableTagger)
                    {
                        if (currentSubject.Contains(_variable.Key))
                            if (Tools.IsWell(currentSubject, _variable.Key))
                                currentSubject = currentSubject.Replace(_variable.Key, _variable.Value);
                        if (currentObject.Contains(_variable.Key))
                            if (Tools.IsWell(currentObject, _variable.Key))
                                currentObject = currentObject.Replace(_variable.Key, _variable.Value);
                    }

                for (int i = Variable.VariableZero; i < Variable.Threshold; i++)
                    foreach (var oldVariable in variableHistorical)
                    {
                        if (currentSubject.Contains(oldVariable))
                        {
                            if (Tools.IsWell(currentSubject, oldVariable))
                            {
                                if (!variableDic.ContainsKey(oldVariable))
                                {
                                    var newSymbol = vObj.GetAvailiableVariableSymbol;
                                    variableDic[oldVariable] = newSymbol;
                                    currentSubject = currentSubject.Replace(oldVariable, variableDic[oldVariable]);
                                }
                                else
                                    currentSubject = currentSubject.Replace(oldVariable, variableDic[oldVariable]);
                            }
                        }
                        if (currentObject.Contains(oldVariable))
                        {
                            if (Tools.IsWell(currentObject, oldVariable))
                            {
                                if (!variableDic.ContainsKey(oldVariable))
                                {
                                    var newSymbol = vObj.GetAvailiableVariableSymbol;
                                    variableDic[oldVariable] = newSymbol;
                                    currentObject = currentObject.Replace(oldVariable, variableDic[oldVariable]);
                                }
                                else
                                    currentObject = currentObject.Replace(oldVariable, variableDic[oldVariable]);
                            }
                        }
                    }
                targetTriple.Add(new TripleTerm(currentSubject, currentPredicate, currentObject));
            }
            foreach (var ele in qaStructure)
            {
                if (variableTagger.ContainsKey(ele.Key))
                    targetQA.Add(new KeyValuePair<string, VariableType>(variableTagger[ele.Key], ele.Value));
                else
                    targetQA.Add(ele);
            }
            Dictionary<string, string> allTrans = new Dictionary<string, string>(variableDic);
            allVariableDic = allTrans.Union(variableTagger).ToDictionary(x => x.Key, x => x.Value);
            return new RuleStructure(targetTriple, targetQA, questionLength);
        }
        private List<ProcessingUnit> SortTimeElement(List<ProcessingUnit> accumulativeFact)
        {
            Dictionary<string, List<ProcessingUnit>> timeDic = new Dictionary<string, List<ProcessingUnit>>();
            List<ProcessingUnit> noTime = new List<ProcessingUnit>();
            List<ProcessingUnit> result = new List<ProcessingUnit>();
            foreach (var unit in accumulativeFact)
            {
                var currentSujebct = unit.GetSubjectValue;
                var currentObject = unit.GetObjectValue;
                bool flag = true;
                foreach (var timeKey in Time.TimeStamp)
                {
                    if (currentSujebct.Contains(timeKey))
                    {
                        currentSujebct = currentSujebct.Replace(timeKey, Variable.NullString);
                        currentSujebct = currentSujebct.Trim();
                        flag = false;
                        if (!timeDic.ContainsKey(timeKey))
                            timeDic[timeKey] = new List<ProcessingUnit> { new ProcessingUnit(unit.GetTripleType,
                                currentSujebct,unit.GetPredicateValue,unit.GetObjectValue,unit.GetQuestion,unit.GetAnswer,unit.GetAnswerRelated)};
                        else timeDic[timeKey].Add(new ProcessingUnit(unit.GetTripleType,
                                currentSujebct, unit.GetPredicateValue, unit.GetObjectValue, unit.GetQuestion, unit.GetAnswer, unit.GetAnswerRelated));
                    }
                    else if (currentObject.Contains(timeKey))
                    {
                        flag = false;
                        currentObject = currentObject.Replace(timeKey, Variable.NullString);
                        currentObject = currentObject.Trim();
                        if (!timeDic.ContainsKey(timeKey))
                            timeDic[timeKey] = new List<ProcessingUnit> { new ProcessingUnit(unit.GetTripleType,
                                unit.GetSubjectValue,unit.GetPredicateValue,currentObject,unit.GetQuestion,unit.GetAnswer,unit.GetAnswerRelated)};
                        else
                            timeDic[timeKey].Add(new ProcessingUnit(unit.GetTripleType,
                                unit.GetSubjectValue, unit.GetPredicateValue, currentObject, unit.GetQuestion, unit.GetAnswer, unit.GetAnswerRelated));
                    }
                }
                if (flag)
                    noTime.Add(unit);
            }
            foreach (var ele in Time.TimeStamp)
                if (timeDic.ContainsKey(ele))
                    result.AddRange(timeDic[ele]);
            if (noTime.Count != Variable.VariableZero)
                result.AddRange(noTime);
            return result;
        }
        private List<ProcessingUnit> GetCurrentQuestionAccumulation(List<ProcessingUnit> accumulativeFacts, QACombination currentQA)
        {
            var accumulativeFact = SortTimeElement(accumulativeFacts);
            List<KeyValuePair<string, VariableType>> QAStructure = new List<KeyValuePair<string, VariableType>>(currentQA.Question);
            List<int> indexList = new List<int>();
            QAStructure.AddRange(currentQA.Answer);
            for (int index = Variable.VariableZero; index < accumulativeFact.Count; index++)
                foreach (var ele in QAStructure)
                {
                    if (accumulativeFact[index].GetSubjectValue.Contains(ele.Key))
                        if (Tools.IsWell(accumulativeFact[index].GetSubjectValue, ele.Key))
                            if (!indexList.Contains(index))
                                indexList.Add(index);
                    if (accumulativeFact[index].GetObjectValue.Contains(ele.Key))
                        if (Tools.IsWell(accumulativeFact[index].GetObjectValue, ele.Key))
                            if (!indexList.Contains(index))
                                indexList.Add(index);
                }
            List<int> indexCopy = new List<int>(indexList);
            for (int i = Variable.VariableZero; i < accumulativeFact.Count; i++)
            {
                if (indexList.Contains(i))
                    continue;
                else
                    foreach (var ele in indexCopy)
                        if (accumulativeFact[ele].GetSubjectValue == accumulativeFact[i].GetSubjectValue ||
                            accumulativeFact[ele].GetObjectValue == accumulativeFact[i].GetObjectValue)
                            if (!indexList.Contains(i))
                                indexList.Add(i);
            }
            indexList.Sort();
            List<ProcessingUnit> result = new List<ProcessingUnit>();
            foreach (var unit in indexList)
                result.Add(accumulativeFact[unit]);
            return result;
        }


        private QACombination GetQACombination(HashSet<string> subjectSet, HashSet<string> objectSet, HashSet<string> predicateSet, List<string> question, List<string> answer)
        {
            var entitySet = new HashSet<string>(subjectSet);
            entitySet.UnionWith(objectSet);
            var subjectVariable = CalLength(subjectSet);
            var predicateVariable = CalLength(predicateSet);
            var objectVariable = CalLength(objectSet);

            var questionVariableInSubject = GetQuestionVariable(question, subjectVariable);
            var questionVariableInObject = GetQuestionVariable(question, objectVariable);
            var answerVariableInSubject = GetAnswerVariable(answer, subjectVariable);
            var answerVariableInObject = GetAnswerVariable(answer, objectVariable);

            var questionUnion = MergeTwoDic(questionVariableInSubject, questionVariableInObject);
            var answerUnion = MergeTwoDic(answerVariableInSubject, answerVariableInObject);

            var qaCombination = ConstructQACombination(questionUnion, answerUnion, question, answer);
            return qaCombination;
        }
        private Dictionary<string, string> MergeTwoDic(Dictionary<string, string> subjectDic, Dictionary<string, string> objectDic)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(subjectDic);
            foreach (var ele in objectDic)
            {
                if (!result.ContainsKey(ele.Key))
                    result[ele.Key] = ele.Value;
                else
                    if (result[ele.Key] != ele.Value)
                {
                    if (result[ele.Key] != ele.Key)
                        if (ele.Key == ele.Value)
                            result[ele.Key] = ele.Value;
                }
            }
            return result;
        }
        private QACombination ConstructQACombination(Dictionary<string, string> question, Dictionary<string, string> answer, List<string> questionString, List<string> answerString)
        {
            List<KeyValuePair<string, VariableType>> variableQuestionPropertyList = new List<KeyValuePair<string, VariableType>>();
            List<KeyValuePair<string, VariableType>> variableAnswerPropertyList = new List<KeyValuePair<string, VariableType>>();
            List<string> variableList = new List<string>();
            var _questionString = Variable.NullString;
            foreach (var e in questionString)
                _questionString += e + Variable.SpaceString;
            _questionString = _questionString.Trim();
            Dictionary<string, string> sortedQuestion = new Dictionary<string, string>(question);
            var lengthDic = CalLength(new HashSet<string>(sortedQuestion.Keys.ToList()));
            var dicSort = from d in lengthDic

                          orderby d.Value

                          descending

                          select d;
            var tempDic = dicSort.ToDictionary(x => x.Key, x => x.Value);
            List<KeyValuePair<int, string>> mark = new List<KeyValuePair<int, string>>();
            HashSet<int> indexed = new HashSet<int>();
            foreach (var ele in tempDic)
            {
                if (ele.Key != Variable.NullString)
                    if (_questionString.Contains(ele.Key))
                    {
                        var index = _questionString.IndexOf(ele.Key);
                        if (indexed.Contains(index))
                        {
                            index = _questionString.LastIndexOf(ele.Key);
                            if (indexed.Contains(index))
                                continue;
                        }
                        {
                            indexed.Add(index);
                            mark.Add(new KeyValuePair<int, string>(index, ele.Key));
                            var currentVariable = ele.Key;
                            VariableType type;
                            if (ele.Key != question[ele.Key])
                                type = VariableType.PartOfSubject;
                            else
                                type = VariableType.Subject;
                            variableQuestionPropertyList.Add(new KeyValuePair<string, VariableType>(currentVariable, type));
                        }
                    }

            }
            foreach (var ele in answerString)
            {
                VariableType type;
                if (answer.ContainsKey(ele))
                {
                    if (ele == answer[ele])
                        type = VariableType.Subject;
                    else
                        type = VariableType.PartOfSubject;
                    variableAnswerPropertyList.Add(new KeyValuePair<string, VariableType>(ele, type));
                }
            }
            var QACombination = ConstructQACombination(mark, _questionString, variableAnswerPropertyList, answerString);
            return QACombination;
        }
        private QACombination ConstructQACombination(List<KeyValuePair<int, string>> mark, string question, List<KeyValuePair<string, VariableType>> variableAnswerPropertyList, List<string> answerList)
        {
            List<KeyValuePair<string, VariableType>> _question = new List<KeyValuePair<string, VariableType>>();
            List<char> charList = new List<char>();
            Dictionary<int, int> index2index = new Dictionary<int, int>();
            for (int i = Variable.VariableZero; i < mark.Count; i++)
                index2index[mark[i].Key] = i;
            for (int i = Variable.VariableZero; i < question.Length; i++)
            {
                if (index2index.ContainsKey(i))
                {
                    if (charList.Count == Variable.VariableZero)
                    {
                        int length = mark[index2index[i]].Value.Length;
                        var subString = question.Substring(i, length);
                        _question.Add(new KeyValuePair<string, VariableType>(subString, VariableType.Subject));
                        i += length;
                    }
                    else
                    {
                        string temp = Variable.NullString;
                        foreach (var ele in charList)
                            temp += ele;
                        temp = temp.Trim();
                        charList.Clear();
                        int length = mark[index2index[i]].Value.Length;
                        _question.Add(new KeyValuePair<string, VariableType>(temp, VariableType.Unknown));
                        var subString = question.Substring(i, length);
                        _question.Add(new KeyValuePair<string, VariableType>(subString, VariableType.Subject));
                        i += length;
                    }
                }
                else
                    charList.Add(question[i]);
            }
            if (charList.Count != Variable.VariableZero)
            {
                string temp = Variable.NullString;
                foreach (var ele in charList)
                    temp += ele;
                temp = temp.Trim();
                _question.Add(new KeyValuePair<string, VariableType>(temp, VariableType.Unknown));
            }
            List<KeyValuePair<string, VariableType>> _answer = new List<KeyValuePair<string, VariableType>>();
            foreach (var ele in answerList)
            {
                var flag = IsPropersyListContains(ele, variableAnswerPropertyList);
                if (flag.Key == Variable.SpecialTag)
                    _answer.Add(new KeyValuePair<string, VariableType>(ele, VariableType.Unknown));
                else
                    _answer.Add(flag);
            }
            return new QACombination(_question, _answer);
        }
        private KeyValuePair<string, VariableType> IsPropersyListContains(string key, List<KeyValuePair<string, VariableType>> propertyList)
        {
            KeyValuePair<string, VariableType> result = new KeyValuePair<string, VariableType>(Variable.SpecialTag, VariableType.Unknown);
            foreach (var ele in propertyList)
                if (ele.Key == key)
                    return ele;
            return result;
        }
        private Dictionary<string, string> GetAnswerVariable(List<string> answer, Dictionary<string, int> variableSet)
        {
            Dictionary<string, string> word2Variable = new Dictionary<string, string>();
            foreach (var word in answer)
                foreach (var variable in variableSet)
                    if (variable.Key.Contains(word))
                        word2Variable[word] = variable.Key;
            return word2Variable;
        }
        private Dictionary<string, string> GetQuestionVariable(List<string> question, Dictionary<string, int> variableSet)
        {
            Dictionary<string, string> word2Variable = new Dictionary<string, string>();

            foreach (var word in question)
                foreach (var currentVariable in variableSet)
                {
                    if (currentVariable.Key.Contains(word))
                    {
                        if (currentVariable.Value == Variable.VariableSingle)
                            if (currentVariable.Key == word)
                                word2Variable[word] = currentVariable.Key;
                            else
                            {
                                var LCS = GetLCS(question, currentVariable.Key);
                                string result = Variable.NullString;
                                foreach (var ele in LCS)
                                    result += ele + Variable.SpaceString;
                                word2Variable[result.Trim()] = currentVariable.Key;
                            }
                    }
                    else
                        continue;
                }
            return word2Variable;
        }
        private List<string> GetLCS(List<string> question, string variableSet)
        {
            List<string> result = new List<string>();
            string[] wordArray = variableSet.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            var wordList = wordArray.ToList();
            for (int i = Variable.VariableZero; i < question.Count; i++)
            {
                var currentWord = question[i];
                if (wordList.Contains(currentWord))
                {
                    var index = wordList.IndexOf(currentWord);
                    List<string> temp = new List<string>();
                    for (int j = i; j < question.Count && index < wordList.Count; j++, index++)
                        if (question[j] == wordList[index])
                            temp.Add(question[j]);
                        else
                            break;
                    if (temp.Count > result.Count)
                        result = temp;
                }
                else
                    continue;
            }
            return result;
        }
        private Dictionary<string, int> CalLength(HashSet<string> variableSet)
        {
            Dictionary<string, int> variable2Length = new Dictionary<string, int>();
            foreach (var variable in variableSet)
            {
                string[] temp = variable.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                variable2Length[variable] = temp.Length;
            }
            return variable2Length;
        }
        private List<List<ProcessingUnit>> FormatSession(List<Session> sessionSet)
        {
            List<List<ProcessingUnit>> unitSet = new List<List<ProcessingUnit>>();
            foreach (var session in sessionSet)
            {
                var _session = session.session;
                List<ProcessingUnit> unitList = new List<ProcessingUnit>();
                foreach (var currentTriple in _session)
                {
                    if (currentTriple.tripleType == TripleType.Facts.ToString())
                        unitList.Add(new ProcessingUnit(TripleType.Facts, currentTriple.subjectValue,
                            currentTriple.predicateValue, currentTriple.objectValue, new List<string>(),
                            new List<string>(), currentTriple.answerRelated));
                    else
                    {
                        string question = currentTriple.question;
                        string answer = currentTriple.answer;
                        var qa = FormatQA(question, answer);
                        unitList.Add(new ProcessingUnit(TripleType.Question, currentTriple.subjectValue,
                            currentTriple.predicateValue, currentTriple.objectValue, qa.Question,
                            qa.Answer, currentTriple.answerRelated));
                    }
                }
                unitSet.Add(unitList);
            }
            return unitSet;
        }
        private QAStructure FormatQA(string question, string answer)
        {
            List<string> _question = new List<string>();
            List<string> _answer = new List<string>();
            string[] questionList = question.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            _question = questionList.ToList();
            if (answer.Contains(Variable.CommaString))
            {
                string[] answerList = answer.Split(new char[] { Variable.CommaChar }, StringSplitOptions.RemoveEmptyEntries);
                _answer = answerList.ToList();
            }
            else
                _answer = new List<string> { answer };
            return new QAStructure(_question, _answer);

        }

        private List<Session> PreProcessing(List<Session> sessionSet)
        {
            List<Session> result = new List<Session>();
            foreach (var currentSession in sessionSet)
            {
                var _session = currentSession.session;
                List<TripleSet> eachSession = new List<TripleSet>();
                foreach (var tripleSet in _session)
                {
                    if (tripleSet.tripleType == TripleType.Facts.ToString())
                    {
                        var temp = TriplePreProcessing(tripleSet);
                        eachSession.Add(new TripleSet(tripleSet.tripleType.ToString(),
                            temp.SubjectValue, temp.PredicateValue,
                            temp.ObjectValue, tripleSet.question,
                            tripleSet.answer, tripleSet.answerRelated));
                    }
                    else
                    {
                        string question = tripleSet.question;
                        string answer = tripleSet.answer;
                        QAPreProcessing(ref question, ref answer);
                        eachSession.Add(new TripleSet(tripleSet.tripleType.ToString(),
                            tripleSet.subjectValue, tripleSet.predicateValue,
                            tripleSet.objectValue, question,
                            answer, tripleSet.answerRelated));
                    }
                }
                result.Add(new Session(eachSession));
            }
            return result;
        }
        private TripleTerm TriplePreProcessing(TripleSet currentTriple)
        {
            return new TripleTerm(
                currentTriple.subjectValue.ToLower(),
                currentTriple.predicateValue.ToLower(),
                currentTriple.objectValue.ToLower());
        }
        private void QAPreProcessing(ref string question, ref string answer)
        {
            question = question.Replace(Variable.QChar, Variable.SpaceChar).Trim().ToLower();
            answer = answer.ToLower().Trim();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class Test
    {
        Dictionary<string, string> _predicateSymbolDic;
        Dictionary<string, string> symbol2Predicate = new Dictionary<string, string>();
        HashSet<string> _variableHistorical;
        List<RuleStructureCombination> _ruleSet;
        List<Session> _sessionSet;
        private Test() { }
        public Test(Dictionary<string, string> predicateSymbolDic, HashSet<string> variableHistorical, List<RuleStructureCombination> ruleSet, List<Session> sessionSet)
        {
            _predicateSymbolDic = predicateSymbolDic;
            _variableHistorical = variableHistorical;
            _ruleSet = ruleSet;
            _sessionSet = sessionSet;
            foreach (var ele in predicateSymbolDic)
                symbol2Predicate[ele.Value] = ele.Key;
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
        public double PipeLine()
        {
            var preProcessedSession = PreProcessing(_sessionSet);
            var processedQASession = FormatSession(preProcessedSession);
            var accuracy = ProcessSessions(processedQASession, ref _variableHistorical);
            return accuracy;
        }
        private List<List<RuleStructure>> PredicateProcessing(List<List<RuleStructure>> ruleSet, Dictionary<string, string> predicateSymbolDic)
        {
            var result = PredicateTrans(ruleSet, predicateSymbolDic);
            return result;
        }
        private List<List<RuleStructure>> PredicateTrans(List<List<RuleStructure>> ruleSet, Dictionary<string, string> predicateDic)
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
        private double ProcessSessions(List<List<ProcessingUnit>> sessionSet, ref HashSet<string> variableSet)
        {
            int sessionCounter = Variable.VariableZero;
            int allQuestion = Variable.VariableZero; int allSuccessful = Variable.VariableZero;
            foreach (var session in sessionSet)
            {
                ++sessionCounter;
                HashSet<string> currentSessionVariableSet = new HashSet<string>();
                int sum = Variable.VariableZero; int success = Variable.VariableZero;
                ProcessEachSession(new List<ProcessingUnit>(session), ref currentSessionVariableSet, out sum, out success);
                allQuestion += sum;
                allSuccessful += success;
            }
            double accuracy = (double)allSuccessful / (double)allQuestion;
            return accuracy;
        }
        private void ProcessEachSession(List<ProcessingUnit> currentSession, ref HashSet<string> variableHistorical, out int sum, out int success)
        {
            int questionSum = Variable.VariableZero;
            int successfulSum = Variable.VariableZero;
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
                    questionSum++;
                    bool IsFindingAnswer = false;
                    #region
                    List<GraphNode> currentSubGraph = new List<GraphNode>();
                    List<string> questionList = new List<string>(unit.GetQuestion);
                    List<string> exceptanswer = new List<string>(unit.GetAnswer);


                    string question = Variable.NullString;
                    foreach (var ele in questionList)
                        question += ele + Variable.SpaceString;
                    question = question.Trim();
                    AnswerStore answerListObj = new AnswerStore();

                    var toCompareRuleSet = GetRuleSet(question);
                    var keyPredicate = GetKeyPredicate(toCompareRuleSet);
                    List<List<GraphNode>> toCompare = new List<List<GraphNode>>();
                    List<RuleNameMatching> targetRuleSet = new List<RuleNameMatching>();
                    List<List<KeyValuePair<string, VariableType>>> answerList = new List<List<KeyValuePair<string, VariableType>>>();
                    #endregion
                    foreach (var ele in toCompareRuleSet)
                    {
                        bool FLAG = false;
                        #region 
                        Dictionary<string, string> allVariable = new Dictionary<string, string>();
                        var variableQ2RuleName = GetQuestionMap2RuleName(ele);
                        var reverseDic = new Dictionary<string, string>();
                        foreach (var e in variableQ2RuleName)
                            reverseDic[e.Value] = e.Key;
                        var qaStructure = ele.Target.QAStructure;
                        var qLength = ele.Target.QuestionLength;
                        List<KeyValuePair<string, VariableType>> questionStructure = new List<KeyValuePair<string, VariableType>>();
                        List<KeyValuePair<string, VariableType>> answerStructure = new List<KeyValuePair<string, VariableType>>();
                        for (int i = Variable.VariableZero; i < qaStructure.Count; i++)
                            if (i >= qLength)
                            {
                                if (qaStructure[i].Key.Contains(Variable.SpaceString))
                                {
                                    string[] aArray = qaStructure[i].Key.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var a in aArray)
                                        answerStructure.Add(new KeyValuePair<string, VariableType>(a, qaStructure[i].Value));
                                }
                                else
                                    answerStructure.Add(qaStructure[i]);
                            }
                            else questionStructure.Add(qaStructure[i]);
                        VariableType expectanswerType = answerStructure[Variable.VariableZero].Value;

                        var questionStructureCopy = new List<KeyValuePair<string, VariableType>>(questionStructure);
                        for (int i = Variable.VariableZero; i < questionStructureCopy.Count; i++)
                        {
                            if (questionStructureCopy[i].Value != VariableType.Unknown)
                                questionStructureCopy[i] = new KeyValuePair<string, VariableType>(reverseDic[questionStructureCopy[i].Key], questionStructureCopy[i].Value);
                        }
                        #endregion
                        if (expectanswerType == VariableType.Unknown)
                        {
                            #region
                            List<int> factSelectIndex = new List<int>();
                            QACombination combination = new QACombination(questionStructureCopy, answerStructure);
                            var currentQuestionAccumulation = GetCurrentQuestionAccumulation(accumulativeFact, combination, ref factSelectIndex);
                            var ruleStructure = GenerateRule(currentQuestionAccumulation, combination, ref variableHistorical, ref allVariable, variableQ2RuleName);
                            var currentRuleStructureUpdate = UpdateRuleSet.Update(_variableHistorical, new List<List<RuleStructure>> { new List<RuleStructure> { ruleStructure } });
                            var predicateTransfed = PredicateProcessing(currentRuleStructureUpdate, _predicateSymbolDic);

                            FactUnit factUnit = new FactUnit(predicateTransfed[Variable.VariableZero][Variable.VariableZero].Triple);
                            var currentgraphStructure = BuildCurrentGraphStructure(factUnit);
                            var nodeList = currentgraphStructure.CurrentGraphStructure;
                            var subgraph = ShrinkGraphNodeCollection(nodeList, currentRuleStructureUpdate[Variable.VariableZero][Variable.VariableZero].QAStructure);
                            currentSubGraph = subgraph;
                            int temp = Variable.VariableZero;
                            toCompare.Add(subgraph);
                            targetRuleSet.Add(ele);
                            answerList.Add(answerStructure);
                            #endregion
                            foreach (var targetSubgraph in ele.Target.GraphStructureList)
                            {
                                temp++;
                                var current = FurtherRemoveEdge(subgraph, keyPredicate);
                                var target = FurtherRemoveEdge(targetSubgraph.CurrentGraphStructure, keyPredicate);
                                var flag = SubGraphMatch(current, target, ele);

                                if (flag)
                                {
                                    var isAnswer = CompareAnswer(exceptanswer, answerStructure);
                                    if (isAnswer)
                                        successfulSum++;
                                    IsFindingAnswer = true;
                                    FLAG = true;
                                    break;
                                }
                            }
                            if (FLAG)
                                break;
                        }
                        else
                        {
                            var answerListLength = answerStructure.Count;
                            var allVariableInFact = GetAllVariableInFacts(accumulativeFact, question);
                            var tryingAnswerList = GetTryingAnswer(answerListLength, allVariableInFact);
                            foreach (var tryingAnswer in tryingAnswerList)
                            {
                                #region
                                Dictionary<string, string> qa2RuleName = new Dictionary<string, string>(variableQ2RuleName);
                                Dictionary<string, string> tempA2RuleAnswer = new Dictionary<string, string>();
                                bool continueFlag = false;
                                for (int i = Variable.VariableZero; i < tryingAnswer.Count; i++)
                                    if (tempA2RuleAnswer.ContainsKey(tryingAnswer[i].Key))
                                    { continueFlag = true; break; }
                                    else
                                        tempA2RuleAnswer[tryingAnswer[i].Key] = answerStructure[i].Key;
                                if (continueFlag)
                                    continue;
                                bool isContinue = false;
                                foreach (var eee in tempA2RuleAnswer)
                                    if (qa2RuleName.ContainsKey(eee.Key))
                                    {
                                        isContinue = true;
                                        break;
                                    }
                                    else
                                        qa2RuleName[eee.Key] = eee.Value;
                                if (isContinue)
                                    continue;
                                bool isCurrentAnswer = false;
                                List<int> factSelectIndex = new List<int>();
                                QACombination combination = new QACombination(questionStructureCopy, tryingAnswer);
                                var currentQuestionAccumulation = GetCurrentQuestionAccumulation(accumulativeFact, combination, ref factSelectIndex);
                                var ruleStructure = GenerateRule(currentQuestionAccumulation, combination, ref variableHistorical, ref allVariable, qa2RuleName);
                                var currentRuleStructureUpdate = UpdateRuleSet.Update(_variableHistorical, new List<List<RuleStructure>> { new List<RuleStructure> { ruleStructure } });
                                var predicateTransfed = PredicateProcessing(currentRuleStructureUpdate, _predicateSymbolDic);
                                FactUnit factUnit = new FactUnit(predicateTransfed[Variable.VariableZero][Variable.VariableZero].Triple);
                                var currentgraphStructure = BuildCurrentGraphStructure(factUnit);
                                var nodeList = currentgraphStructure.CurrentGraphStructure;
                                var subgraph = ShrinkGraphNodeCollection(nodeList, currentRuleStructureUpdate[Variable.VariableZero][Variable.VariableZero].QAStructure);

                                toCompare.Add(subgraph);
                                targetRuleSet.Add(ele);
                                answerList.Add(tryingAnswer);


                                var tryAnswerStructure = new List<KeyValuePair<string, VariableType>>(currentRuleStructureUpdate[Variable.VariableZero][Variable.VariableZero].QAStructure);
                                tryAnswerStructure.RemoveRange(Variable.VariableZero, qLength);
                                int temp = Variable.VariableZero;
                                #endregion
                                foreach (var targetsubgraph in ele.Target.GraphStructureList)
                                {
                                    temp++;
                                    var subGraphStructure = GraphStructureTrans(new SubGraphStructure(currentgraphStructure.NodeNameSet,
                                            currentgraphStructure.EdgeCount, subgraph), targetsubgraph, tryAnswerStructure, answerStructure);

                                    var first = FurtherRemoveEdge(subGraphStructure.CurrentGraphStructure, keyPredicate);
                                    var second = FurtherRemoveEdge(targetsubgraph.CurrentGraphStructure, keyPredicate);

                                    var flag = SubGraphMatch(first, second, ele);
                                    if (flag)
                                    {
                                        var isAnswer = CompareAnswer(exceptanswer, tryingAnswer);
                                        if (isAnswer)
                                            successfulSum++;
                                        IsFindingAnswer = true;
                                        FLAG = true;
                                        isCurrentAnswer = true;
                                        break;
                                    }
                                }
                                if (isCurrentAnswer)
                                    break;
                            }
                        }
                        if (FLAG)
                            break;
                    }
                    if (!IsFindingAnswer)
                    {
                        var indexList = Incoherent(toCompare, targetRuleSet);
                        if (indexList.Count != Variable.VariableZero)
                        {
                            var flag = CompareAnswer(exceptanswer, answerList[indexList[Variable.VariableZero]]);
                            if (flag)
                                successfulSum++;
                        }
                    }
                }
            }
            sum = questionSum;
            success = successfulSum;
        }

        private List<int> Incoherent(List<List<GraphNode>> toCompare, List<RuleNameMatching> targetRule)
        {
            List<int> result = new List<int>();
            for (int i = Variable.VariableZero; i < toCompare.Count; i++)
            {
                var currentSubGraph = toCompare[i];
                var targetList = targetRule[i].Target.GraphStructureList;
                var currentNewGraph = LeaveOneEdge(currentSubGraph);
                foreach (var ele in targetList)
                {
                    var target = LeaveOneEdge(ele.CurrentGraphStructure);
                    var flag = SubGraphMatch(currentNewGraph, target, targetRule[i]);
                    if (flag)
                        result.Add(i);
                }
            }
            return result;
        }
        private List<GraphNode> LeaveOneEdge(List<GraphNode> graph)
        {
            List<GraphNode> result = new List<GraphNode>();
            foreach (var node in graph)
            {
                var outlink = node.OutLinks;
                var inlink = node.InLinks;
                List<GraphEdge> newOutlink = new List<GraphEdge>();
                List<GraphEdge> newInlink = new List<GraphEdge>();
                foreach (var link in outlink)
                    if (newOutlink.Count == Variable.VariableZero)
                        newOutlink.Add(link);
                    else
                    {
                        if (link.TimeStamp > newOutlink[Variable.VariableZero].TimeStamp)
                            newOutlink[Variable.VariableZero] = link;
                    }
                foreach (var link in inlink)
                    if (newInlink.Count == Variable.VariableZero)
                        newInlink.Add(link);
                    else
                    {
                        if (link.TimeStamp > newInlink[Variable.VariableZero].TimeStamp)
                            newInlink[Variable.VariableZero] = link;
                    }
                List<GraphEdge> newOutlinkCopy = new List<GraphEdge>();
                List<GraphEdge> newInlinkCopy = new List<GraphEdge>();
                Dictionary<string, int> nodeName2TimestampOutLink = new Dictionary<string, int>();
                Dictionary<string, int> nodeName2TimestampInLink = new Dictionary<string, int>();
                HashSet<int> toRemove = new HashSet<int>();
                foreach (var ele in newOutlink)
                    nodeName2TimestampOutLink[ele.NodeName] = ele.TimeStamp;
                foreach (var ele in newInlink)
                    nodeName2TimestampInLink[ele.NodeName] = ele.TimeStamp;

                foreach (var ele in nodeName2TimestampOutLink)
                    if (nodeName2TimestampInLink.ContainsKey(ele.Key))
                        toRemove.Add(ele.Value > nodeName2TimestampInLink[ele.Key] ? nodeName2TimestampInLink[ele.Key] : ele.Value);
                foreach (var ele in newOutlink)
                    if (!toRemove.Contains(ele.TimeStamp))
                        newOutlinkCopy.Add(ele);
                foreach (var ele in newInlink)
                    if (!toRemove.Contains(ele.TimeStamp))
                        newInlinkCopy.Add(ele);
                result.Add(new GraphNode(node.Name, newOutlinkCopy, newInlinkCopy));
            }
            return result;
        }
        private List<GraphNode> FurtherRemoveEdge(List<GraphNode> nodes, HashSet<string> keyPredicate)
        {
            List<GraphNode> result = new List<GraphNode>();
            if (keyPredicate.Count == Variable.VariableZero)
                return nodes;
            foreach (var ele in nodes)
            {
                List<GraphEdge> newOutEdge = new List<GraphEdge>();
                List<GraphEdge> newInEdge = new List<GraphEdge>();
                foreach (var outlink in ele.OutLinks)
                    if (keyPredicate.Contains(GetEquClassName(outlink.Predicate)))
                        newOutEdge.Add(outlink);
                foreach (var inlink in ele.InLinks)
                    if (keyPredicate.Contains(GetEquClassName(inlink.Predicate)))
                        newInEdge.Add(inlink);
                result.Add(new GraphNode(ele.Name, newOutEdge, newInEdge));
            }
            return result;
        }
        private HashSet<string> GetKeyPredicate(List<RuleNameMatching> units)
        {

            HashSet<string> isRelated = new HashSet<string>();
            HashSet<string> notRelated = new HashSet<string>();
            foreach (var unit in units)
            {
                foreach (var ele in unit.Target.GraphStructureList)
                {
                    var nodes = ele.CurrentGraphStructure;
                    int edgeCount = Variable.VariableZero;
                    foreach (var node in nodes)
                        edgeCount += node.OutLinks.Count;
                    if (edgeCount == Variable.VariableSingle)
                    {
                        foreach (var e in nodes)
                            foreach (var ee in e.OutLinks)
                                isRelated.Add(GetEquClassName(ee.Predicate));
                        foreach (var node in nodes)
                            foreach (var e in node.InLinks)
                                isRelated.Add(GetEquClassName(e.Predicate));
                    }

                }
            }

            if (units.Count > Variable.VariableSingle)
                foreach (var first in units)
                    foreach (var second in units)
                    {
                        var firstQA = first.Target.QAStructure;
                        var secondQA = second.Target.QAStructure;
                        var firstQLength = first.Target.QuestionLength;
                        var secondQLength = second.Target.QuestionLength;
                        foreach (var e in first.Target.GraphStructureList)
                            foreach (var ee in second.Target.GraphStructureList)
                                if (IsSingleFact(e.CurrentGraphStructure) && IsTwoFact(ee.CurrentGraphStructure))
                                    if (SameQuestonDiffAnswer(firstQA, firstQLength, secondQA, secondQLength))
                                        if (IsOneEdgeSame(e.CurrentGraphStructure, ee.CurrentGraphStructure, ref isRelated))
                                            return isRelated;
                    }
            return isRelated;
        }

        private bool IsOneEdgeSame(List<GraphNode> first, List<GraphNode> second, ref HashSet<string> related)
        {
            foreach (var oneOutlinkNode in first)
                if (oneOutlinkNode.OutLinks.Count == Variable.VariableSingle)
                    foreach (var twoOutLinkNode in second)
                        if (twoOutLinkNode.OutLinks.Count == Variable.VariableDouble)
                            if (IsSameClass(oneOutlinkNode.OutLinks[Variable.VariableZero].Predicate, twoOutLinkNode.OutLinks[Variable.VariableZero].Predicate) &&
                               oneOutlinkNode.Name == twoOutLinkNode.Name &&
                               oneOutlinkNode.OutLinks[Variable.VariableZero].NodeName == twoOutLinkNode.OutLinks[Variable.VariableZero].NodeName &&
                               (!IsSameClass(twoOutLinkNode.OutLinks[Variable.VariableZero].Predicate, twoOutLinkNode.OutLinks[Variable.VariableSingle].Predicate)))
                            {
                                related.Add(GetEquClassName(twoOutLinkNode.OutLinks[1].Predicate));
                                return true;
                            }

            return false;
        }
        private bool SameQuestonDiffAnswer(List<KeyValuePair<string, VariableType>> firstQA, int firstQlength,
            List<KeyValuePair<string, VariableType>> secQA, int secQlength)
        {
            if (IsSameQ(firstQA, firstQlength, secQA, secQlength) && IsDiffA(firstQA, firstQlength, secQA, secQlength))
                return true;
            return false;
        }
        private bool IsSameQ(List<KeyValuePair<string, VariableType>> firstQA, int firstQlength,
            List<KeyValuePair<string, VariableType>> secQA, int secQlength)
        {
            if (firstQlength == secQlength)
            {
                bool flag = true;
                for (int i = Variable.VariableZero; i < firstQlength; i++)
                    if (firstQA[i].Key != secQA[i].Key)
                        flag = false;
                if (flag)
                    return true;
            }
            return false;
        }
        private bool IsDiffA(List<KeyValuePair<string, VariableType>> firstQA, int firstQlength,
            List<KeyValuePair<string, VariableType>> secQA, int secQlength)
        {
            var firstQACopy = new List<KeyValuePair<string, VariableType>>(firstQA);
            var secondQACopy = new List<KeyValuePair<string, VariableType>>(secQA);

            firstQACopy.RemoveRange(Variable.VariableZero, firstQlength);
            secondQACopy.RemoveRange(Variable.VariableZero, secQlength);
            if (firstQACopy.Count != secondQACopy.Count)
                return true;
            for (int i = Variable.VariableZero; i < firstQACopy.Count; i++)
                if (firstQACopy[i].Key != secondQACopy[i].Key)
                    return true;
            return false;
        }
        private bool IsSingleFact(List<GraphNode> graph)
        {
            if (graph.Count == Variable.VariableDouble)
                if (graph[Variable.VariableZero].OutLinks.Count + graph[Variable.VariableSingle].OutLinks.Count == Variable.VariableSingle
                    && graph[Variable.VariableZero].InLinks.Count + graph[Variable.VariableSingle].InLinks.Count == Variable.VariableSingle)
                    return true;
            return false;
        }
        private bool IsTwoFact(List<GraphNode> graph)
        {
            if (graph.Count == Variable.VariableSingle)
                if (graph[Variable.VariableZero].OutLinks.Count == Variable.VariableDouble)
                    return true;
            return false;
        }
        private bool IsAnswerEqual(List<KeyValuePair<string, VariableType>> first, List<KeyValuePair<string, VariableType>> second)
        {
            if (first.Count != second.Count)
                return false;
            if (first[Variable.VariableZero].Value == VariableType.Unknown)
            {
                for (int i = Variable.VariableZero; i < first.Count; i++)
                {
                    if (first[i].Key != second[i].Key)
                        return false;
                }
            }
            else
            {
                foreach (var ele in first)
                {
                    bool flag = false;
                    foreach (var e in second)
                        if (e.Key == ele.Key)
                            flag = true;
                    if (!flag)
                        return false;
                }
            }
            return true;
        }
        private List<SubGraphStructure> GetIntersectionInSameRuleSet(RuleNameMatching unit)
        {
            List<SubGraphStructure> result = new List<SubGraphStructure>();
            var currentTarget = unit.Target;
            var qaStructure = currentTarget.QAStructure;
            var qLength = currentTarget.QuestionLength;
            var graphList = currentTarget.GraphStructureList;
            HashSet<string> keyNodeList = new HashSet<string>();
            foreach (var ele in qaStructure)
                if (ele.Value != VariableType.Unknown)
                    keyNodeList.Add(ele.Key);
            for (int i = Variable.VariableZero; i < graphList.Count; i++)
            {
                for (int j = i + Variable.VariableSingle; j < graphList.Count; j++)
                {
                    var firstGraph = graphList[i];
                    var secondGraph = graphList[j];
                    var newSubGraphRule = IntersectTwoGraph(firstGraph, secondGraph, keyNodeList);
                    result.Add(new SubGraphStructure(graphList[i].NodeNameSet, graphList[i].EdgeCount, newSubGraphRule));
                }
            }
            return result;
        }
        private List<GraphNode> IntersectTwoGraph(SubGraphStructure first, SubGraphStructure second, HashSet<string> keyNodeSet)
        {
            var firstGraph = first.CurrentGraphStructure;
            var secondGraph = second.CurrentGraphStructure;
            HashSet<int> allEdgeTimeStampInFirst = new HashSet<int>();
            HashSet<int> allEdgeTimeStampInSecond = new HashSet<int>();
            foreach (var ele in firstGraph)
            {
                foreach (var outlink in ele.OutLinks)
                    allEdgeTimeStampInFirst.Add(outlink.TimeStamp);
                foreach (var inlink in ele.InLinks)
                    allEdgeTimeStampInFirst.Add(inlink.TimeStamp);
            }
            foreach (var ele in secondGraph)
            {
                foreach (var outlink in ele.OutLinks)
                    allEdgeTimeStampInSecond.Add(outlink.TimeStamp);
                foreach (var inlink in ele.InLinks)
                    allEdgeTimeStampInSecond.Add(inlink.TimeStamp);
            }
            if (IsTwoGraphHaveSameStructure(firstGraph, secondGraph))
            {
                List<GraphNode> result = new List<GraphNode>();
                foreach (var firstNode in firstGraph)
                {
                    var secondNode = GetSameNameNode(firstNode.Name, secondGraph);
                    var newNode = GetCommonEdge(firstNode, secondNode, keyNodeSet, allEdgeTimeStampInFirst, allEdgeTimeStampInSecond);
                    result.Add(newNode);
                }
                return result;
            }
            else
                return new List<GraphNode>();
        }
        private List<GraphNode> IntersectionGeneration(List<GraphNode> firstGraph, List<GraphNode> secondGraph)
        {
            if (IsTwoGraphHaveSameStructure(firstGraph, secondGraph))
            {
                List<GraphNode> result = new List<GraphNode>();
                foreach (var firstNode in firstGraph)
                {
                    var secondNode = GetSameNameNode(firstNode.Name, secondGraph);
                    var newNode = GetCommonEdge(firstNode, secondNode);
                    result.Add(newNode);
                }
                return result;
            }
            else
                return new List<GraphNode>();
        }
        private GraphNode GetCommonEdge(GraphNode first, GraphNode second)
        {
            var firstOutlinks = first.OutLinks;
            var firstInlinks = first.InLinks;
            var secondOutlinks = second.OutLinks;
            var secondInlinks = second.InLinks;
            var commOutlink = GetCommonEdgeList(firstOutlinks, secondOutlinks);
            var commInlink = GetCommonEdgeList(firstInlinks, secondInlinks);
            GraphNode newGraphNode = new GraphNode(first.Name, commOutlink, commInlink);
            return newGraphNode;
        }

        private GraphNode GetCommonEdge(GraphNode first, GraphNode second, HashSet<string> keyNode, HashSet<int> allTimeStampInFirst, HashSet<int> allTimeStampInSecond)
        {
            var firstOutlinks = first.OutLinks;
            var firstInlinks = first.InLinks;
            var secondOutlinks = second.OutLinks;
            var secondInlinks = second.InLinks;
            var commOutlink = GetCommonEdgeList(firstOutlinks, secondOutlinks, keyNode, allTimeStampInFirst, allTimeStampInSecond);
            var commInlink = GetCommonEdgeList(firstInlinks, secondInlinks, keyNode, allTimeStampInFirst, allTimeStampInSecond);
            GraphNode newGraphNode = new GraphNode(first.Name, commOutlink, commInlink);
            return newGraphNode;
        }
        private List<GraphEdge> GetCommonEdgeList(List<GraphEdge> first, List<GraphEdge> second)
        {
            List<GraphEdge> result = new List<GraphEdge>();
            foreach (var firstEdge in first)
                foreach (var secEdge in second)
                    if (IsSameClass(firstEdge.Predicate, secEdge.Predicate) && firstEdge.NodeName == secEdge.NodeName)
                        result.Add(firstEdge);
            return result;
        }
        private List<GraphEdge> GetCommonEdgeList(List<GraphEdge> first, List<GraphEdge> second, HashSet<string> keyNode, HashSet<int> allTimeStampInFirst, HashSet<int> allTimeStampInSecond)
        {
            List<GraphEdge> result = new List<GraphEdge>();
            foreach (var firstEdge in first)
                foreach (var secEdge in second)
                    if (IsSameClass(firstEdge.Predicate, secEdge.Predicate) && firstEdge.NodeName == secEdge.NodeName)
                        result.Add(firstEdge);
            return result;
        }
        private bool IsSameRelativeOrder(HashSet<int> first, HashSet<int> second, GraphEdge firstEdge, GraphEdge secondEdge)
        {
            return true;
        }
        private bool IsTwoGraphHaveSameStructure(List<GraphNode> first, List<GraphNode> second)
        {
            if (first.Count != second.Count)
                return false;
            else
                foreach (var firstNode in first)
                    if (!IsContainCurrentNode(second, firstNode.Name))
                        return false;
            return true;
        }
        private GraphNode GetSameNameNode(string nodeName, List<GraphNode> nodeList)
        {
            foreach (var ele in nodeList)
                if (nodeName == ele.Name)
                    return ele;
            throw new Exception();
        }
        private bool IsContainCurrentNode(List<GraphNode> graph, string name)
        {
            foreach (var ele in graph)
                if (ele.Name == name)
                    return true;
            return false;
        }
        private SubGraphStructure GraphStructureTrans(SubGraphStructure source, SubGraphStructure target,
            List<KeyValuePair<string, VariableType>> sourceQA, List<KeyValuePair<string, VariableType>> targetQA)
        {
            Dictionary<string, string> variableTransDic = new Dictionary<string, string>();//trying answer 2 expect answer
            for (int i = Variable.VariableZero; i < sourceQA.Count; i++)
                if (sourceQA[i].Key != targetQA[i].Key)
                    variableTransDic[sourceQA[i].Key] = targetQA[i].Key;
            if (variableTransDic.Count == Variable.VariableZero)
                return source;
            else
            {
                HashSet<string> toReplactInSource = new HashSet<string>();
                foreach (var ele in variableTransDic)
                    if (!variableTransDic.ContainsKey(ele.Value))
                        toReplactInSource.Add(ele.Value);
                Dictionary<string, string> firstChanged = new Dictionary<string, string>();

                var newFacts = VariableReplaceInFact(source.NodeNameSet, source.CurrentGraphStructure, toReplactInSource, ref firstChanged, variableTransDic);
                var symmetryPairs = GetSymmetryPairs(variableTransDic);

                Dictionary<string, string> restOfQa2RuleName = new Dictionary<string, string>();
                var midFacts = SymmetryTrans(newFacts, symmetryPairs);

                foreach (var e in variableTransDic)
                {
                    if (!firstChanged.ContainsKey(e.Key))
                        restOfQa2RuleName[e.Key] = e.Value;
                    if (!symmetryPairs.ContainsKey(e.Key) && !symmetryPairs.ContainsKey(e.Value))
                        restOfQa2RuleName[e.Key] = e.Value;
                }
                var transOrder = GetTransOrder(restOfQa2RuleName);
                var finalGraphNodeList = ReplaceVariableInFact(restOfQa2RuleName, midFacts, transOrder);
                return new SubGraphStructure(source.NodeNameSet, source.EdgeCount, finalGraphNodeList);
            }
        }
        private List<string> Swap(List<string> source, int i, int j)
        {
            List<string> temp = new List<string>(source);
            temp[i] = source[j];
            temp[j] = source[i];
            return temp;
        }
        private List<KeyValuePair<string, string>> GetTransOrder(Dictionary<string, string> qa2RuleName)
        {
            List<KeyValuePair<string, string>> order = new List<KeyValuePair<string, string>>();
            HashSet<string> keyList = new HashSet<string>(qa2RuleName.Keys.ToList());
            HashSet<string> valueList = new HashSet<string>(qa2RuleName.Values.ToList());
            keyList.IntersectWith(valueList);
            var commonList = keyList.ToList();
            if (commonList.Count > Variable.VariableSingle)
            {
                for (int i = Variable.VariableZero; i < commonList.Count; i++)
                    for (int j = Variable.VariableZero; j < commonList.Count; j++)
                        if (j != i)
                        {
                            var one = commonList[i];
                            var another = commonList[j];
                            if (qa2RuleName[one] == another)
                                if (j > i)
                                    commonList = Swap(commonList, i, j);
                        }
                foreach (var ele in commonList)
                    order.Add(new KeyValuePair<string, string>(ele, qa2RuleName[ele]));
                return order;
            }
            else if (commonList.Count == Variable.VariableSingle)
            {
                order.Add(new KeyValuePair<string, string>(commonList[Variable.VariableZero], qa2RuleName[commonList[Variable.VariableZero]]));
                return order;
            }
            else
                return order;
        }
        private List<GraphNode> SymmetryTrans(List<GraphNode> facts, Dictionary<string, string> SymmetryPair)
        {
            List<GraphNode> result = new List<GraphNode>(facts);
            foreach (var pair in SymmetryPair)
            {
                for (int i = Variable.VariableZero; i < result.Count; i++)
                {
                    var currentNode = result[i];
                    var currentName = currentNode.Name;
                    var currentOutlinks = currentNode.OutLinks;
                    var currentInlinks = currentNode.InLinks;
                    List<GraphEdge> newInlinks = new List<GraphEdge>();
                    List<GraphEdge> newOutLinks = new List<GraphEdge>();
                    {
                        if (currentName.Contains(pair.Key))
                            currentName = currentName.Replace(pair.Key, pair.Value);
                        else if (currentName.Contains(pair.Value))
                            currentName = currentName.Replace(pair.Value, pair.Key);
                        foreach (var outlink in currentOutlinks)
                            if (outlink.NodeName.Contains(pair.Key))
                                newOutLinks.Add(new GraphEdge(outlink.Predicate, outlink.TimeStamp, outlink.NodeName.Replace(pair.Key, pair.Value)));
                            else if (outlink.NodeName.Contains(pair.Value))
                                newOutLinks.Add(new GraphEdge(outlink.Predicate, outlink.TimeStamp, outlink.NodeName.Replace(pair.Value, pair.Key)));
                            else
                                newOutLinks.Add(outlink);
                        foreach (var inlink in currentInlinks)
                            if (inlink.NodeName.Contains(pair.Key))
                                newInlinks.Add(new GraphEdge(inlink.Predicate, inlink.TimeStamp, inlink.NodeName.Replace(pair.Key, pair.Value)));
                            else if (inlink.NodeName.Contains(pair.Value))
                                newInlinks.Add(new GraphEdge(inlink.Predicate, inlink.TimeStamp, inlink.NodeName.Replace(pair.Value, pair.Key)));
                            else
                                newInlinks.Add(inlink);
                    }
                    result[i] = new GraphNode(currentName, newOutLinks, newInlinks);
                }
            }
            return result;
        }
        private Dictionary<string, string> GetSymmetryPairs(Dictionary<string, string> qa2RuleName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var ele in qa2RuleName)
                if (qa2RuleName.ContainsKey(ele.Value) && (ele.Key == qa2RuleName[ele.Value]))
                    if (!result.ContainsKey(ele.Value))
                        result[ele.Key] = ele.Value;
            return result;
        }
        private List<GraphNode> VariableReplaceInFact(HashSet<string> currentVariableSetInFact, List<GraphNode> subGraph,
            HashSet<string> toReplaceInFact, ref Dictionary<string, string> firstChanged, Dictionary<string, string> qa2RuleName)
        {
            int max = Variable.VariableZero;
            Variable _var = new Variable();
            foreach (var ele in currentVariableSetInFact)
            {
                var index = _var.GetVariableIndex(ele);
                if (index > max)
                    max = index;
            }
            Variable variable = new Variable(max + Variable.VariableSingle);
            Dictionary<string, string> variableInFact = new Dictionary<string, string>();
            foreach (var ele in toReplaceInFact)
            {
                if (!variableInFact.ContainsKey(ele))
                {
                    var newSymbol = variable.GetAvailiableVariableSymbol;
                    for (int i = Variable.VariableZero; i < 10; i++)
                        if (!qa2RuleName.ContainsKey(newSymbol))
                        {
                            variableInFact[ele] = newSymbol;
                            break;
                        }
                        else newSymbol = variable.GetAvailiableVariableSymbol;
                }
            }
            firstChanged = variableInFact;
            var newTripleList = ReplaceVariableInFact(variableInFact, subGraph, new List<KeyValuePair<string, string>>());
            return newTripleList;
        }
        private List<GraphNode> ReplaceVariableInFact(Dictionary<string, string> toReplace, List<GraphNode> subGraph, List<KeyValuePair<string, string>> order)
        {
            List<GraphNode> result = new List<GraphNode>(subGraph);
            foreach (var orderUnit in order)
            {
                for (int i = Variable.VariableZero; i < result.Count; i++)
                {
                    var currentNode = result[i];
                    var currentName = currentNode.Name;
                    var currentOutlinks = currentNode.OutLinks;
                    var currentInlinks = currentNode.InLinks;
                    List<GraphEdge> newInlinks = new List<GraphEdge>();
                    List<GraphEdge> newOutLinks = new List<GraphEdge>();
                    if (currentName.Contains(orderUnit.Key))
                        currentName = currentName.Replace(orderUnit.Key, orderUnit.Value);
                    foreach (var outlink in currentOutlinks)
                        if (outlink.NodeName.Contains(orderUnit.Key))
                            newOutLinks.Add(new GraphEdge(outlink.Predicate, outlink.TimeStamp, outlink.NodeName.Replace(orderUnit.Key, orderUnit.Value)));
                        else
                            newOutLinks.Add(outlink);
                    foreach (var inlink in currentInlinks)
                        if (inlink.NodeName.Contains(orderUnit.Key))
                            newInlinks.Add(new GraphEdge(inlink.Predicate, inlink.TimeStamp, inlink.NodeName.Replace(orderUnit.Key, orderUnit.Value)));
                        else
                            newInlinks.Add(inlink);
                    result[i] = new GraphNode(currentName, newOutLinks, newInlinks);
                }
            }
            for (int i = Variable.VariableZero; i < result.Count; i++)
            {
                var currentNode = result[i];
                var currentName = currentNode.Name;
                var currentOutlinks = currentNode.OutLinks;
                var currentInlinks = currentNode.InLinks;
                List<GraphEdge> newInlinks = new List<GraphEdge>();
                List<GraphEdge> newOutLinks = new List<GraphEdge>();
                foreach (var variable in toReplace)
                {
                    if (currentName.Contains(variable.Key))
                        currentName = currentName.Replace(variable.Key, variable.Value);
                    foreach (var outlink in currentOutlinks)
                        if (outlink.NodeName.Contains(variable.Key))
                            newOutLinks.Add(new GraphEdge(outlink.Predicate, outlink.TimeStamp, outlink.NodeName.Replace(variable.Key, variable.Value)));
                        else
                            newOutLinks.Add(outlink);
                    foreach (var inlink in currentInlinks)
                        if (inlink.NodeName.Contains(variable.Key))
                            newInlinks.Add(new GraphEdge(inlink.Predicate, inlink.TimeStamp, inlink.NodeName.Replace(variable.Key, variable.Value)));
                        else
                            newInlinks.Add(inlink);
                }
                result[i] = new GraphNode(currentName, newOutLinks, newInlinks);
            }
            return result;
        }
        private bool CompareAnswer(List<string> expcetAnswer, List<KeyValuePair<string, VariableType>> realAnswer)
        {
            if (expcetAnswer.Count != realAnswer.Count)
                return false;

            if (realAnswer[Variable.VariableZero].Value == VariableType.Unknown)
                for (int i = Variable.VariableZero; i < expcetAnswer.Count; i++)
                    if (expcetAnswer[i] != realAnswer[i].Key)
                        return false;
            if (realAnswer[Variable.VariableZero].Value != VariableType.Unknown)
                foreach (var ele in realAnswer)
                    if (!expcetAnswer.Contains(ele.Key))
                        return false;
            return true;
        }

        private List<List<KeyValuePair<string, VariableType>>> GetTryingAnswer(int answerLength, HashSet<string> variableSet)
        {
            List<List<KeyValuePair<string, VariableType>>> result = new List<List<KeyValuePair<string, VariableType>>>();

            string[] par = variableSet.ToArray();
            var allResult = Tools.GetPermutationN(par, answerLength);
            foreach (var ele in allResult)
            {
                List<KeyValuePair<string, VariableType>> temp = new List<KeyValuePair<string, VariableType>>();
                foreach (var e in ele)
                    temp.Add(new KeyValuePair<string, VariableType>(e, VariableType.Subject));
                result.Add(temp);
            }
            return result;
        }
        private HashSet<string> GetAllVariableInFacts(List<ProcessingUnit> accumulativeFact, string question)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var tripleTerm in accumulativeFact)
                foreach (var variable in _variableHistorical)
                    if (tripleTerm.GetSubjectValue.Contains(variable) || tripleTerm.GetObjectValue.Contains(variable))
                        result.Add(variable);
            return result;
        }
        private bool SubGraphMatch(List<GraphNode> first, List<GraphNode> second, RuleNameMatching matchingUnit)
        {
            HashSet<string> keyVariable = new HashSet<string>();
            foreach (var ele in matchingUnit.Target.QAStructure)
                if (ele.Value != VariableType.Unknown)
                    keyVariable.Add(ele.Key);
            if (first.Count != second.Count)
                return false;
            foreach (var ele in first)
            {
                if (keyVariable.Contains(ele.Name))
                    try
                    {
                        var nodeInSeconde = GetGraphNode(ele.Name, second);
                        if (ele.OutLinks.Count == nodeInSeconde.OutLinks.Count
                            && ele.InLinks.Count == nodeInSeconde.InLinks.Count)
                        {
                            if (!(CompareEdgeList(ele.OutLinks, nodeInSeconde.OutLinks, keyVariable) &&
                                CompareEdgeList(ele.InLinks, nodeInSeconde.InLinks, keyVariable)))
                                return false;
                        }
                        else
                            return false;
                    }
                    catch { return false; }
                else continue;
            }

            return true;
        }
        private bool CompareEdges(List<GraphNode> firstGraph, List<GraphNode> secondGraph, HashSet<string> keyVariable)
        {
            List<GraphEdge> first = new List<GraphEdge>();
            List<GraphEdge> second = new List<GraphEdge>();
            foreach (var ele in firstGraph)
            {
                foreach (var e in ele.OutLinks)
                    first.Add(e);
                foreach (var e in ele.InLinks)
                    first.Add(e);
            }
            foreach (var ele in secondGraph)
            {
                foreach (var e in ele.OutLinks)
                    second.Add(e);
                foreach (var e in ele.InLinks)
                    first.Add(e);
            }
            var firSort = SortEdge(first);
            var secSort = SortEdge(second);
            return CompareSortedEdgeList(firSort, secSort, keyVariable);
        }
        private List<GraphEdge> SortEdge(List<GraphEdge> edgeList)
        {
            List<GraphEdge> result = new List<GraphEdge>();
            Dictionary<int, int> index2Order = new Dictionary<int, int>();
            for (int i = Variable.VariableZero; i < edgeList.Count; i++)
                index2Order[i] = edgeList[i].TimeStamp;
            var dicSort = from d in index2Order

                          orderby d.Value

                          ascending

                          select d;
            foreach (var ele in dicSort)
                result.Add(edgeList[ele.Key]);
            return result;
        }
        private bool CompareEdgeList(List<GraphEdge> first, List<GraphEdge> second, HashSet<string> keyVariable)
        {
            var sortedFir = SortEdge(first);
            var sortedSec = SortEdge(second);
            HashSet<int> firstcategory = new HashSet<int>();
            HashSet<int> seccategory = new HashSet<int>();
            foreach (var ele in first)
                firstcategory.Add(ele.TimeStamp);
            foreach (var ele in second)
                seccategory.Add(ele.TimeStamp);
            if (firstcategory.Count != seccategory.Count)
                return false;
            if (first.Count > firstcategory.Count)
            {
                List<int> timeStampInFirst = new List<int>();
                foreach (var ele in first)
                    timeStampInFirst.Add(ele.TimeStamp);
                List<int> timeStampInSecond = new List<int>();
                foreach (var ele in second)
                    timeStampInSecond.Add(ele.TimeStamp);
                timeStampInFirst.Sort();
                timeStampInSecond.Sort();
                int index = Variable.VariableZero;
                for (int i = Variable.VariableZero; i < timeStampInFirst.Count - Variable.VariableSingle; i++)
                    if (timeStampInFirst[i] == timeStampInFirst[i + Variable.VariableSingle])
                    { index = i; break; }
                if (timeStampInSecond[index] != timeStampInSecond[index + Variable.VariableSingle])
                    return false;
            }
            var flag = CompareSortedEdgeList(sortedFir, sortedSec, keyVariable);
            return flag;
        }
        private bool CompareSortedEdgeList(List<GraphEdge> first, List<GraphEdge> second, HashSet<string> keyVariable)
        {
            Variable variable = new Variable();
            var allVariable = variable.GetAllVariable;
            for (int i = Variable.VariableZero; i < first.Count; i++)
            {
                if (!((IsSameClass(first[i].Predicate, second[i].Predicate) &&
             (keyVariable.Contains(first[i].NodeName) && keyVariable.Contains(second[i].NodeName))
               && (first[i].NodeName == second[i].NodeName))
              || (IsSameClass(first[i].Predicate, second[i].Predicate) &&
              (!keyVariable.Contains(first[i].NodeName) && !keyVariable.Contains(second[i].NodeName))
                && (allVariable.Contains(first[i].NodeName) && allVariable.Contains(second[i].NodeName)))
                || (IsSameClass(first[i].Predicate, second[i].Predicate) &&
              (!keyVariable.Contains(first[i].NodeName) && !keyVariable.Contains(second[i].NodeName))
                && (!allVariable.Contains(first[i].NodeName) && !allVariable.Contains(second[i].NodeName))
                && (first[i].NodeName == second[i].NodeName))
                ))
                    return false;
            }
            return true;
        }
        private bool IsSameClass(string p1, string p2)//compare $X$ and $Y$
        {
            foreach (var ele in Equivalence.allEquivalence)
                if (ele.Value.Contains(symbol2Predicate[p1]) && ele.Value.Contains(symbol2Predicate[p2]))
                    return true;
            return false;
        }
        private GraphNode GetGraphNode(string name, List<GraphNode> nodeList)
        {
            foreach (var ele in nodeList)
                if (name == ele.Name)
                    return ele;
            throw new Exception();
        }
        private List<GraphNode> ShrinkGraphNodeCollection(List<GraphNode> nodeList, List<KeyValuePair<string, VariableType>> qaStructure)
        {
            List<GraphNode> result = new List<GraphNode>();

            HashSet<string> added = new HashSet<string>();
            HashSet<string> variableSet = new HashSet<string>();

            foreach (var ele in qaStructure)
                if (ele.Value != VariableType.Unknown)
                    variableSet.Add(ele.Key);

            List<string> keyNode = new List<string>(variableSet);
            Dictionary<string, HashSet<string>> variableRelatedList = new Dictionary<string, HashSet<string>>();

            var variableList = variableSet.ToList();
            if (variableList.Count == Variable.VariableSingle)
            {
                result.Add(GetGraphNode(nodeList, variableList[Variable.VariableZero]));
                return MergeSurplusEdge(result);
            }
            if (variableList.Count < Variable.NodeThreshold)
                for (int i = Variable.VariableZero; i < variableList.Count; i++)
                {
                    for (int j = i + Variable.VariableSingle; j < variableList.Count; j++)
                    {
                        var pathSet = IsConnection(variableList[i], variableList[j], nodeList);
                        if (pathSet.Count != Variable.VariableZero)
                        {
                            foreach (var path in pathSet)
                                foreach (var ele in path)
                                    if (!keyNode.Contains(ele))
                                        keyNode.Add(ele);
                        }
                        else
                        {
                            if (!keyNode.Contains(variableList[i]))
                                keyNode.Add(variableList[i]);
                            if (!keyNode.Contains(variableList[j]))
                                keyNode.Add(variableList[j]);
                        }
                    }
                }
            if (keyNode.Count > Variable.NodeThreshold)
            {
                var initkeyNodeCopy = new List<string>(keyNode);
                initkeyNodeCopy.RemoveRange(Variable.VariableDouble, keyNode.Count - Variable.VariableDouble);
                var tempCopy = new List<string>(keyNode);
                tempCopy.RemoveRange(Variable.VariableZero, Variable.VariableDouble);
                string toKeep = Variable.NullString;
                int timeStamp = Variable.VariableZero;
                foreach (var ele in tempCopy)
                {
                    var timeSize = CalcTimeStamp(nodeList, ele, initkeyNodeCopy);
                    if (timeSize > timeStamp)
                    {
                        toKeep = ele;
                        timeStamp = timeSize;
                    }
                }
                initkeyNodeCopy.Add(toKeep);
                keyNode = new List<string>(initkeyNodeCopy);
            }

            int oldCount = keyNode.Count;
            List<string> newKeyNode = new List<string>(keyNode);
            if (keyNode.Count < Variable.NodeThreshold)
                newKeyNode = UpdateKeyNode(keyNode.ToList(), nodeList);
            int newCount = newKeyNode.Count;
            foreach (var ele in newKeyNode)//get a subgraph only contain keynode
                try
                {
                    result.Add(GetGraphNode(nodeList, ele));
                }
                catch { }
            var subGraph = DelEdge(new HashSet<string>(newKeyNode), result);
            var mergedOutlink = MergeOutlinks(subGraph, new HashSet<string>(newKeyNode));
            var tt = MergeSurplusEdge(mergedOutlink);
            if (newKeyNode.Count < Variable.NodeThreshold)
                return tt;
            else
                return MSTGeneration(tt, newKeyNode, oldCount, newCount);
        }
        private int CalcTimeStamp(List<GraphNode> graph, string nodeName, List<string> keyNode)
        {
            foreach (var ele in graph)
                if (ele.Name == nodeName)
                {
                    int sum = Variable.VariableZero;
                    foreach (var link in ele.OutLinks)
                        if (keyNode.Contains(link.NodeName))
                            sum += link.TimeStamp;
                    foreach (var link in ele.InLinks)
                        if (keyNode.Contains(link.NodeName))
                            sum += link.TimeStamp;
                    return sum;
                }
            return Variable.VariableZero;
        }
        private List<GraphNode> MSTGeneration(List<GraphNode> graph, List<string> keyNode, int oldCount, int newCount)
        {
            List<GraphNode> result = new List<GraphNode>(graph);
            if (oldCount == newCount)
                return graph;
            var firstNode = GetGraphNode(keyNode[Variable.VariableZero], graph);
            var secondNode = GetGraphNode(keyNode[Variable.VariableSingle], graph);
            var firstOutLink = firstNode.OutLinks;
            var firstInlink = firstNode.InLinks;
            var seconOutLink = secondNode.OutLinks;
            var secondInlink = secondNode.InLinks;

            List<GraphEdge> firNewOutLink = new List<GraphEdge>();
            List<GraphEdge> firNewLinkLink = new List<GraphEdge>();
            List<GraphEdge> secNewOutLink = new List<GraphEdge>();
            List<GraphEdge> secNewLinkLink = new List<GraphEdge>();
            foreach (var ele in firstOutLink)
                if (ele.NodeName != secondNode.Name)
                    firNewOutLink.Add(ele);
            foreach (var ele in firstInlink)
                if (ele.NodeName != secondNode.Name)
                    firNewLinkLink.Add(ele);
            foreach (var ele in seconOutLink)
                if (ele.NodeName != firstNode.Name)
                    secNewOutLink.Add(ele);
            foreach (var ele in secondInlink)
                if (ele.NodeName != firstNode.Name)
                    secNewLinkLink.Add(ele);
            result[Variable.VariableZero] = new GraphNode(firstNode.Name, firNewOutLink, firNewLinkLink);
            result[Variable.VariableSingle] = new GraphNode(secondNode.Name, secNewOutLink, secNewLinkLink);
            return result;
        }
        private bool IsContainsKeyNodeLink(List<GraphEdge> link, string name)
        {
            foreach (var ele in link)
                if (ele.NodeName == name)
                    return true;
            return false;
        }
        private List<string> UpdateKeyNode(List<string> keynode, List<GraphNode> nodeList)
        {
            List<string> result = new List<string>(keynode);
            HashSet<string> temp = new HashSet<string>();
            try
            {
                foreach (var node in nodeList)
                {
                    var currentNode = GetGraphNode(node.Name, nodeList);
                    var outlink = currentNode.OutLinks.Select(x => x.NodeName);
                    var inlink = currentNode.InLinks.Select(x => x.NodeName);
                    List<string> allLink = new List<string>(outlink);
                    allLink.AddRange(inlink);
                    if (IsContainKeyNode(allLink, keynode))
                        temp.Add(node.Name);
                }
            }
            catch { }
            foreach (var e in temp)
                if (result.Count < Variable.NodeThreshold)
                    result.Add(e);
            return result;
        }
        private bool IsContainKeyNode(List<string> link, List<string> keynode)
        {
            foreach (var ele in keynode)
                if (!link.Contains(ele))
                    return false;
            return true;
        }
        private List<GraphNode> MergeOutlinks(List<GraphNode> currentGraph, HashSet<string> keyNode)
        {
            return currentGraph;
        }
        private HashSet<string> GetNeighbor(string node, List<GraphNode> nodeList)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var ele in nodeList)
                if (ele.Name == node)
                {
                    foreach (var e in ele.OutLinks)
                        result.Add(e.NodeName);
                    foreach (var e in ele.InLinks)
                        result.Add(e.NodeName);
                }
            return result;
        }
        private List<List<string>> IsConnection(string source, string target, List<GraphNode> nodeList)
        {
            List<List<string>> result = new List<List<string>>();
            List<string> init = new List<string> { source };
            Queue<List<string>> queue = new Queue<List<string>>();
            queue.Enqueue(init);
            while (queue.Count != Variable.VariableZero)
            {
                var currentPath = queue.Dequeue();
                var currentTail = currentPath.Last();
                var neighbors = GetNeighbor(currentTail, nodeList);
                if (neighbors.Contains(target))
                {
                    var temp = new List<string>(currentPath);
                    temp.Add(target);
                    result.Add(temp);
                }
                else
                    foreach (var ele in neighbors)
                    {
                        var temp = new List<string>(currentPath);
                        if (!temp.Contains(ele))
                        {
                            temp.Add(ele);
                            queue.Enqueue(temp);
                        }
                    }
            }
            int min = Variable.Threshold;
            int index = Variable.NegInit;
            for (int i = Variable.VariableZero; i < result.Count; i++)
                if (result[i].Count <= min)
                {
                    min = result[i].Count;
                    index = i;
                }
            if (index == Variable.NegInit)
                return new List<List<string>>();
            else
            {
                List<List<string>> temp = new List<List<string>>();
                foreach (var ele in result)
                    if (ele.Count == min)
                        temp.Add(ele);
                return temp;
            }
        }
        private bool IsContains(List<GraphEdge> edgeList, HashSet<string> keySet)
        {
            bool flag = false;
            foreach (var edge in edgeList)
                if (keySet.Contains(edge.NodeName))
                    return true;
            return flag;
        }
        private GraphNode GetGraphNode(List<GraphNode> nodeList, string name)
        {
            foreach (var node in nodeList)
                if (node.Name == name)
                    return node;
            throw new Exception(Variable.HasNoGraphNode);
        }
        private bool DetectIsMerge(List<GraphEdge> edgeList, List<int> index, List<GraphEdge> otherLink)
        {
            HashSet<string> className = new HashSet<string>();
            List<int> timeStamp = new List<int>();
            foreach (var ele in index)
                timeStamp.Add(edgeList[ele].TimeStamp);
            foreach (var e in index)
                className.Add(GetEquClassName(edgeList[e].Predicate));
            List<GraphEdge> newTimeStampList = new List<GraphEdge>();
            for (int i = Variable.VariableZero; i < edgeList.Count; i++)
                if (edgeList[i].TimeStamp > timeStamp.Min() && edgeList[i].TimeStamp < timeStamp.Max())
                    newTimeStampList.Add(edgeList[i]);
            for (int i = Variable.VariableZero; i < otherLink.Count; i++)
                if (otherLink[i].TimeStamp > timeStamp.Min() && otherLink[i].TimeStamp < timeStamp.Max())
                    newTimeStampList.Add(otherLink[i]);
            foreach (var ele in newTimeStampList)
                className.Add(GetEquClassName(ele.Predicate));
            if (className.Count > Variable.VariableSingle)
                return false;
            else
                return true;
        }
        private List<GraphNode> MergeSurplusEdge(List<GraphNode> subGraph)
        {
            List<GraphNode> result = new List<GraphNode>();
            foreach (var graphNode in subGraph)
            {
                List<GraphEdge> outlink = graphNode.OutLinks;
                List<GraphEdge> inlink = graphNode.InLinks;
                Dictionary<string, List<int>> outlinkDic = new Dictionary<string, List<int>>();
                Dictionary<string, List<int>> inlinkDic = new Dictionary<string, List<int>>();
                for (int i = Variable.VariableZero; i < outlink.Count; i++)
                {
                    string targetname = outlink[i].NodeName;
                    if (outlinkDic.ContainsKey(targetname))
                        outlinkDic[targetname].Add(i);
                    else
                        outlinkDic[targetname] = new List<int> { i };
                }
                for (int i = Variable.VariableZero; i < inlink.Count; i++)
                {
                    string sourcename = inlink[i].NodeName;
                    if (inlinkDic.ContainsKey(sourcename))
                        inlinkDic[sourcename].Add(i);
                    else
                        inlinkDic[sourcename] = new List<int> { i };
                }
                List<int> outlinkindexList = new List<int>();
                List<int> inlinkIndexlist = new List<int>();
                foreach (var ele in outlinkDic)
                {
                    if (ele.Value.Count > Variable.VariableSingle)
                    {
                        Dictionary<string, List<int>> className2index = new Dictionary<string, List<int>>();

                        foreach (var index in ele.Value)
                        {
                            var name = GetEquClassName(outlink[index].Predicate);
                            if (className2index.ContainsKey(name))
                                className2index[name].Add(index);
                            else
                                className2index[name] = new List<int> { index };
                        }
                        foreach (var e in className2index)
                            if (e.Value.Count == Variable.VariableZero)
                                outlinkindexList.Add(e.Value[Variable.VariableZero]);
                            else
                            {
                                var flag = DetectIsMerge(outlink, e.Value, inlink);
                                if (flag)
                                    outlinkindexList.Add(e.Value[e.Value.Count - Variable.VariableSingle]);
                                else
                                    outlinkindexList.AddRange(e.Value);
                            }
                    }
                    else
                        outlinkindexList.Add(ele.Value[Variable.VariableZero]);
                }
                foreach (var ele in inlinkDic)
                {
                    if (ele.Value.Count > Variable.VariableSingle)
                    {
                        Dictionary<string, List<int>> className2index = new Dictionary<string, List<int>>();
                        foreach (var index in ele.Value)
                        {
                            var name = GetEquClassName(inlink[index].Predicate);
                            if (className2index.ContainsKey(name))
                                className2index[name].Add(index);
                            else
                                className2index[name] = new List<int> { index };
                        }
                        foreach (var e in className2index)
                            if (e.Value.Count == Variable.VariableSingle)
                                inlinkIndexlist.Add(e.Value[Variable.VariableZero]);
                            else
                            {
                                var flag = DetectIsMerge(inlink, e.Value, outlink);
                                if (flag)
                                    inlinkIndexlist.Add(e.Value[e.Value.Count - Variable.VariableSingle]);
                                else
                                    inlinkIndexlist.AddRange(e.Value);
                            }
                    }
                    else
                        inlinkIndexlist.Add(ele.Value[Variable.VariableZero]);
                }
                List<GraphEdge> newOutLink = new List<GraphEdge>();
                List<GraphEdge> newInLink = new List<GraphEdge>();
                foreach (var ele in outlinkindexList)
                    newOutLink.Add(outlink[ele]);
                foreach (var ele in inlinkIndexlist)
                    newInLink.Add(inlink[ele]);
                result.Add(new GraphNode(graphNode.Name, newOutLink, newInLink));
            }
            return result;
        }
        private string GetEquClassName(string symbol)
        {
            var name = symbol2Predicate[symbol];
            foreach (var ele in Equivalence.allEquivalence)
                if (ele.Value.Contains(name))
                    return ele.Key;
            throw new Exception();
        }
        private List<GraphNode> DelEdge(HashSet<string> keySet, List<GraphNode> nodeList)
        {
            List<GraphNode> result = new List<GraphNode>();
            foreach (var ele in nodeList)
            {
                var inlink = ele.InLinks;
                var outlink = ele.OutLinks;
                List<GraphEdge> newInlinks = new List<GraphEdge>();
                List<GraphEdge> newOutlinks = new List<GraphEdge>();

                foreach (var e in inlink)
                    if (keySet.Contains(e.NodeName))
                        newInlinks.Add(e);
                foreach (var e in outlink)
                {
                    if (keySet.Count > Variable.VariableDouble)
                    {
                        if (keySet.Contains(e.NodeName))
                            newOutlinks.Add(e);
                    }
                    else
                        newOutlinks.Add(e);
                }
                var name = ele.Name;
                result.Add(new GraphNode(name, newOutlinks, newInlinks));
            }
            return result;
        }
        private GraphStructure BuildCurrentGraphStructure(FactUnit currentFactUnit)
        {
            var tripleList = currentFactUnit.GetFactUnit;
            int edgeCounter = Variable.VariableZero;

            HashSet<string> nodeNameSet = new HashSet<string>();
            List<GraphNode> graphNodeList = new List<GraphNode>();
            foreach (var triple in tripleList)
            {
                var currentSubject = triple.SubjectValue;
                var currentPredicate = triple.PredicateValue;
                var currentObject = triple.ObjectValue;
                int PredicateID = ++edgeCounter;

                #region single single
                if (!currentSubject.Contains(Variable.VariableAnd) && !currentObject.Contains(Variable.VariableOr))//single variable
                {
                    GraphEdge outLinkEdge = new GraphEdge(currentPredicate, PredicateID, currentObject);
                    GraphEdge inLinkEdge = new GraphEdge(currentPredicate, PredicateID, currentSubject);
                    graphNodeList = UpdateSingleSubject(ref nodeNameSet, currentSubject, graphNodeList, outLinkEdge);
                    graphNodeList = UpdateSingleObject(ref nodeNameSet, currentObject, graphNodeList, inLinkEdge);
                }
                #endregion
                else if (!currentSubject.Contains(Variable.VariableAnd) && currentObject.Contains(Variable.VariableOr))
                {
                    if (currentObject.Contains(Variable.VariableAnd))
                        throw new Exception();
                    if (!currentObject.Contains(Variable.VariableOr))
                        throw new Exception();
                    var tempObject = currentObject.Replace(Variable.VariableOr + Variable.SpaceString, Variable.NullString);
                    string[] objectArray = tempObject.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var ele in objectArray)
                    {
                        var outLinkEdge = new GraphEdge(currentPredicate, PredicateID, ele.Trim());
                        graphNodeList = UpdateSingleSubject(ref nodeNameSet, currentSubject, graphNodeList, outLinkEdge);
                    }
                    var inLinkEdge = new GraphEdge(currentPredicate, PredicateID, currentSubject);
                    foreach (var ele in objectArray)
                        graphNodeList = UpdateSingleObject(ref nodeNameSet, ele.Trim(), graphNodeList, inLinkEdge);
                }
                else if (currentSubject.Contains(Variable.VariableAnd) && !currentObject.Contains(Variable.VariableOr))
                {
                    if (currentSubject.Contains(Variable.VariableOr))
                        throw new Exception();
                    if (!currentSubject.Contains(Variable.VariableAnd))
                        throw new Exception();
                    var tempSubject = currentSubject.Replace(Variable.VariableAnd + Variable.SpaceString, Variable.NullString);
                    string[] subjectArray = tempSubject.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                    var outLinkEdge = new GraphEdge(currentPredicate, PredicateID, currentObject);
                    foreach (var ele in subjectArray)
                        graphNodeList = UpdateSingleSubject(ref nodeNameSet, ele.Trim(), graphNodeList, outLinkEdge);
                    foreach (var ele in subjectArray)
                    {
                        var inLinkEdge = new GraphEdge(currentPredicate, PredicateID, ele.Trim());
                        graphNodeList = UpdateSingleObject(ref nodeNameSet, currentObject, graphNodeList, inLinkEdge);
                    }
                }
                else if (currentSubject.Contains(Variable.VariableAnd) && currentObject.Contains(Variable.VariableOr))
                {
                    throw new Exception();
                }
            }
            return new GraphStructure(nodeNameSet, edgeCounter, graphNodeList);
        }
        private List<GraphNode> UpdateSingleSubject(ref HashSet<string> nodeNameSet, string currentSubject, List<GraphNode> graphNodeList, GraphEdge outLinkEdge)
        {
            if (nodeNameSet.Contains(currentSubject))//update node
            {
                var index = GetGraphNodeIndex(currentSubject, graphNodeList);
                var edgeList = graphNodeList[index].OutLinks;
                var outLinkEdgeList = new List<GraphEdge>(edgeList);
                outLinkEdgeList.Add(outLinkEdge);
                graphNodeList[index] = new GraphNode(currentSubject, outLinkEdgeList, graphNodeList[index].InLinks);
                return graphNodeList;
            }
            else//add node
            {
                graphNodeList.Add(new GraphNode(currentSubject, new List<GraphEdge> { outLinkEdge }, new List<GraphEdge>()));
                nodeNameSet.Add(currentSubject);
                return graphNodeList;
            }
        }
        private int GetGraphNodeIndex(string nodeName, List<GraphNode> nodeList)
        {
            for (int i = Variable.VariableZero; i < nodeList.Count; i++)
                if (nodeList[i].Name == nodeName)
                    return i;
            throw new Exception(Variable.NullString);
        }
        private List<GraphNode> UpdateSingleObject(ref HashSet<string> nodeNameSet, string currentObject, List<GraphNode> graphNodeList, GraphEdge inLinkEdge)
        {
            if (nodeNameSet.Contains(currentObject))//update
            {
                var index = GetGraphNodeIndex(currentObject, graphNodeList);
                var edgeList = graphNodeList[index].InLinks;
                var inLinkEdgeList = new List<GraphEdge>(edgeList);
                inLinkEdgeList.Add(inLinkEdge);
                graphNodeList[index] = new GraphNode(currentObject, graphNodeList[index].OutLinks, inLinkEdgeList);
                return graphNodeList;
            }
            else//add
            {
                graphNodeList.Add(new GraphNode(currentObject, new List<GraphEdge>(), new List<GraphEdge> { inLinkEdge }));
                nodeNameSet.Add(currentObject);
                return graphNodeList;
            }
        }
        private RuleStructure GenerateRule(List<ProcessingUnit> accumulation, QACombination currentQA, ref HashSet<string> variableHistorical, ref Dictionary<string, string> allVariableDic, Dictionary<string, string> variableTagger)
        {
            int questionLength = currentQA.Question.Count;
            List<KeyValuePair<string, VariableType>> qaStructure = new List<KeyValuePair<string, VariableType>>(currentQA.Question);
            qaStructure.AddRange(currentQA.Answer);
            Variable variable = new Variable();
            foreach (var ele in qaStructure)
                if (ele.Value != VariableType.Unknown)
                    variableHistorical.Add(ele.Key);
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
                targetTriple.Add(new TripleTerm(currentSubject, currentPredicate, currentObject));
            }
            Dictionary<string, string> allTrans = new Dictionary<string, string>(variableDic);
            allVariableDic = allTrans.Union(variableTagger).ToDictionary(x => x.Key, x => x.Value);
            foreach (var ele in qaStructure)
            {
                if (allVariableDic.ContainsKey(ele.Key))
                    targetQA.Add(new KeyValuePair<string, VariableType>(allVariableDic[ele.Key], ele.Value));
                else
                    targetQA.Add(ele);
            }
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
                        {
                            timeDic[timeKey] = new List<ProcessingUnit> { new ProcessingUnit(unit.GetTripleType,
                                unit.GetSubjectValue,unit.GetPredicateValue,currentObject,unit.GetQuestion,unit.GetAnswer,unit.GetAnswerRelated)};
                        }
                        else
                            timeDic[timeKey].Add(new ProcessingUnit(unit.GetTripleType,
                                unit.GetSubjectValue, unit.GetPredicateValue, currentObject, unit.GetQuestion, unit.GetAnswer, unit.GetAnswerRelated));
                    }
                }
                if (flag)
                    noTime.Add(unit);
            }
            foreach (var ele in Time.TimeStamp)
            {
                if (timeDic.ContainsKey(ele))
                    result.AddRange(timeDic[ele]);
            }
            if (noTime.Count != Variable.VariableZero)
                result.AddRange(noTime);
            return result;
        }
        private List<ProcessingUnit> GetCurrentQuestionAccumulation(List<ProcessingUnit> accumulativeFacts, QACombination currentQA, ref List<int> factIndexList)
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
            factIndexList = indexList;
            List<ProcessingUnit> result = new List<ProcessingUnit>();
            foreach (var unit in indexList)
                result.Add(accumulativeFact[unit]);
            return result;
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
        private Dictionary<string, string> GetQuestionMap2RuleName(RuleNameMatching target)
        {
            Dictionary<string, string> questionVariable2RuleName = new Dictionary<string, string>();
            List<string> variableInRuleName = new List<string>();
            for (int i = Variable.VariableZero; i < target.Target.QuestionLength; i++)
                if (target.Target.QAStructure[i].Value != VariableType.Unknown)
                    variableInRuleName.Add(target.Target.QAStructure[i].Key);
            var dicSort = from d in target.RepalceIndex

                          orderby d.Value

                          ascending

                          select d;
            if (variableInRuleName.Count != dicSort.Count())
                throw new Exception();
            var temp = dicSort.ToList();
            for (int i = Variable.VariableZero; i < variableInRuleName.Count; i++)
                questionVariable2RuleName[temp[i].Key] = variableInRuleName[i];
            return questionVariable2RuleName;
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
        private List<RuleNameMatching> GetRuleSet(string question)
        {
            List<RuleNameMatching> result = new List<RuleNameMatching>();
            var dicSet = CalLength(_variableHistorical);
            var dicSort = from d in dicSet

                          orderby d.Value

                          descending

                          select d;
            var tempDic = dicSort.ToDictionary(x => x.Key, x => x.Value);
            Dictionary<string, int> toReplaceIndex = new Dictionary<string, int>();
            List<string> variableList = new List<string>();
            foreach (var ele in tempDic)
                if (ele.Key != Variable.NullString)
                    if (question.Contains(ele.Key))
                    {
                        int index = question.IndexOf(ele.Key);
                        question = question.Replace(ele.Key, Variable.StarString);
                        variableList.Add(ele.Key);
                        toReplaceIndex[ele.Key] = index;
                    }
            List<string> questionList = new List<string>();
            string[] arrayy = question.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            questionList = arrayy.ToList();
            foreach (var ele in _ruleSet)
            {
                List<string> ruleName = new List<string>();
                var target = ele.Target;
                var qaStructure = target.QAStructure;
                for (int i = Variable.VariableZero; i < target.QuestionLength; i++)
                {
                    if (target.QAStructure[i].Key.Contains(Variable.SpaceString))
                    {
                        string[] array = target.QAStructure[i].Key.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var word in array)
                            ruleName.Add(word);
                    }
                    else
                        ruleName.Add(target.QAStructure[i].Key);
                }
                bool flag = true;
                if (questionList.Count != ruleName.Count)
                    continue;

                for (int i = Variable.VariableZero; i < questionList.Count; i++)
                    if (!questionList[i].Contains(Variable.StarString) && !ruleName[i].Contains(Variable.StarString))
                        if (questionList[i] != ruleName[i])
                            flag = false;
                if (flag)
                    result.Add(new RuleNameMatching(variableList, toReplaceIndex, ele.Target));
            }
            return result;
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

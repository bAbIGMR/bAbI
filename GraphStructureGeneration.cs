using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
namespace GMR
{
    public class GraphStructureGeneration
    {
        Dictionary<string, string> _predicateDic = new Dictionary<string, string>();//predicate2Symbol
        Dictionary<string, string> symbol2Predicate = new Dictionary<string, string>();
        HashSet<string> _variableSet = new HashSet<string>();
        List<RuleSetUnit> RuleSetUnitList = new List<RuleSetUnit>();
        List<IntermediateRuleStructure> intermediateRuleStructure = new List<IntermediateRuleStructure>();
        List<RuleStructureCombination> result = new List<RuleStructureCombination>();
        public GraphStructureGeneration(List<VariableHistorical> variableSet, List<PredicateSymbolDic> predicateDic, List<RuleUnitCell> RuleUnitSetCollection)
        {
            foreach (var ele in variableSet)
                _variableSet.Add(ele.variable);
            foreach (var ele in predicateDic)
            {
                _predicateDic[ele.predicate] = ele.symbol;
                symbol2Predicate[ele.symbol] = ele.predicate;
            }
            foreach (var currentRuleUnitSet in RuleUnitSetCollection)
            {
                var qaStructure = currentRuleUnitSet.qaStructure;
                var questionLength = currentRuleUnitSet.questionLength;
                var factExpression = currentRuleUnitSet.factExpression;
                List<KeyValuePair<string, VariableType>> qa = new List<KeyValuePair<string, VariableType>>();
                foreach (var ele in qaStructure)
                {
                    var variableType = (VariableType)Enum.Parse(typeof(VariableType), ele.variableType);
                    qa.Add(new KeyValuePair<string, VariableType>(ele.unitContent, variableType));
                }
                List<RuleStorage> ruleStorageList = new List<RuleStorage>();
                foreach (var ruleStorage in factExpression)
                {
                    int factCount = ruleStorage.currentRuleSetFactCount;
                    List<FactUnit> factUnitList = new List<FactUnit>();
                    foreach (var factUnit in ruleStorage.factExpression)
                    {
                        List<TripleTerm> tripleList = new List<TripleTerm>();
                        foreach (var triple in factUnit.tripleList)
                            tripleList.Add(new TripleTerm(triple.subjectValue, triple.predicateValue, triple.objectValue));
                        factUnitList.Add(new FactUnit(tripleList));
                    }
                    ruleStorageList.Add(new RuleStorage(factCount, factUnitList));
                }
                RuleSetUnitList.Add(new RuleSetUnit(qa, ruleStorageList, questionLength));
            }
        }
        public void PipeLine()
        {
            intermediateRuleStructure = IntermediateRuleStructureGeneration();
            result = TargetRuleStructureGeneration();

            List<TestSet> testDataList = new List<TestSet>();
            for (int i = Variable.VariableZero; i < Variable.TaskCount; i++)
                testDataList.Add(new TestSet());
            foreach (var task in Global.LocalStorage.TestSet_Accessor_Selector())
                testDataList[task.taskIndex - Variable.VariableSingle] = task;
            foreach (var ele in testDataList)
            {
                var task = ele.taskIndex;
                Test tester = new Test(_predicateDic, _variableSet, result, ele.SessionSet);
                var accuracy = tester.PipeLine();
                Console.WriteLine("currentTaskID:{0},accuracy:{1}", task, accuracy);
            }
        }
        private int GetGraphNodeIndex(string nodeName, List<GraphNode> nodeList)
        {
            for (int i = Variable.VariableZero; i < nodeList.Count; i++)
                if (nodeList[i].Name == nodeName)
                    return i;
            throw new Exception(Variable.NullString);
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
                    var tempObject = currentObject.Replace(Variable.VariableOr+Variable.SpaceString, Variable.NullString);
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
                    var tempSubject = currentSubject.Replace(Variable.VariableAnd+Variable.SpaceString, Variable.NullString);
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
                    throw new Exception();
            }
            return new GraphStructure(nodeNameSet, edgeCounter, graphNodeList);
        }
        private List<IntermediateRuleStructure> IntermediateRuleStructureGeneration()
        {
            List<IntermediateRuleStructure> result = new List<IntermediateRuleStructure>();
            foreach (var currentRuleSetUnit in RuleSetUnitList)
            {
                var qaStructure = currentRuleSetUnit.RuleName;
                var questionLength = currentRuleSetUnit.QuestionLength;
                var expressionSet = currentRuleSetUnit.FactExpressionSet;
                List<GraphStructure> graphStructureList = new List<GraphStructure>();
                foreach (var ruleStorage in expressionSet)
                {
                    var factUnitList = ruleStorage.FactsCount2Facts;
                    var factCount = ruleStorage.FactCount;
                    foreach (var factUnit in factUnitList)
                    {
                        var currentgraphStructure = BuildCurrentGraphStructure(factUnit);
                        graphStructureList.Add(currentgraphStructure);
                    }
                }
                result.Add(new IntermediateRuleStructure(qaStructure, questionLength, graphStructureList));
            }
            return result;
        }
        private List<RuleStructureCombination> TargetRuleStructureGeneration()
        {
            List<RuleStructureCombination> result = new List<RuleStructureCombination>();
            foreach (var ele in intermediateRuleStructure)
            {
                var qa = ele.QAStructure;
                HashSet<string> checker = new HashSet<string>();
                var questionLength = ele.QuestionLength;
                var graphNodeSet = ele.GraphStructureList;
                List<SubGraphStructure> subGraphList = new List<SubGraphStructure>();
                int temp = Variable.VariableZero;
                foreach (var graphStructure in graphNodeSet)
                {
                    temp++;
                    var nodeList = graphStructure.CurrentGraphStructure;
                    var subgraph = ShrinkGraphNodeCollection(nodeList, qa, ele);
                    subGraphList.Add(new SubGraphStructure(graphStructure.NodeNameSet, graphStructure.EdgeCount, subgraph));
                }
                TargetRuleStructure target = new TargetRuleStructure(qa, questionLength, subGraphList);
                result.Add(new RuleStructureCombination(ele, target));
            }
            return result;
        }
        private GraphNode GetGraphNode(List<GraphNode> nodeList, string name)
        {
            foreach (var node in nodeList)
                if (node.Name == name)
                    return node;
            throw new Exception(Variable.HasNoGraphNode);
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
                {
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
            }
            int min =Variable.Threshold;
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
        private List<GraphNode> ShrinkGraphNodeCollection(List<GraphNode> nodeList, List<KeyValuePair<string, VariableType>> qaStructure, IntermediateRuleStructure graph)
        {
            List<GraphNode> result = new List<GraphNode>();

            HashSet<string> added = new HashSet<string>();
            HashSet<string> variableSet = new HashSet<string>();

            foreach (var ele in qaStructure)
                if (ele.Value != VariableType.Unknown)
                    variableSet.Add(ele.Key);

            List<string> keyNode = new List<string>(variableSet);
            Dictionary<string, HashSet<string>> variableRelatedList = new Dictionary<string, HashSet<string>>();
            int initCount = keyNode.Count;
            var variableList = variableSet.ToList();
            if (variableList.Count == Variable.VariableSingle)
            {
                result.Add(GetGraphNode(nodeList, variableList[Variable.VariableZero]));
                return MergeSurplusEdge(result, graph);
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
                tempCopy.RemoveRange(Variable.VariableDouble, Variable.VariableDouble);
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
            var tt = MergeSurplusEdge(mergedOutlink, graph);
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
        private bool IsContainsKeyNodeLink(List<GraphEdge> link, string name)
        {
            foreach (var ele in link)
                if (ele.NodeName == name)
                    return true;
            return false;
        }
        private List<GraphNode> MSTGeneration(List<GraphNode> graph, List<string> keyNode, int oldCount, int newCount)
        {
            List<GraphNode> result = new List<GraphNode>(graph);
            if (oldCount == newCount)
                return graph;
            var firstNode = GetGraphNode(graph, keyNode[Variable.VariableZero]);
            var secondNode = GetGraphNode(graph, keyNode[Variable.VariableSingle]);
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
        private List<GraphNode> MergeSurplusEdge(List<GraphNode> subGraph, IntermediateRuleStructure graph)
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
                            if (e.Value.Count == Variable.VariableSingle)
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
        private string GetEquClassName(string symbol)
        {
            var name = symbol2Predicate[symbol];
            foreach (var ele in Equivalence.allEquivalence)
                if (ele.Value.Contains(name))
                    return ele.Key;
            throw new Exception();
        }
        private List<string> UpdateKeyNode(List<string> keynode, List<GraphNode> nodeList)
        {
            List<string> result = new List<string>(keynode);
            HashSet<string> temp = new HashSet<string>();
            try
            {
                foreach (var node in nodeList)
                {
                    var currentNode = GetGraphNode(nodeList, node.Name);
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
                    if (keySet.Count > Variable.VariableDouble)
                    {
                        if (keySet.Contains(e.NodeName))
                            newOutlinks.Add(e);
                    }
                    else
                        newOutlinks.Add(e);
                var name = ele.Name;
                result.Add(new GraphNode(name, newOutlinks, newInlinks));
            }
            return result;
        }
        private List<GraphNode> GetRestOfRelatedNode(HashSet<string> keyNodeSet, List<GraphNode> nodeList)
        {
            List<GraphNode> result = new List<GraphNode>();
            foreach (var node in nodeList)
                if (!keyNodeSet.Contains(node.Name))
                {
                    var inlink = node.InLinks;
                    var outlink = node.OutLinks;
                    if (IsContains(inlink, keyNodeSet) || IsContains(outlink, keyNodeSet))
                        result.Add(node);
                }
            return result;
        }
        private bool IsContains(List<GraphEdge> edgeList, HashSet<string> keySet)
        {
            bool flag = false;
            foreach (var edge in edgeList)
                if (keySet.Contains(edge.NodeName))
                    return true;
            return flag;
        }
        private HashSet<string> GetRelatedNode(string variable, List<GraphNode> nodeList)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var ele in nodeList)
            {
                if (variable != ele.Name)
                    continue;
                var inlinks = ele.InLinks;
                var outlinkd = ele.OutLinks;
                foreach (var node in inlinks)
                    result.Add(node.NodeName);
                foreach (var node in outlinkd)
                    result.Add(node.NodeName);
            }
            return result;
        }
    }
}

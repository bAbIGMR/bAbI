using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class GraphEdge
    {
        string _predicate;
        int _timeStamp;
        string _nodeName;
        public GraphEdge(string predicate, int timeStamp, string nodeName)
        {
            _predicate = predicate;
            _timeStamp = timeStamp;
            _nodeName = nodeName;
        }
        public string Predicate { get { return _predicate; } set { _predicate = value; } }
        public int TimeStamp { get { return _timeStamp; } set { _timeStamp = value; } }
        public string NodeName { get { return _nodeName; } set { _nodeName = value; } }
    }
    public class GraphNode
    {
        string _name;
        List<GraphEdge> _outLinks = new List<GraphEdge>();
        List<GraphEdge> _inLinks = new List<GraphEdge>();
        public GraphNode(string name, List<GraphEdge> outLinks, List<GraphEdge> inLinks)
        {
            _name = name;
            _outLinks = outLinks;
            _inLinks = inLinks;
        }
        public string Name { get { return _name; } }
        public List<GraphEdge> OutLinks { get { return _outLinks; } set { _outLinks = value; } }
        public List<GraphEdge> InLinks { get { return _inLinks; } set { _inLinks = value; } }
    }
    public class RuleStructureCombination
    {
        IntermediateRuleStructure _source;
        TargetRuleStructure _target;
        public RuleStructureCombination(IntermediateRuleStructure source, TargetRuleStructure target)
        {
            _source = source;
            _target = target;
        }
        public IntermediateRuleStructure Source { get { return _source; } }
        public TargetRuleStructure Target { get { return _target; } }
    }
    public class IntermediateRuleStructure
    {
        List<KeyValuePair<string, VariableType>> _qaStructure;
        int _questionLength;
        List<GraphStructure> _graphStructureList = new List<GraphStructure>();
        public IntermediateRuleStructure(List<KeyValuePair<string, VariableType>> qaStructure, int questionLength, List<GraphStructure> graphStructureList)
        {
            _qaStructure = qaStructure;
            _questionLength = questionLength;
            _graphStructureList = graphStructureList;
        }
        public List<KeyValuePair<string, VariableType>> QAStructure { get { return _qaStructure; } set { _qaStructure = value; } }
        public int QuestionLength { get { return _questionLength; } set { _questionLength = value; } }
        public List<GraphStructure> GraphStructureList { get { return _graphStructureList; } set { _graphStructureList = value; } }
    }
    public class TargetRuleStructure
    {
        List<KeyValuePair<string, VariableType>> _qaStructure;
        int _questionLength;
        List<SubGraphStructure> _graphStructureList = new List<SubGraphStructure>();
        public TargetRuleStructure(List<KeyValuePair<string, VariableType>> qaStructure, int questionLength, List<SubGraphStructure> graphStructureList)
        {
            _qaStructure = qaStructure;
            _questionLength = questionLength;
            _graphStructureList = graphStructureList;
        }
        public List<KeyValuePair<string, VariableType>> QAStructure { get { return _qaStructure; } set { _qaStructure = value; } }
        public int QuestionLength { get { return _questionLength; } set { _questionLength = value; } }
        public List<SubGraphStructure> GraphStructureList { get { return _graphStructureList; } set { _graphStructureList = value; } }
    }
    public class GraphStructure
    {
        HashSet<string> _nodeNameSet = new HashSet<string>();
        int _edgeCount;
        List<GraphNode> _graphStrucure = new List<GraphNode>();
        public GraphStructure(HashSet<string> nodeNameSet, int edgeCount, List<GraphNode> graphNodeSet)
        {
            _nodeNameSet = nodeNameSet;
            _edgeCount = edgeCount;
            _graphStrucure = graphNodeSet;
        }
        public HashSet<string> NodeNameSet { get { return _nodeNameSet; } }
        public int EdgeCount { get { return _edgeCount; } set { _edgeCount = value; } }
        public List<GraphNode> CurrentGraphStructure { get { return _graphStrucure; } set { _graphStrucure = value; } }
    }
    public class SubGraphStructure
    {
        HashSet<string> _nodeNameSet = new HashSet<string>();
        int _edgeCount;
        List<GraphNode> _graphStrucure = new List<GraphNode>();
        public SubGraphStructure(HashSet<string> nodeNameSet, int edgeCount, List<GraphNode> subGraphNodeSet)
        {
            _nodeNameSet = nodeNameSet;
            _edgeCount = edgeCount;
            _graphStrucure = subGraphNodeSet;
        }
        public HashSet<string> NodeNameSet { get { return _nodeNameSet; } }
        public int EdgeCount { get { return _edgeCount; } set { _edgeCount = value; } }
        public List<GraphNode> CurrentGraphStructure { get { return _graphStrucure; } set { _graphStrucure = value; } }
    }
}

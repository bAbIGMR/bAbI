using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace GMR
{
    public class TriplePresentation
    {
        string _subject;
        string _predicate;
        string _object;
        public TriplePresentation(string Subject, string Predicate, string Object)
        {
            _subject = Subject;
            _predicate = Predicate;
            _object = Object;
        }
        public string Subject { get { return _subject; } }
        public string Predicate { get { return _predicate; } }
        public string Object { get { return _object; } }

    }
    public class RuleFileStructure
    {
        string fileName = Variable.NullString;
        Dictionary<string, string> predicateSymbolDic = new Dictionary<string, string>();
        List<List<TriplePresentation>> ruleSet = new List<List<TriplePresentation>>();
        public RuleFileStructure(string FileName, Dictionary<string, string> PredicateSymbolDic, List<List<TriplePresentation>> RuleSet)
        {
            fileName = FileName;
            predicateSymbolDic = PredicateSymbolDic;
            ruleSet = RuleSet;
        }
        public string FileName { get { return fileName; } }
        public Dictionary<string, string> PredicateSymbolDic { get { return predicateSymbolDic; } }
        public List<List<TriplePresentation>> RuleSet { get { return ruleSet; } }
    }
    public class LoadRuleSet
    {

        string ruleSetFileDirectory = Variable.RuleSetFileDirectory;
        public List<RuleFileStructure> GetRuleRepresentation()
        {
            return ReadRuleSetFile();
        }
        public string RuleSetFileDirectory { get { return ruleSetFileDirectory; } }
        public List<RuleFileStructure> ReadRuleSetFile()
        {
            var fileList = GetAllRuleFile();
            return HandleEachFile(fileList);
        }
        private List<RuleFileStructure> HandleEachFile(List<string> fileList)
        {
            List<RuleFileStructure> all = new List<RuleFileStructure>();
            foreach (var file in fileList)
            {
                List<string> predicateSet = new List<string>();
                List<List<string>> ruleFile = new List<List<string>>();
                using (StreamReader sr = new StreamReader(new FileStream(file, FileMode.Open)))
                {
                    List<string> ruleSet = new List<string>();
                    string readLine = sr.ReadLine();
                    while (readLine != null)
                    {
                        if (SpliteSymbol(readLine) == Variable.VariableSingle)
                            predicateSet.Add(readLine);
                        else if (SpliteSymbol(readLine) == Variable.VariableDouble)
                            ruleSet.Add(readLine);
                        else
                        {
                            if (ruleSet.Count > Variable.VariableZero)
                                ruleFile.Add(new List<string>(ruleSet));
                            ruleSet.Clear();
                        }
                        readLine = sr.ReadLine();
                    }
                }
                all.Add(Format(file,predicateSet, ruleFile));
            }
            return all;
        }
        private RuleFileStructure Format(string file,List<string> predicateDic, List<List<string>> ruleFile)
        {
            Dictionary<string, string> preDic = new Dictionary<string, string>();
            foreach (var kv in predicateDic)
            {
                string[] subString = kv.Split(new char[] { Variable.TabChar }, StringSplitOptions.RemoveEmptyEntries);
                preDic[subString[Variable.VariableZero]] = subString[Variable.VariableSingle];
            }
            List<List<TriplePresentation>> ruleFileSet = new List<List<TriplePresentation>>();
            foreach (var ruleSet in ruleFile)
            {
                List<TriplePresentation> tripleList = new List<TriplePresentation>();
                foreach (var ruleLine in ruleSet)
                {
                    string[] subString = ruleLine.Split(new char[] { Variable.TabChar }, StringSplitOptions.RemoveEmptyEntries);
                    tripleList.Add(new TriplePresentation(subString[Variable.VariableZero], subString[Variable.VariableSingle], subString[Variable.VariableDouble]));
                }
                ruleFileSet.Add(tripleList);
            }
            return new RuleFileStructure(file,preDic, ruleFileSet);
        }
        private int SpliteSymbol(string readLine)
        {
            int counter = Variable.VariableSingle;
            foreach (var symbol in readLine)
                if (symbol == Variable.TabChar)
                    counter++;
            return counter;
        }
        private List<string> GetAllRuleFile()
        {
            var files = Directory.GetFiles(ruleSetFileDirectory);
            return files.ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Trinity;
using Trinity.Storage;
using Trinity.Configuration;
using Trinity.Utilities;
using Trinity.Diagnostics;
using java.util;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using edu.stanford.nlp.ie.util;
using System.Diagnostics;
namespace GMR
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameter = ArgumentParser(args);
            if (parameter == Variable.DataPreProcessing)
                Extractor.Extract();
            if (parameter == Variable.RuleGeneration)
                TripleRuleGeneration.GenerateTripleRule();
            if (parameter == Variable.Test)
                BuildGraphStructure();
        }
        static string ArgumentParser(string[] args)
        {
            return args[0].Trim();
        }
        static void BuildGraphStructure()
        {
            Global.LocalStorage.LoadStorage();
            List<PredicateSymbolDic> predicList = new List<PredicateSymbolDic>();
            List<VariableHistorical> variableSet = new List<VariableHistorical>();
            foreach (var ele in Global.LocalStorage.PredicateSymbolDic_Accessor_Selector())
                predicList.Add(ele);
            foreach (var ele in Global.LocalStorage.VariableHistorical_Accessor_Selector())
                variableSet.Add(ele);
            List<RuleUnitCell> toHandleList = new List<RuleUnitCell>();
            foreach (var ele in Global.LocalStorage.RuleUnitCell_Accessor_Selector())
                toHandleList.Add(ele);
            GraphStructureGeneration obj = new GraphStructureGeneration(variableSet, predicList, toHandleList);
            obj.PipeLine();

        }
    }
}

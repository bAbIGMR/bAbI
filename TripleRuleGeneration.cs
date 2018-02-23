using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity;
namespace GMR
{
    public class TripleRuleGeneration
    {
        public static HashSet<string> checker = new HashSet<string>();
        static TripleRuleGeneration()
        {
            checker.Add(WordSet.TT);
            checker.Add(WordSet.FTT);

            checker.Add(WordSet.ATT);
            checker.Add(WordSet.AT);
            checker.Add(WordSet.TH);
            checker.Add(WordSet.TS);

            checker.Add(WordSet.FTT);
            checker.Add(WordSet.FTH);
            checker.Add(WordSet.FTS);

            checker.Add(WordSet.ATT);
            checker.Add(WordSet.ATH);
            checker.Add(WordSet.ATS);

            checker.Add(WordSet.AWT);
            checker.Add(WordSet.AWS);
            checker.Add(WordSet.AWH);
        }
        public static void GenerateTripleRule()
        {
            Global.LocalStorage.LoadStorage();
            Dictionary<string, string> predicateSymbolDic = new Dictionary<string, string>();
            HashSet<string> variableHistorical = new HashSet<string>();

            variableHistorical.Add(WordSet.Swan);
            variableHistorical.Add(WordSet.Lion);

            variableHistorical.Add(WordSet.Rhino);
            variableHistorical.Add(WordSet.Frog);



            List<RuleSetUnit> RuleCollection = new List<RuleSetUnit>();
            List<TrainingSet> trainDataList = new List<TrainingSet>();
            for (int i = Variable.VariableZero; i < Variable.TaskCount; i++)
                trainDataList.Add(new TrainingSet());
            foreach (var task in Global.LocalStorage.TrainingSet_Accessor_Selector())
            {
                trainDataList[task.taskIndex - Variable.VariableSingle] = task;
            }
            int taskId = Variable.VariableZero;
            foreach (var trainingSet in trainDataList)
            {
                ++taskId;
                if (taskId != trainingSet.taskIndex)
                    throw new Exception(Variable.WrongTaskID);
                HandleFirstTask obj = new HandleFirstTask(taskId, trainingSet.SessionSet);
                var currentRuleSet = obj.PipeLine(ref predicateSymbolDic, ref variableHistorical, taskId);
                Tools.PrintResult(predicateSymbolDic, currentRuleSet, taskId.ToString());
                RuleCollection = UpdateRuleSet.Update(variableHistorical, RuleCollection);
                MergeRuleSet mergeObj = new MergeRuleSet();
                var mergedCollection = mergeObj.StoreRuleSet(currentRuleSet, RuleCollection);
                RuleCollection = new List<RuleSetUnit>(mergedCollection);
            }
            StoreRuleSet.StoreRuleCollection(predicateSymbolDic, variableHistorical, RuleCollection);
            Tools.PrintAll(predicateSymbolDic, RuleCollection);
        }
    }
}

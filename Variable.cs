using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class Variable
    {
        public static readonly string DataPreProcessing="-e";
        public static readonly string RuleGeneration = "-g";
        public static readonly string Test = "-t";
        public static readonly int TaskCount = 20;
        public static readonly int Threshold = 1000;
        public static readonly int NegInit = -1;
        public static readonly int NodeThreshold = 3;
        public static readonly int VariableZero = 0;
        public static readonly int VariableSingle = 1;
        public static readonly int VariableDouble = 2;
        public static readonly string VariableAnd = "and";
        public static readonly string VariableOr = "or";
        public static readonly string SpecialTag = "$$$$";
        public static readonly string NullString = "";
        public static readonly string SpaceString = " ";
        public static readonly string QString = "?";
        public static readonly string CommaString = ",";
        public static readonly string EastAcro = "e";
        public static readonly string WestAcro = "w";
        public static readonly string SouthAcro = "s";
        public static readonly string NorthAcro = "n";
        public static readonly string RuleSetFileDirectory = ".\\RuleSet\\";
        public static readonly string TextSuffix = ".txt";
        public static readonly string StarString = "*";
        public static readonly string ReDir = ".\\";
        public static readonly string DataDir = ".\\babi\\en\\";
        public static readonly string FileNamePrefix = "qa";
        public static readonly string TrainingFileNameSuffix = "_train.txt";
        public static readonly string TestFileNameSuffix = "_test.txt";
        public static readonly string WrongTaskID = "wrong task id";
        public static readonly string HasNoGraphNode = "has no Graph node";
        public static readonly string DoLine = "$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$";
        public static readonly string ConLine = "-------------------------------------------";
        public static readonly string StarLine = "******************************************";
        public static readonly string NumLine = "###########################################";
        public static readonly string InvalidType = "invalid type";
        public static readonly char QChar = '?';
        public static readonly char SpaceChar = ' ';
        public static readonly char TabChar = '\t';
        public static readonly char CommaChar = ',';
        public static readonly char DotChar = '.';

        List<string> _variable = new List<string> { "*A*", "*B*",
            "*C*", "*D*", "*E*", "*F*", "*G*", "*H*", "*I*", "*J*", "*K*",
            "*L*", "*M*", "*N*", "*O*", "*P*", "*Q*", "*R*", "*S*", "*T*",
            "*U*", "*V*", "*W*", "*X*", "*Y*", "*Z*", };
        int index = 0;
        public List<string> GetAllVariable { get { return _variable; } }
        public string GetAvailiableVariableSymbol { get { return _variable[index++]; } }
        public Variable(int currentCount)
        {
            index = currentCount;
        }
        public int GetVariableIndex(string variable)
        {
            return _variable.IndexOf(variable);
        }
        public Variable() { }
    }
    public class WordSet
    {
        public static readonly string Swan = "swan";
        public static readonly string Lion = "lion";
        public static readonly string Rhino = "rhino";
        public static readonly string Frog = "frog";
        public static readonly string TT = "then they";
        public static readonly string FTT = "following that they";
        public static readonly string ATT = "after that they";
        public static readonly string AT = "afterwards they";
        public static readonly string TH = "then he";
        public static readonly string TS = "then she";
        public static readonly string FTH = "following that he";
        public static readonly string FTS = "following that she";
        public static readonly string ATH = "after that he";
        public static readonly string ATS = "after that she";
        public static readonly string AWT = "afterwards they";
        public static readonly string AWS = "afterwards she";
        public static readonly string AWH = "afterwards he";
        public static readonly string TBox = "tbox";
        public static readonly string BOC = "box of chocolates";
        public static readonly string ISA = "is a";
        public static readonly string IS = "is";
        public static readonly string THEN = "Then";
        public static readonly string FT = "Following that";
        public static readonly string AW = "afterwards";
        public static readonly string AFT = "After that";
        public static readonly string EI = "either";
        public static readonly string ISIN = "is in";
        public static readonly string ISNIN = "is not in";
        public static readonly string ISNL = "is no longer";
        public static readonly string EAST = "east";
        public static readonly string WEST = "west";
        public static readonly string SOUTH = "south";
        public static readonly string NORTH = "north";
        public static readonly string GOTO = "go to";
        public static readonly string ITLO = "is to left of";
        public static readonly string ITRO = "is to right of";
        public static readonly string GAVE = "gave";
        public static readonly string HANDED = "handed";
        public static readonly string PASSED = "passed";
        public static readonly string TO = "to";
        public static readonly string THE = "The";
        public static readonly string The = "the";
        public static readonly string THERE = "there";
        public static readonly string WOLCOM = "Wolves";
        public static readonly string WolCOM = "wolves";
        public static readonly string CATCOM = "Cats";
        public static readonly string CatCOM = "cats";
        public static readonly string MOUSECOM = "Mice";
        public static readonly string MouseCOM = "mice";
        public static readonly string WOL = "Wolf";
        public static readonly string Wol = "wolf";
        public static readonly string CAT = "Cat";
        public static readonly string Cat = "cat";
        public static readonly string MOUSE = "Mouse";
        public static readonly string Mouse = "mouse";
        public static readonly string WIS = "Where is";
        public static readonly string WAS = "Where was";
        public static readonly string BEFORE = "before";
        public static readonly string WIN = "was in";
        public static readonly string WHAT = "What";
        public static readonly string WHO = "Who";
        public static readonly string HM = "How many";
        public static readonly string WC = "What color";
        public static readonly string WILL = "Will";
        public static readonly string YES = "yes";
        public static readonly string NO = "no";
        public static readonly string IN = "in";
        public static readonly string ISTO = "is to";
        public static readonly string BELOW = "below";
        public static readonly string ISBELOW = "is below";
        public static readonly string ISNBELOW = "is not below";
        public static readonly string ISNABOVE = "is not above";
        public static readonly string ISNTO = "is not to";
        public static readonly string ISABOVE = "is above";
        public static readonly string MII = "maybe is in";
        public static readonly string ABOVE = "above";
        public static readonly string GO = "go";
        public static readonly string WHEREDOES = "Where does";
        public static readonly string HDYG = "How do you go";
        public static readonly string WD = "Where did";
        public static readonly string WHYDID = "Why did";

    }
}

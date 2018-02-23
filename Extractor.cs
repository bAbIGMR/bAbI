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
namespace GMR
{
    public class Extractor
    {
        readonly static string source_directory = Variable.DataDir;
        readonly static string fileNamePrefix =Variable.FileNamePrefix;
        readonly static string trainingFileNameSuffix = Variable.TrainingFileNameSuffix;
        readonly static string testFileNameSuffix = Variable.TestFileNameSuffix;
        readonly static HashSet<string> startTimeStamp = new HashSet<string> { "Yesterday", "This morning", "This evening", "This afternoon" };
        readonly static HashSet<string> endTimeStamp = new HashSet<string> { "yesterday", "this morning", "this evening", "this afternoon" };
        static Dictionary<string, string> complex2Single = new Dictionary<string, string>();
        static Dictionary<string, string> reductionTrans = new Dictionary<string, string>();
        static Extractor()
        {
            complex2Single[WordSet.WOLCOM] = WordSet.WOL;
            complex2Single[WordSet.WolCOM] = WordSet.Wol;
            complex2Single[WordSet.CATCOM] = WordSet.CAT;
            complex2Single[WordSet.CatCOM] = WordSet.Cat;
            complex2Single[WordSet.MOUSECOM] = WordSet.MOUSE;
            complex2Single[WordSet.MouseCOM] = WordSet.Mouse;
            reductionTrans[Variable.EastAcro] = WordSet.EAST;
            reductionTrans[Variable.WestAcro] = WordSet.WEST;
            reductionTrans[Variable.NorthAcro] = WordSet.NORTH;
            reductionTrans[Variable.SouthAcro] = WordSet.SOUTH;
        }
        #region ExtractFromSourceFile
        public static void Extract()
        {
            for (int i = Variable.VariableSingle; i < Variable.TaskCount + Variable.VariableSingle; i++)
            {
                PipeLine(i, FileType.Training);
                Console.WriteLine(i);
            }
            for (int i = Variable.VariableSingle; i < Variable.TaskCount + Variable.VariableSingle; i++)
            {
                PipeLine(i, FileType.Test);
                Console.WriteLine(i);
            }
            Global.LocalStorage.SaveStorage();
        }
        static List<string> ReadFile(string fileName)
        {
            List<string> eachLineString = new List<string>();
            using (StreamReader sr = new StreamReader(new FileStream(source_directory + fileName, FileMode.Open)))
            {
                string readLine = sr.ReadLine();
                while (readLine != null)
                {
                    eachLineString.Add(readLine.Trim());
                    readLine = sr.ReadLine();
                }
            }
            return eachLineString;
        }
        static List<List<string>> SplitBySession(List<string> multipleSession)
        {
            List<List<string>> sessionSet = new List<List<string>>();
            List<string> currentSession = new List<string>();
            for (int i = Variable.VariableZero; i < multipleSession.Count; i++)
            {
                string currentLine = multipleSession[i];
                string[] subCurrentLine = currentLine.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
                int index = int.Parse(subCurrentLine[Variable.VariableZero]);
                if (index == Variable.VariableSingle)
                {
                    if (currentSession.Count != Variable.VariableZero)
                    {
                        sessionSet.Add(new List<string>(currentSession));
                        currentSession.Clear();
                    }
                }
                currentSession.Add(currentLine);
            }
            sessionSet.Add(currentSession);
            return sessionSet;
        }
        static List<Session> HandleEachSession(List<List<string>> SessionSet)
        {
            List<Session> sessionList = new List<Session>();
            foreach (var session in SessionSet)
            {
                List<TripleSet> tripleSet = new List<TripleSet>();
                foreach (var line in session)
                {
                    string lineCopy = line;
                    for (int i = Variable.VariableZero; i < Variable.Threshold; i++)
                        foreach (var complex in complex2Single)
                            if (lineCopy.Contains(complex.Key))
                                lineCopy = lineCopy.Replace(complex.Key, complex.Value);
                    if (lineCopy.Contains(Variable.QString))
                    {
                        var answerRelated = GetAnswerRelated(lineCopy);
                        string question = Variable.NullString;
                        string answer = Variable.NullString;
                        Line2QuestionAnswer(lineCopy, out question, out answer);
                        question = ReFormatQuestion(question);
                        tripleSet.Add(new TripleSet(TripleType.Question.ToString(), Variable.NullString, Variable.NullString, Variable.NullString, question, answer, answerRelated));
                    }
                    else
                    {
                        var tripleList = String2Triple(lineCopy);
                        foreach (var triple in tripleList)
                            tripleSet.Add(new TripleSet(TripleType.Facts.ToString(), triple.SubjectValue, triple.PredicateValue, triple.ObjectValue, Variable.NullString, Variable.NullString, new List<int>()));
                    }
                }
                Session currentSession = new Session(tripleSet);
                sessionList.Add(currentSession);
            }
            return sessionList;
        }
        static string ReFormatQuestion(string question)
        {
            string result = Variable.NullString;
            string[] subQuestion = question.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = Variable.VariableSingle; i < subQuestion.Length; i++)
                result += subQuestion[i] + Variable.SpaceString;
            return result.Trim();
        }
        static void Line2QuestionAnswer(string LineContent, out string question, out string answer)
        {
            string[] subContent = LineContent.Split(new char[] { Variable.TabChar }, StringSplitOptions.RemoveEmptyEntries);
            answer = subContent[Variable.VariableSingle].ToString().Trim();
            if (answer.Contains(Variable.CommaString))
            {
                string[] answerArray = answer.Split(new char[] { Variable.CommaChar }, StringSplitOptions.RemoveEmptyEntries);
                var answerArrayList = answerArray.ToList();
                bool flag = false;
                foreach (var ele in answerArrayList)
                    if (ele == Variable.EastAcro || ele == Variable.NorthAcro || ele == Variable.SouthAcro || ele == Variable.WestAcro)
                        flag = true;
                if (flag)
                {
                    for (int i = Variable.VariableZero; i < answerArrayList.Count; i++)
                    {
                        string current = answerArrayList[i];
                        answerArrayList[i] = reductionTrans[current];
                    }
                    string result = Variable.NullString;
                    foreach (var ele in answerArrayList)
                        result += ele + Variable.CommaString;
                    result = result.Remove(result.Length - Variable.VariableSingle);
                    answer = result;
                }
            }
            var tempQuestion = subContent[Variable.VariableZero].Trim();
            string[] subTempQuestion = tempQuestion.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            var tempQuestionList = subTempQuestion.ToList();
            question = Variable.NullString;
            foreach (var e in tempQuestionList)
                question += e + Variable.SpaceString;
            question = question.Trim();

        }
        static string QuestionAnswer2Fact(string question, string answer)
        {
            question = question.Replace(Variable.QChar, Variable.SpaceChar).Trim();
            if (question.Contains(WordSet.WIS))
            {
                var subject = question.Replace(WordSet.WIS, Variable.NullString).Trim();
                return subject + Variable.SpaceString + WordSet.ISIN+Variable.SpaceString + answer;
            }
            else if (question.Contains(WordSet.WAS))
            {
                var subject = question.Replace(WordSet.WAS, Variable.NullString).Trim();
                subject = subject.Replace(WordSet.BEFORE, WordSet.WIN+Variable.SpaceString + answer + Variable.SpaceString+WordSet.BEFORE);
                return subject;
            }
            else if (question.Contains(WordSet.WHAT+Variable.SpaceString+WordSet.IS))
            {
                var temp = question.Replace(WordSet.WHAT, answer);
                return temp;
            }
            else if (question.Contains(WordSet.WHO))
            {
                var temp = question.Replace(WordSet.WHO, answer);
                return temp;
            }
            else if (question.Contains(WordSet.HM))
            {
                var temp = question.Replace(WordSet.HM, answer);
                return temp;
            }
            else if (question.Contains(WordSet.WC))
            {
                var temp = question.Replace(WordSet.WC, answer);
                return temp;
            }
            else if (question.Contains(WordSet.WILL))
            {
                var temp = question.Replace(WordSet.WILL, Variable.SpaceString).Trim();
                return temp;
            }
            else if (question.StartsWith(WordSet.IS))
            {
                var temp = question.Replace(WordSet.IS, Variable.SpaceString).Trim();
                if (answer == WordSet.YES)
                {
                    if (temp.Contains(WordSet.IN))
                        return temp.Replace(WordSet.IN, WordSet.ISIN);
                    else if (temp.Contains(WordSet.TO))
                        return temp.Replace(WordSet.TO, WordSet.ISTO);
                    else if (temp.Contains(WordSet.BELOW))
                        return temp.Replace(WordSet.BELOW,WordSet.ISBELOW);
                    else if (temp.Contains(WordSet.ABOVE))
                        return temp.Replace(WordSet.ABOVE, WordSet.ISABOVE);
                }
                else if (answer == WordSet.NO)
                {
                    if (temp.Contains(WordSet.IN))
                        return temp.Replace(WordSet.IN ,WordSet.ISNIN);
                    else if (temp.Contains(WordSet.TO))
                        return temp.Replace(WordSet.TO, WordSet.ISNTO);
                    else if (temp.Contains(WordSet.BELOW))
                        return temp.Replace(WordSet.BELOW, WordSet.ISNBELOW);
                    else if (temp.Contains(WordSet.ABOVE))
                        return temp.Replace(WordSet.ABOVE, WordSet.ISNABOVE);
                }
                else
                {

                    temp = temp.Replace(WordSet.IN, WordSet.MII);
                    return temp;
                }
            }
            else if (question.StartsWith(WordSet.WD))
            {
                var temp = question.Replace(WordSet.GO, WordSet.GO + Variable.SpaceString + answer);
                return temp;
            }
            else if (question.StartsWith(WordSet.HDYG))
            {
                var temp = question.Replace(WordSet.HDYG, Variable.SpaceString).Trim();
                return temp;
            }
            else if (question.StartsWith(WordSet.WHEREDOES))
            {
                var temp = question.Replace(WordSet.WHEREDOES, Variable.SpaceString).Trim();
                temp = temp.Replace(WordSet.GO, WordSet.GO + Variable.SpaceString + answer);
                return temp;
            }
            else if (question.StartsWith(WordSet.WHYDID))
            {
                var temp = question.Replace(WordSet.WHYDID, Variable.SpaceString).Trim();
                return temp;
            }
            return Variable.NullString;
        }
        static List<int> GetAnswerRelated(string line)
        {
            List<int> result = new List<int>();
            string[] subLine = line.Split(new char[] { Variable.TabChar }, StringSplitOptions.RemoveEmptyEntries);
            string containIndex = subLine[subLine.Length - 1];
            var tempResult = containIndex.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var e in tempResult)
                result.Add(int.Parse(e));
            return result;
        }
        static List<TripleTerm> String2Triple(string sentence)
        {
            return Fact2Triple(sentence);
        }
        static List<TripleTerm> Fact2Triple(string fact)
        {
            string[] subFact = fact.Split(new char[] { Variable.SpaceChar }, StringSplitOptions.RemoveEmptyEntries);
            List<string> wordList = subFact.ToList();
            while (true)
            {
                wordList.Remove(Variable.DotChar.ToString());
                wordList.Remove(WordSet.The);
                wordList.Remove(WordSet.THE);
                wordList.Remove(WordSet.THERE);
                wordList.Remove(WordSet.THERE + Variable.DotChar.ToString());
                if (!wordList.Contains(Variable.DotChar.ToString()) && !wordList.Contains(WordSet.The) && !wordList.Contains(WordSet.THE) && !wordList.Contains(WordSet.THERE) && !wordList.Contains(WordSet.THERE + Variable.DotChar.ToString()))
                    break;
            }
            string cleanedSentence = Variable.NullString;
            for (int i = Variable.VariableSingle; i < wordList.Count; i++)
                cleanedSentence += wordList[i] + Variable.SpaceString;
            cleanedSentence = cleanedSentence.Trim();
            cleanedSentence = cleanedSentence.Replace(Variable.DotChar, Variable.SpaceChar).Trim();
            var result = OpenIE(ref cleanedSentence);
            if (result.Count != Variable.VariableZero)
            {
                int index = Variable.NegInit;
                for (int i = Variable.VariableZero; i < result.Count; i++)
                {
                    var temp = result[i];
                    string output = temp.SubjectValue + Variable.SpaceString + temp.PredicateValue + Variable.SpaceString + temp.ObjectValue;
                    if (output.Length == cleanedSentence.Length)
                        index = i;
                }
                return new List<TripleTerm> { result[index] };
            }
            else
            {
                List<TripleTerm> rr = new List<TripleTerm>();
                if (cleanedSentence.Contains(Variable.SpaceString + WordSet.GAVE + Variable.SpaceString))
                {
                    var indexGave = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.GAVE + Variable.SpaceString);
                    var indexTo = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.TO + Variable.SpaceString);
                    var s1 = cleanedSentence.Substring(Variable.VariableZero, indexGave);
                    var p1 = WordSet.GAVE.ToUpper();
                    var o1 = cleanedSentence.Substring(indexGave + p1.Length + Variable.SpaceString.Length + Variable.SpaceString.Length, indexTo - indexGave - (p1.Length + Variable.SpaceString.Length + Variable.SpaceString.Length));
                    var s2 = o1;
                    var p2 = WordSet.TO;
                    var o2 = cleanedSentence.Substring(indexTo + p1.Length);
                    rr.Add(new TripleTerm(s1, p1, o1));
                    rr.Add(new TripleTerm(s2, p2, o2));

                }
                else if (cleanedSentence.Contains(Variable.SpaceString + WordSet.PASSED + Variable.SpaceString))
                {
                    var indexGave = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.PASSED + Variable.SpaceString);
                    var indexTo = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.TO + Variable.SpaceString);
                    var s1 = cleanedSentence.Substring(Variable.VariableZero, indexGave);
                    var p1 = WordSet.PASSED.ToUpper();
                    var o1 = cleanedSentence.Substring(indexGave + Variable.SpaceString.Length + Variable.SpaceString.Length + p1.Length, indexTo - indexGave - (Variable.SpaceString.Length + Variable.SpaceString.Length + p1.Length));
                    var s2 = o1;
                    var p2 = WordSet.TO.ToUpper();
                    var o2 = cleanedSentence.Substring(indexTo + p1.Length - Variable.SpaceString.Length - Variable.SpaceString.Length);
                    rr.Add(new TripleTerm(s1, p1, o1));
                    rr.Add(new TripleTerm(s2, p2, o2));
                }
                else if (cleanedSentence.Contains(Variable.SpaceString + WordSet.HANDED + Variable.SpaceString))
                {
                    var indexGave = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.HANDED + Variable.SpaceString);
                    var indexTo = cleanedSentence.IndexOf(Variable.SpaceString + WordSet.TO + Variable.SpaceString);
                    var s1 = cleanedSentence.Substring(Variable.VariableZero, indexGave);
                    var p1 = WordSet.HANDED.ToUpper();
                    var o1 = cleanedSentence.Substring(indexGave + Variable.SpaceString.Length + Variable.SpaceString.Length + p1.Length, indexTo - indexGave - (Variable.SpaceString.Length + Variable.SpaceString.Length + p1.Length));
                    var s2 = o1;
                    var p2 = WordSet.TO.ToUpper();
                    var o2 = cleanedSentence.Substring(indexTo + p1.Length - Variable.SpaceString.Length - Variable.SpaceString.Length);
                    rr.Add(new TripleTerm(s1, p1, o1));
                    rr.Add(new TripleTerm(s2, p2, o2));
                }
                return rr;
            }
        }
        static List<TripleTerm> OpenIE(ref string sourceString)
        {
            List<TripleTerm> result = new List<TripleTerm>();
            if ((sourceString.Contains(Variable.SpaceString + WordSet.GAVE + Variable.SpaceString) && sourceString.Contains(Variable.SpaceString + WordSet.TO + Variable.SpaceString))
                || (sourceString.Contains(Variable.SpaceString + WordSet.HANDED + Variable.SpaceString) && sourceString.Contains(Variable.SpaceString + WordSet.TO + Variable.SpaceString))
                || (sourceString.Contains(Variable.SpaceString + WordSet.PASSED + Variable.SpaceString) && sourceString.Contains(Variable.SpaceString + WordSet.TO + Variable.SpaceString)))
                new List<TripleTerm>();
            else
            {
                string replaced = WordSet.GOTO;
                string toReplaceOne = WordSet.ITLO;
                string toReplaceTwo = WordSet.ITRO;
                if (sourceString.Contains(toReplaceOne) || sourceString.Contains(toReplaceTwo))
                {

                    if (sourceString.Contains(toReplaceOne))
                    {
                        string temp = Variable.NullString;
                        temp = sourceString.Replace(toReplaceOne, replaced);
                        var triple = ExtractTriple(temp);
                        foreach (var e in triple)
                        {
                            if (e.SubjectValue.Contains(replaced))
                                e.SubjectValue = e.SubjectValue.Replace(replaced, toReplaceOne);
                            if (e.PredicateValue.Contains(replaced))
                                e.PredicateValue = e.PredicateValue.Replace(replaced, toReplaceOne);
                            if (e.ObjectValue.Contains(replaced))
                                e.ObjectValue = e.ObjectValue.Replace(replaced, toReplaceOne);
                        }
                        result = triple;
                        sourceString = sourceString.Replace(replaced, toReplaceOne);
                    }
                    else
                    {
                        string temp = Variable.NullString;
                        temp = sourceString.Replace(toReplaceTwo, replaced);
                        var triple = ExtractTriple(temp);
                        foreach (var e in triple)
                        {
                            if (e.SubjectValue.Contains(replaced))
                                e.SubjectValue = e.SubjectValue.Replace(replaced, toReplaceTwo);
                            if (e.PredicateValue.Contains(replaced))
                                e.PredicateValue = e.PredicateValue.Replace(replaced, toReplaceTwo);
                            if (e.ObjectValue.Contains(replaced))
                                e.ObjectValue = e.ObjectValue.Replace(replaced, toReplaceTwo);
                        }
                        result = triple;
                        sourceString = sourceString.Replace(replaced, toReplaceTwo);
                    }
                }
                else
                {
                    if (sourceString.Contains(WordSet.NORTH))
                    {
                        sourceString = sourceString.Replace(WordSet.NORTH, WordSet.EAST);
                        var temp = ExtractTriple(sourceString);
                        foreach (var e in temp)
                        {
                            if (e.SubjectValue.Contains(WordSet.EAST))
                                e.SubjectValue = e.SubjectValue.Replace(WordSet.EAST, WordSet.NORTH);
                            if (e.PredicateValue.Contains(WordSet.EAST))
                                e.PredicateValue = e.PredicateValue.Replace(WordSet.EAST, WordSet.NORTH);
                            if (e.ObjectValue.Contains(WordSet.EAST))
                                e.ObjectValue = e.ObjectValue.Replace(WordSet.EAST, WordSet.NORTH);
                        }
                        result = temp;
                        sourceString = sourceString.Replace(WordSet.EAST, WordSet.NORTH);
                    }
                    else if (sourceString.Contains(WordSet.SOUTH))
                    {
                        sourceString = sourceString.Replace(WordSet.SOUTH, WordSet.EAST);
                        var temp = ExtractTriple(sourceString);
                        foreach (var e in temp)
                        {
                            if (e.SubjectValue.Contains(WordSet.EAST))
                                e.SubjectValue = e.SubjectValue.Replace(WordSet.EAST, WordSet.SOUTH);
                            if (e.PredicateValue.Contains(WordSet.EAST))
                                e.PredicateValue = e.PredicateValue.Replace(WordSet.EAST, WordSet.SOUTH);
                            if (e.ObjectValue.Contains(WordSet.EAST))
                                e.ObjectValue = e.ObjectValue.Replace(WordSet.EAST, WordSet.SOUTH);
                        }
                        result = temp;
                        sourceString = sourceString.Replace(WordSet.EAST, WordSet.SOUTH);

                    }
                    else
                    {
                        if (sourceString.Contains(WordSet.ISNL))
                        {
                            sourceString = sourceString.Replace(WordSet.ISNL, WordSet.IS);
                            var temp = ExtractTriple(sourceString);
                            foreach (var e in temp)
                            {
                                if (e.SubjectValue.Contains(WordSet.IS))
                                    e.SubjectValue = e.SubjectValue.Replace(WordSet.IS, WordSet.ISNL);
                                if (e.PredicateValue.Contains(WordSet.IS))
                                    e.PredicateValue = e.PredicateValue.Replace(WordSet.IS, WordSet.ISNL);
                                if (e.ObjectValue.Contains(WordSet.IS))
                                    e.ObjectValue = e.ObjectValue.Replace(WordSet.IS, WordSet.ISNL);
                            }
                            result = temp;
                            sourceString = sourceString.Replace(WordSet.IS, WordSet.ISNL);
                        }
                        else if (sourceString.Contains(WordSet.ISNIN))
                        {
                            sourceString = sourceString.Replace(WordSet.ISNIN, WordSet.ISIN);
                            var temp = ExtractTriple(sourceString);
                            foreach (var e in temp)
                            {
                                if (e.SubjectValue.Contains(WordSet.ISIN))
                                    e.SubjectValue = e.SubjectValue.Replace(WordSet.ISIN, WordSet.ISNIN);
                                if (e.PredicateValue.Contains(WordSet.ISIN))
                                    e.PredicateValue = e.PredicateValue.Replace(WordSet.ISIN, WordSet.ISNIN);
                                if (e.ObjectValue.Contains(WordSet.ISIN))
                                    e.ObjectValue = e.ObjectValue.Replace(WordSet.ISIN, WordSet.ISNIN);
                            }
                            sourceString = sourceString.Replace(WordSet.ISIN, WordSet.ISNIN);
                            result = temp;
                        }
                        else
                        {
                            if (sourceString.Contains(WordSet.EI + Variable.SpaceString))
                            {
                                sourceString = sourceString.Replace(WordSet.EI + Variable.SpaceString, Variable.NullString);
                                int index = sourceString.IndexOf(Variable.VariableOr);
                                string subString = sourceString.Substring(Variable.VariableZero, index).Trim();
                                string toAppend = sourceString.Substring(index).Trim();
                                var temp = ExtractTriple(subString);
                                foreach (var e in temp)
                                    e.ObjectValue = e.ObjectValue + Variable.SpaceString + toAppend;
                                result = temp;
                            }
                            else
                            {
                                if (sourceString.Contains(WordSet.AFT))
                                {
                                    sourceString = sourceString.Replace(WordSet.AFT, Variable.NullString).Trim();
                                    var temp = ExtractTriple(sourceString);
                                    foreach (var e in temp)
                                        e.SubjectValue = WordSet.AFT + Variable.SpaceString + e.SubjectValue;
                                    result = temp;
                                    sourceString = WordSet.AFT + Variable.SpaceString + sourceString;
                                }

                                else if (sourceString.Contains(WordSet.AW))
                                {
                                    sourceString = sourceString.Replace(WordSet.AW, Variable.NullString).Trim();
                                    var temp = ExtractTriple(sourceString);
                                    foreach (var e in temp)
                                        e.SubjectValue = WordSet.AW + Variable.SpaceString + e.SubjectValue;
                                    result = temp;
                                    sourceString = WordSet.AW + Variable.SpaceString + sourceString;

                                }
                                else if (sourceString.Contains(WordSet.FT))
                                {
                                    sourceString = sourceString.Replace(WordSet.FT, Variable.NullString).Trim();
                                    var temp = ExtractTriple(sourceString);
                                    foreach (var e in temp)
                                        e.SubjectValue = WordSet.FT + Variable.SpaceString + e.SubjectValue;
                                    result = temp;
                                    sourceString = WordSet.FT + Variable.SpaceString + sourceString;

                                }
                                else if (sourceString.Contains(WordSet.THEN))
                                {
                                    sourceString = sourceString.Replace(WordSet.THEN, Variable.NullString).Trim();
                                    var temp = ExtractTriple(sourceString);
                                    foreach (var e in temp)
                                        e.SubjectValue = WordSet.THEN + Variable.SpaceString + e.SubjectValue;
                                    result = temp;
                                    sourceString = WordSet.THEN + Variable.SpaceString + sourceString;

                                }
                                else
                                {
                                    if (sourceString.Contains(Variable.VariableAnd + Variable.SpaceString))
                                    {
                                        int index = sourceString.IndexOf(Variable.VariableAnd + Variable.SpaceString);
                                        string subString = sourceString.Substring(index + Variable.NodeThreshold).Trim();
                                        string toAppend = sourceString.Substring(Variable.VariableZero, index + Variable.NodeThreshold).Trim();
                                        var temp = ExtractTriple(subString);
                                        foreach (var e in temp)
                                            e.SubjectValue = toAppend + Variable.SpaceString + e.SubjectValue;
                                        result = temp;
                                    }
                                    else
                                    {
                                        if (sourceString.Contains(Variable.SpaceString + WordSet.ISA + Variable.SpaceString))
                                        {
                                            sourceString = sourceString.Replace(Variable.SpaceString + WordSet.ISA + Variable.SpaceString, Variable.SpaceString + "is" + Variable.SpaceString);
                                            var temp = ExtractTriple(sourceString);
                                            foreach (var e in temp)
                                                e.PredicateValue = e.PredicateValue.Replace(WordSet.IS, WordSet.ISA);
                                            sourceString = sourceString.Replace(Variable.SpaceString + WordSet.IS + Variable.SpaceString, Variable.SpaceString + "is a" + Variable.SpaceString);
                                            result = temp;
                                        }
                                        else
                                        {
                                            if (sourceString.Contains(WordSet.BOC))
                                            {
                                                sourceString = sourceString.Replace(WordSet.BOC, WordSet.TBox);
                                                var temp = ExtractTriple(sourceString);
                                                foreach (var e in temp)
                                                {
                                                    if (e.SubjectValue.Contains(WordSet.TBox))
                                                        e.SubjectValue = e.SubjectValue.Replace(WordSet.TBox, WordSet.BOC);
                                                    if (e.PredicateValue.Contains(WordSet.TBox))
                                                        e.PredicateValue = e.PredicateValue.Replace(WordSet.TBox, WordSet.BOC);
                                                    if (e.ObjectValue.Contains(WordSet.TBox))
                                                        e.ObjectValue = e.ObjectValue.Replace(WordSet.TBox, WordSet.BOC);
                                                }
                                                sourceString = sourceString.Replace(WordSet.TBox, WordSet.BOC);
                                                result = temp;
                                            }
                                            else
                                            {
                                                result = ExtractTriple(sourceString);
                                                foreach (var e in startTimeStamp)
                                                {
                                                    if (sourceString.StartsWith(e))
                                                    {
                                                        sourceString = sourceString.Replace(e, Variable.NullString).Trim();
                                                        var temp = ExtractTriple(sourceString);
                                                        foreach (var triple in temp)
                                                            triple.SubjectValue = e + Variable.SpaceString + triple.SubjectValue;
                                                        sourceString = e + Variable.SpaceString + sourceString;
                                                        result = temp;
                                                        break;
                                                    }
                                                }
                                                foreach (var e in endTimeStamp)
                                                {
                                                    if (sourceString.EndsWith(e))
                                                    {
                                                        sourceString = sourceString.Replace(e, Variable.NullString).Trim();
                                                        var temp = ExtractTriple(sourceString);
                                                        foreach (var triple in temp)
                                                            triple.ObjectValue = triple.ObjectValue + Variable.SpaceString + e;
                                                        sourceString = sourceString + Variable.SpaceString + e.Replace(Variable.DotChar, Variable.SpaceChar).Trim();
                                                        result = temp;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        static List<TripleTerm> ExtractTriple(string content)
        {
            List<TripleTerm> result = new List<TripleTerm>();
            edu.stanford.nlp.simple.Document doc = new edu.stanford.nlp.simple.Document(content);
            var list = doc.sentences();
            for (int i = Variable.VariableZero; i < list.size(); i++)
            {
                edu.stanford.nlp.simple.Sentence sentence = (edu.stanford.nlp.simple.Sentence)list.get(i);
                var tripleList = sentence.openieTriples();
                for (int j = Variable.VariableZero; j < tripleList.size(); j++)
                {
                    var tripleListArray = tripleList.toArray();
                    RelationTriple relationTriple = (RelationTriple)tripleListArray[j];
                    var currentTriple = new TripleTerm(
                          relationTriple.subjectGloss(),
                           relationTriple.relationGloss(),
                           relationTriple.objectGloss());
                    result.Add(currentTriple);
                }
            }
            return result;
        }
        static void PipeLine(int taskID, FileType fileType)
        {
            var fileName = GetFileName(taskID, fileType);
            var lineStringSet = ReadFile(fileName);
            var sessionList = SplitBySession(lineStringSet);
            var sessionSet = HandleEachSession(sessionList);
            SaveCell(sessionSet, fileType, taskID);
        }
        static void SaveCell(List<Session> sessionSet, FileType fileType, int taskID)
        {
            if (fileType == FileType.Training)
            {
                TrainingSet trainingSet = new TrainingSet(taskID, sessionSet);
                Global.LocalStorage.SaveTrainingSet(trainingSet);
            }
            else if (fileType == FileType.Test)
            {
                TestSet testSet = new TestSet(taskID, sessionSet);
                Global.LocalStorage.SaveTestSet(testSet);
            }
            else if (fileType == FileType.Valid)
            {
                ValidSet validSet = new ValidSet(taskID, sessionSet);
                Global.LocalStorage.SaveValidSet(validSet);
            }
        }
        static string GetFileName(int taskIndex, FileType type)
        {
            if (type == FileType.Training)
                return fileNamePrefix + taskIndex.ToString() + trainingFileNameSuffix;
            else if (type == FileType.Test)
                return fileNamePrefix + taskIndex.ToString() + testFileNameSuffix;
            else
                return new Exception(Variable.InvalidType).ToString();
        }
        #endregion
    }
}

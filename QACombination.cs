using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class QACombination
    {
        List<KeyValuePair<string, VariableType>> _question;
        List<KeyValuePair<string, VariableType>> _answer;
        public List<KeyValuePair<string, VariableType>> Question { get { return _question; } }
        public List<KeyValuePair<string, VariableType>> Answer { get { return _answer; } }
        private QACombination() { }
        public QACombination(List<KeyValuePair<string, VariableType>> question, List<KeyValuePair<string, VariableType>> answer)
        {
            _question = question;
            _answer = answer;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class QAStructure
    {
        List<string> _question;
        List<string> _answer;
        private QAStructure() { }
        public List<string> Question { get { return _question; } }
        public List<string> Answer { get { return _answer; } }
        public QAStructure(List<string> question, List<string> answer)
        {
            _question = question;
            _answer = answer;
        }
    }
}

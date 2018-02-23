using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public enum FileType
    {
        Training,
        Test,
        Valid
    }
    public enum TripleType
    {
        Question,
        Facts
    }
    public enum VariableType
    {
        Subject,
        Predicate,
        Object,
        PartOfSubject,
        PartOfPredicate,
        PartOfObject,
        Unknown
    }
}

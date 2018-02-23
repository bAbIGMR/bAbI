using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMR
{
    public class Predicate
    {
        List<string> _predicate = new List<string> { "$A$", "$B$", "$C$", "$D$",
        "$E$","$F$","$G$","$H$","$I$","$J$","$K$","$L$","$M$","$N$","$O$","$P$",
        "$Q$","$R$","$S$","$T$","$U$","$V$","$W$","$X$","$Y$","$Z$","$AA$", "$BB$", "$CC$", "$DD$",
        "$EE$","$FF$","$GG$","$HH$","$II$","$JJ$","$KK$","$LL$","$MM$","$NN$","$OO$","$PP$",
        "$QQ$","$RR$","$SS$","$TT$","$UU$","$VV$","$WW$","$XX$","$YY$","$ZZ$"};
        int index = 0;
        public List<string> GetAllPredicate { get { return _predicate; } }
        public string GetAvailiablePredicateSymbol { get { return _predicate[index++]; } }
        public Predicate(int currentCount)
        { index = currentCount; }
        public Predicate() { }
    }
}

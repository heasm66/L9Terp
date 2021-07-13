using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L9Terp
{
    class L9Header
    {
        /* Global header information, This is absolut addresses inside the infile */
        public int dictdata, absdatablock, startmd, startabbrev, acodeptr, offset;
        public int enddictdata, endmd,endabbrev;
        public string gamename;
        public int L9MsgType;
        public int L9Version;
    }
}

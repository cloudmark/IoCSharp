using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoCSharp.Exceptions
{
    public class IoCSharpException: Exception
    {
        public IoCSharpException(string str): base(str){}
        public IoCSharpException(string str, Exception e) : base(str, e) { }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    internal class KLIBException : Implement.InternalException
    {
        public KLIBException(string message) : base(message)
        {
        }

        public KLIBException(Exception ex) : base(ex)
        {
        }

        public KLIBException(int code, params object[] values) : base(code, values)
        {

        }
    }
}

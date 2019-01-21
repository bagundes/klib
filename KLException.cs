using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public class KLException : Exception
    {
        private string message;
        public override string Message => message;

        public KLException(string message) : base(message)
        {

        }

        public KLException(int code,params object[] values)
        {
            message = MsgDecode(code, values);
        }

        public KLException(string message, Exception ex)
        {

        }

        private string MsgDecode(int code,params object[] values)
        {
            throw new NotImplementedException();
        }
    }
}

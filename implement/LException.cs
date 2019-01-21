using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.implement
{
    public abstract class LException : System.Exception
    {
        private string message;
        public override string Message => this.message;

        public LException(string message) : base(message)
        {
            this.message = message;
        }

        public LException(int code, params object[] values)
        {
            message = MsgDecode(code, values);
        }

        public LException(Exception ex)
        {

        }

        private string MsgDecode(int code, params object[] values)
        {
            var foo = String.Join(",", values.ToArray());
            return $"{code.ToString("0000")} - {foo}";
            //throw new NotImplementedException();
        }
    }
}

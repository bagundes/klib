using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace klib.implement
{
    public abstract class LException : System.Exception
    {
        protected string message;
        public override string Message => MsgDecode(R.Project.LocationResx);
        public readonly int Code;
        protected readonly object[] Values;

        public LException(string message) : base(message)
        {
            Code = 1;
            Values = new object[0];
        }

        public LException(int code, params object[] values)
        {
            Code = code;
            Values = values;
        }

        public LException(Exception ex)
        {
            Code = 1;
            Values = new object[0];
        }

        protected string MsgDecode(ResourceManager resx)
        {
            var a = R.Project.Resources;
            var foo = $"L{Code.ToString("00000")}_{Values.Length}";
            
            //klib.content.Location.resources
            var msg = String.Empty;
            
            try
            {
                msg = resx.GetString(foo);
                msg = msg ?? $"{Code.ToString("0000")} - Args: {String.Join(", ", Values)}";
                return String.Format(msg, Code, Values);
            }
            catch
            {
                return $"{Code.ToString("0000")}: {msg} - Args: {String.Join(", ", Values)}";
            }
        }
    }
}

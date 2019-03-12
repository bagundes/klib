using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace klib.Implement
{
    /// <summary>
    /// This is a internal exception. This abstract class is possible trigger a message in the Location resource.
    /// All the projects needs to inheritance this class.
    /// </summary>
    public abstract class InternalException : System.Exception
    {
        public virtual string LOG => "GEXCEP";
        protected string message;
        public override string Message => MsgDecode(R.Project.LocationResx);
        public readonly int Code;
        protected readonly object[] Values;

        public InternalException(string message) : base(message)
        {
            Code = 1;
            Values = new object[0];
        }

        public InternalException(int code, params object[] values)
        {
            Code = code;
            Values = values;
        }

        public InternalException(Exception ex)
        {
            Code = 1;
            Values = new object[0];
        }

        protected virtual string MsgDecode(ResourceManager resx)
        {
            
            var foo = $"L{Code.ToString("00000")}_{Values.Length}";
            var fstring = new List<object>();
            fstring.Add(Code);
            fstring.AddRange(Values);


            //klib.content.Location.resources
            var msg = String.Empty;
            
            try
            {
                msg = resx.GetString(foo);
                msg = msg ?? $"{Code.ToString("0000")} - Args ({Values.Count()}): {String.Join(", ", Values)}";
                return String.Format(msg, fstring.ToArray());
            }
            catch(Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, LOG, ex);
                return $"{Code.ToString("0000")}: {msg} - Args: {String.Join(", ", Values)}";
            }
        }
    }
}

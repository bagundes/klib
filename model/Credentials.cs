using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.model
{
    public class Credentials1
    {
        public string User { get; protected set; }
        public string Passwd { get; protected set; }
        public string Security { get; protected set; }
        

        public Credentials1(string user, string key)
        {
            Security = Shell.StringMixer(true, R.Security.MasterKey, user);

            User = user;
            Passwd = klib.Shell.Decrypt1(key, Security);
        }

        public Credentials1(string user)
        {
            User = user;
            Security = Shell.StringMixer(true, R.Security.MasterKey, user);
        }

        /// <summary>
        /// Create a key.
        /// </summary>
        /// <param name="passwd"></param>
        /// <returns></returns>
        public string NewKey(string passwd)
        {
            Passwd = passwd;
            return klib.Shell.Encrypt1(passwd, Security);
        }
    }
}

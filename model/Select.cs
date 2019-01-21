using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.model
{
    public class Select
    {
        public string Name;
        public Values Value;
        public bool Default;

        public Select(string name, object value, bool def = false)
        {
            Name = name;
            Value = ValuesEx.To(value);
            Default = def;
        }
    }
}

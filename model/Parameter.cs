using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.model
{
    public class Parameter
    {
        public int Code;
        public string Param;
        public string Diff1;
        public string Diff2;
        public string Diff3;
        public int Order = 0;
        public Dynamic Value;
        public int BPLId;
        public DateTime? DueDate;

        public override string ToString()
        {
            return $"Code:{Code}|Param:{Param}|Diff1/2/3: \"{Diff1}\"/\"{Diff1}\"/\"{Diff3}\"|Order:{Order}|BPLId:{BPLId}|DueDate:{DueDate}";
        }
    }
}

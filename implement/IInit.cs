using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib.Implement
{
    public interface IInit
    {
        void Construct();
        void Configure();
        void Populate();
        void Destruct();
        
    }
}

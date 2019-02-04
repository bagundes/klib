using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace klib
{
    public static class E
    {
        public static class RegexMask
        {
            public static string OnlyLetterAndNumbers => "[^0-9a-zA-Z]+";
            public static string ValidNumbers => @"^[\d,.?!]+$";
        }

        public enum Resource
        {
            srf,
            sql,
        }
    }
}

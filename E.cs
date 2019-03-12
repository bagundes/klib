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

            public static string Email => @"/^[a-z0-9.]+@[a-z0-9]+\.[a-z]+\.([a-z]+)?$/i";
        }

        public enum Resource
        {
            srf,
            sql,
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace klib.core
{
    internal class Cron
    {
        private enum TypeValue
        {
            Minute,
            Hour,
            Day,
            Month,
            Week
        }


        private int[] minutes;
        private int[] hours;
        private int[] days;
        private int[] months;
        private int[] weeks;
        public DateTime Next { get; protected set; }
        private DateTime LastTime = DateTime.Now;
        /// <summary>
        /// Mask : https://crontab.guru/
        /// </summary>
        /// <param name="mask"></param>       
        public Cron(string mask)
        {
            if (!Validate(mask))
                throw new LException(2, "The cron format is not valid.");

            var values = mask.Split(' ');
            SpecialChars(values[0], TypeValue.Minute);
            SpecialChars(values[1], TypeValue.Hour);
            SpecialChars(values[2], TypeValue.Day);
            SpecialChars(values[3], TypeValue.Month);
            SpecialChars(values[4], TypeValue.Week);

            var minute = NextValue(LastTime.Minute, minutes);
            var hour = NextValue(LastTime.Hour, hours);
            var day = NextValue(LastTime.Day, days);
            var month = NextValue(LastTime.Month, months);

            Next = new DateTime(DateTime.Now.Year, month, day, hour, minute, 0);
            LastTime = LastTime.AddSeconds(-LastTime.Second); 

        }

        private int NextValue(int value, int[] values)
        {
            if (values.Count() == 1)
                return values[0];

            if (values.Contains(value))
                return value;

            var max = values.Max();

            if (value < max)
                return values.Where(t => t < value).First();
            else
                return values.Where(t => t >= value).FirstOrDefault();
        }

        public bool Validate(string mask)
        {    
            if ((mask.Split(' ')).Length != 5)
                return false;
            var regex = new Regex(@"[0-9\- /\s?,*]");
            if (!regex.IsMatch(mask))
                return false;

            return true;
        }

        /// <summary>
        /// Wait in real time
        /// </summary>
        /// <returns>values in seconds</returns>
        public int Wait(bool updateLastTime = true)
        {
            if (updateLastTime)
                LastTime = DateTime.Now;

            LastTime = LastTime.AddSeconds(-LastTime.Second);

            return (int) (Next.TimeOfDay.TotalSeconds - LastTime.TimeOfDay.TotalSeconds); 
        }

        public bool IsNow(bool updateLastTime = false)
        {
            if (updateLastTime)            
                LastTime = DateTime.Now;

            if (!weeks.Where(t => t == (int)LastTime.DayOfWeek).Any())
                return false;


            LastTime = LastTime.AddSeconds(-LastTime.Second);
            return klib.ValuesEx.To(Next).Compare(LastTime) == 0;      
        }
        /// <summary>
        /// Chars valids
        /// * any value
        /// , value list separator
        /// - range of values
        /// / step values
        /// </summary>
        /// <param name="info"></param>
        private void SpecialChars(string info, TypeValue typeValue)
        {
            try
            {
                var number = int.Parse(info);
                Load(typeValue, number, number, 1);
            }
            catch
            {
                for (int i = 0; i < info.Length; i++)
                {
                    switch (info[i])
                    {
                        case '*':
                            Load(typeValue, 0, 0, 1);
                            break;
                        case '-':
                            var numbers1 = info.Split('-');
                            Load(typeValue, int.Parse(numbers1[0]), int.Parse(numbers1[1]), 1); break;
                        case '/':
                            var numbers2 = info.Split('/');
                            Load(typeValue, int.Parse(numbers2[0]), 0, int.Parse(numbers2[1])); break;
                        default: break;
                    }
                }
            }
        }

        private void Load(TypeValue typeValue, int start, int end, int step)
        {
            var max = 0;
            var min = 0;
            var zero = 0;
            switch(typeValue)
            {
                case TypeValue.Minute: min = 0; max = 59; zero = 1; break;
                case TypeValue.Hour: min = 0; max = 23; zero = 1;  break;
                case TypeValue.Day: min = 1; max = 31; break;
                case TypeValue.Month: min = 1; max = 12; break;
                case TypeValue.Week: min = 0; max = 6; zero = 1;  break;
                default:
                    throw new LException(2, "Type Value invalid in Cron Load.");
            }

            // If values is zero
            end = end == 0 ? max : end;
            start = start == 0 ? min : start;

            // Validation the conditions
            if (start < min || start > max || end < min || end > max || start > end)
                throw new LException(2, $"Value of {typeValue.ToString()} is wrong in the Cron");

            int size = (end - start + zero) / step;
            var foo = new int[size];
            int c = 0;
            for(int i = start; i < end + zero; i += step)
            {
                foo[c++] = i;
            }


            switch(typeValue)
            {
                case TypeValue.Minute: minutes = foo; break;           
                case TypeValue.Hour: hours = foo;  break;
                case TypeValue.Day: days = foo; break;
                case TypeValue.Month: months = foo; break;
                case TypeValue.Week: weeks = foo;  break;
                default:
                    throw new LException(2, "Type Value invalid in Cron Load.");
            }

        }
    }
}

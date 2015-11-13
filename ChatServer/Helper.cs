using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Helper
    {
        /*Telnet feature
         * telnet on some version windows will treat backspace as moving cursor backward, not delete characters,
         * this need to be specially processed. else will get wrong output.
         * */
        public static string StringTrimBackspace(string input)
        {
            char[] outputarray = new char[input.Length];
            int i = 0;
            foreach (char c in input.ToCharArray())
            {
                if (c != '\b')
                    outputarray[i++] = c;
                else
                    i = (i == 0) ? 0 : (i - 1);
            }
            while (i < outputarray.Length && outputarray[i] != '\0')
                i++;
            return new string(outputarray, 0, i);
        }
    }
}

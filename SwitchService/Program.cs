using MercedesBenz.SystemTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchService
{
    class Program
    {
        public static void Main(string[] args)
        {
            SwitchDispose.Instance.TaskStart();
            Console.ReadKey();
        }
    }
}

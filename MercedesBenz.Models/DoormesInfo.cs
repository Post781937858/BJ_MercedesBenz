using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercedesBenz.Models
{
    public class DoormesInfo : WCSTask
    {
        public OrderTaskType doortype { get; set; }

        public int door { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachinistServer
{
    public class ErrorEventArg:EventArgs
    {
        public Exception Error { get; set; }
    }
}

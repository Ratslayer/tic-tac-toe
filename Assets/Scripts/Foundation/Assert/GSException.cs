using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GSException : Exception
{
    public GSException(string msg, params object[] args) 
        : base(args.Length > 0 ? string.Format(msg, args) : msg)
    {

    }
}


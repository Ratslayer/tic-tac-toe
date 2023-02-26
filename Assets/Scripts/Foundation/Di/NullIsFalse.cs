using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NullIsFalse
{
    public static implicit operator bool(NullIsFalse obj)
    {
        return obj != null;
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCS_Database
{
    interface Db_Interface
    {
        bool Open(ref object connector);
        bool Close(ref object connector);

        string Verify();
    }
}

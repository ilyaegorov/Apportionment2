using System;
using System.Collections.Generic;
using System.Text;

namespace Apportionment2.Interfaces
{
    public interface ISqLite
    {
        string GetDatabasePath(string filename);
    }
}

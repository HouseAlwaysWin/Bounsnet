using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Repositories.Adapters
{
    public class OracleDbAdapter : IConnAdapter
    {
        public string DbParamTag { get => ":"; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Repositories.Adapters
{
    public class MsDbAdapter : IConnAdapter
    {
        public string DbParamTag { get => "@"; }
    }
}

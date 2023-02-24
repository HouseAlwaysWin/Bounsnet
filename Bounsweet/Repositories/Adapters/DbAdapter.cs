using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Repositories.Adapters
{
    public class DbAdapter<TConn> where TConn : IDbConnection, new()
    {
        public string DbParamTag { get => connAdapter.DbParamTag; }
        public IConnAdapter connAdapter { get; set; }

        public DbAdapter()
        {
            if (typeof(TConn) == typeof(SqlConnection))
            {
                connAdapter = new MsDbAdapter();
            }
            else if (typeof(TConn) == typeof(OracleConnection))
            {
                connAdapter = new OracleDbAdapter();
            }
        }
    }
}

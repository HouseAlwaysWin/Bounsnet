using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Models
{
    public class ColSet<T> : Dictionary<Expression<Func<T, object>>, object?>
    {
    }
}

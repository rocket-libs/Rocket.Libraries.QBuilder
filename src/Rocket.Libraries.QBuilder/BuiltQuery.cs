using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocket.Libraries.QBuilder
{
    public class BuiltQuery
    {
        public string ParameterizedSql { get; set; } = string.Empty;

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
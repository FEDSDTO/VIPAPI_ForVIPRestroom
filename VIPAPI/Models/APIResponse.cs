using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VIPAPI.Models
{
    public class APIResponse
    {
        public class rtnMsg
        {
            public string Status { get; set; }
            public string Remark { get; set; }
        }
    }
}
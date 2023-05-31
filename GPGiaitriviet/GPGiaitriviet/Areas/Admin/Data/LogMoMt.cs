using GPGiaitriviet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPGiaitriviet.Areas.Admin.Data
{
    public class LogMoMt : LogMT
    {
        public string Mo { get; set; }
        public string Code { get; set; }
        public string Customer_name { get; set; }
        public string Lisense_code { get; set; }
        public string MOProduct { get; set; }
        public DateTime? Editdate { get; internal set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPGiaitriviet.Models
{
    public class GiftModels
    {

        public long ID { get; set; }
        public string Phone { get; set; }
        public Nullable<int> Count { get; set; }
        public string Product { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> Calldate { get; set; }
        public Nullable<System.DateTime> Successdate { get; set; }
        public string Callby { get; set; }
        public string Note { get; set; }

        // ADD BY TRUNGVD 2021-06-26
        public string Name { get; set; }
        public string Birthday { get; set; }
        public string Address { get; set; }
        public string Sex { get; set; }
        public Nullable<int> HHTD { get; set; }
        public Nullable<int> KTCD { get; set; }
        public Nullable<int> SUPERMAN { get; set; }
        public Nullable<int> VVG { get; set; }
    }
}
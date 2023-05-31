using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPGiaitriviet.Areas.Admin.Data
{
    public class HomeModel
    {
        public int Customer { get; set; }
        public int Code { get; set; }

        public List<Nhanhang> Nhanhang { get; set; }

    }
    public class Nhanhang
    {
        public int SoLuongKhachHang { get; set; }
        public int Tong { get; set; }
        public int Duhanmuc { get; set; }
        public int Chuatrathuong { get; set; }
        public int Datrathuong { get; set; }
        public string Code { get; set; }
        public int Ma { get; set; }
    }
}
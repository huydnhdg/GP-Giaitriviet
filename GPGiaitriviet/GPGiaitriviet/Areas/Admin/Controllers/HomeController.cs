using GPGiaitriviet.Areas.Admin.Data;
using GPGiaitriviet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        public ActionResult Index()
        {
            return RedirectToAction("Index","CodeGP");
            var model = new HomeModel();
            var customer = db.Customers.Count();
            var code = db.CodeGPs.Where(a => a.Status == 1).Count();

            model.Code = code;
            model.Customer = customer;

            //var hhtd = db.CodeGPs.Where(a => a.Category == "HHTD");
            //var ktcd = db.CodeGPs.Where(a => a.Category == "KTCD");
            //var superman = db.CodeGPs.Where(a => a.Category == "SUPERMAN");
            //var vvg = db.CodeGPs.Where(a => a.Category == "VVG");
            //var hm = db.CodeGPs.Where(a => a.Category == "MCMCU");
            //var _hhtd = new Nhanhang()
            //{
            //    SoLuongKhachHang = hhtd.Where(a => a.Phone != null).GroupBy(x => x.Phone).Select(g => g.FirstOrDefault()).Count(),
            //    Tong = hhtd.Where(a => a.Status == 1).Count(),
            //    Duhanmuc = db.GiftExchanges.Where(a => a.Product == "HHTD").Count(),
            //    Chuatrathuong = db.GiftExchanges.Where(a => a.Product == "HHTD" && (a.Status == null || a.Status == 0)).Count(),
            //    Datrathuong = db.GiftExchanges.Where(a => a.Product == "HHTD" && a.Status == 3).Count(),
            //    Ma = hhtd.Count(),
            //    Code = "HHTD"
            //};
            //var _ktcd = new Nhanhang()
            //{
            //    SoLuongKhachHang = ktcd.Where(a => a.Phone != null).GroupBy(x => x.Phone).Select(g => g.FirstOrDefault()).Count(),
            //    Tong = ktcd.Where(a => a.Status == 1).Count(),
            //    Duhanmuc = db.GiftExchanges.Where(a => a.Product == "KTCD").Count(),
            //    Chuatrathuong = db.GiftExchanges.Where(a => a.Product == "KTCD" && (a.Status == null || a.Status == 0)).Count(),
            //    Datrathuong = db.GiftExchanges.Where(a => a.Product == "KTCD" && a.Status == 3).Count(),
            //    Ma = ktcd.Count(),
            //    Code = "KTCD"
            //};
            //var _superman = new Nhanhang()
            //{
            //    SoLuongKhachHang = superman.Where(a => a.Phone != null).GroupBy(x => x.Phone).Select(g => g.FirstOrDefault()).Count(),
            //    Tong = superman.Where(a => a.Status == 1).Count(),
            //    Duhanmuc = db.GiftExchanges.Where(a => a.Product == "SUPERMAN").Count(),
            //    Chuatrathuong = db.GiftExchanges.Where(a => a.Product == "SUPERMAN" && (a.Status == null || a.Status == 0)).Count(),
            //    Datrathuong = db.GiftExchanges.Where(a => a.Product == "SUPERMAN" && a.Status == 3).Count(),
            //    Ma = superman.Count(),
            //    Code = "SUPERMAN"
            //};
            //var _vvg = new Nhanhang()
            //{
            //    SoLuongKhachHang = vvg.Where(a => a.Phone != null).GroupBy(x => x.Phone).Select(g => g.FirstOrDefault()).Count(),
            //    Tong = vvg.Where(a => a.Status == 1).Count(),
            //    Duhanmuc = db.GiftExchanges.Where(a => a.Product == "VVG").Count(),
            //    Chuatrathuong = db.GiftExchanges.Where(a => a.Product == "VVG" && (a.Status == null || a.Status == 0)).Count(),
            //    Datrathuong = db.GiftExchanges.Where(a => a.Product == "VVG" && a.Status == 3).Count(),
            //    Ma = vvg.Count(),
            //    Code = "VVG"
            //};
            //var _hm = new Nhanhang()
            //{
            //    SoLuongKhachHang = hm.Where(a => a.Phone != null).GroupBy(x => x.Phone).Select(g => g.FirstOrDefault()).Count(),
            //    Tong = hm.Where(a => a.Status == 1).Count(),
            //    Duhanmuc = db.GiftExchanges.Where(a => a.Product == "TC").Count(),
            //    Chuatrathuong = db.GiftExchanges.Where(a => a.Product == "TC" && (a.Status == null || a.Status == 0)).Count(),
            //    Datrathuong = db.GiftExchanges.Where(a => a.Product == "TC" && a.Status == 3).Count(),
            //    Ma = hm.Count(),
            //    Code = "MCMCU"
            //};
            model.Nhanhang = new List<Nhanhang>() {  };
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
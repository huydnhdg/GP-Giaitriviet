using GPGiaitriviet.Areas.Admin.Data;
using GPGiaitriviet.Models;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class LogMoController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        static string url;
       
        public ActionResult Index(string searchstring, int? page, string from_date, string to_date, string status,
            string User_ID, string product, string chanel)
        {
            var model = from a in db.LogMTs
                        join b in db.LogMOes on a.MO_ID equals b.ID
                        select new LogMoMt()
                        {
                            ID = b.ID,
                            Phone = a.Phone,
                            Code = b.Code,
                            Customer_name = b.Customer_Name,
                            Lisense_code = b.Lisense_Code,
                            Status = a.Status,
                            Createdate = a.Createdate,
                            Editdate = b.Editdate,
                            Chanel = a.Chanel,
                            Message = a.Message,
                            MO_ID = a.MO_ID,
                            Product = a.Product,
                            Mo = b.Message,
                            MOProduct = b.Product
                        };
            
            
            // khi xem chi tiet khach hang
            
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Phone.Contains(searchstring) || a.Mo.Contains(searchstring));
                ViewBag.searchstring = searchstring;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(s => s.Createdate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                d = d.AddDays(1);
                model = model.Where(s => s.Createdate <= d);
                ViewBag.to_date = to_date;
            }
            if (!string.IsNullOrEmpty(status))
            {
                model = model.Where(a => a.Status.ToString() == status);
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(product))
            {
                model = model.Where(a => a.Product.ToString() == product);
                ViewBag.product = product;
            }
            if (!string.IsNullOrEmpty(chanel))
            {
                model = model.Where(a => a.Chanel.ToString() == chanel);
                ViewBag.chanel = chanel;
            }

            url = Request.Url.AbsoluteUri;

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderByDescending(a => a.Createdate).ToPagedList(pageNumber, pageSize));
        }
        [HttpPost]
        public ActionResult EditCustomer(int? Id)
        {            
            var customer1 = db.LogMOes.FirstOrDefault(a => a.ID == Id);
            return PartialView("~/Areas/Admin/Views/LogMo/_EditCustomer.cshtml", customer1);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomers([Bind(Include = "")] LogMO Log)
        {
            if (ModelState.IsValid)
            {
                var _log = db.LogMOes.FirstOrDefault(a => a.ID == Log.ID);
                _log.Customer_Name = Log.Customer_Name;
                _log.Lisense_Code = Log.Lisense_Code;
                _log.Product = Log.Product;
                db.Entry(_log).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Redirect(url);
            }
            return Redirect(url);

        }
        [HttpPost]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LogMO logmo = db.LogMOes.Find(id);
            if (logmo == null)
            {
                return HttpNotFound();
            }
            return View(logmo);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] LogMO logmo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(logmo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(logmo);

        }



        public void Khachhang()
        {
            var model = from a in db.LogMTs
                        join b in db.LogMOes on a.MO_ID equals b.ID
                        select new LogMoMt()
                        {
                            ID = a.ID,
                            Phone = a.Phone,
                            Code = b.Code,
                            Customer_name = b.Customer_Name,
                            Lisense_code = b.Lisense_Code,
                            Status = a.Status,
                            Createdate = a.Createdate,
                            Editdate = b.Editdate,
                            Chanel = a.Chanel,
                            Message = a.Message,
                            MO_ID = a.MO_ID,
                            Product = a.Product,
                            Mo = b.Message,
                            MOProduct = b.Product
                        };

            IEnumerable<LogMoMt> listexc = model;
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "stt";
            Sheet.Cells["B1"].Value = "họ tên";
            Sheet.Cells["C1"].Value = "số điện thoại";
            Sheet.Cells["D1"].Value = "code";
            Sheet.Cells["E1"].Value = "biển số";
            Sheet.Cells["F1"].Value = "channel";
            Sheet.Cells["G1"].Value = "tin nhắn";
            Sheet.Cells["H1"].Value = "ngày tạo";
            Sheet.Cells["I1"].Value = "MO_ID";
            Sheet.Cells["J1"].Value = "sản phẩm";
            Sheet.Cells["K1"].Value = "trạng thái";
            

            int index = 1;
            int row = 2;
            foreach (var item in listexc)
            {

                Sheet.Cells[string.Format("A{0}", row)].Value = index++;
                Sheet.Cells[string.Format("B{0}", row)].Value = item.Customer_name;
                Sheet.Cells[string.Format("C{0}", row)].Value = item.Phone;
                Sheet.Cells[string.Format("D{0}", row)].Value = "TMAS"+item.Code;
                Sheet.Cells[string.Format("E{0}", row)].Value = item.Lisense_code;
                Sheet.Cells[string.Format("F{0}", row)].Value = item.Chanel;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.Message;
                Sheet.Cells[string.Format("H{0}", row)].Value = (item.Createdate != null) ? item.Createdate.Value.ToString("dd/MM/yyyy") : "";
                Sheet.Cells[string.Format("I{0}", row)].Value = item.MO_ID;
                Sheet.Cells[string.Format("J{0}", row)].Value = item.MOProduct;
                Sheet.Cells[string.Format("K{0}", row)].Value = item.Status;
           
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
    }
}
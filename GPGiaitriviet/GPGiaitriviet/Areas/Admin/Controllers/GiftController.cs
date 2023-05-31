using GPGiaitriviet.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class GiftController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        private static List<GiftModels> listExc = new List<GiftModels>();
        static string url;
        public ActionResult Index(string searchstring, string cr_search, int? page, string from_date, string to_date, string status,
            string product, string callby)
        {

            var model = from a in db.GiftExchanges
                        join b in db.Customers on a.Phone equals b.Phone
                        select new GiftModels()
                        {
                            ID = a.ID,
                            Phone = a.Phone,
                            Count = a.Count,
                            Product = a.Product,
                            Createdate = a.Createdate,
                            Successdate = a.Successdate,
                            Callby = b.Callby,
                            Status = a.Status,
                            Note = a.Note,
                            Name = b.Name,
                            Address = b.Address,
                            Birthday = b.Birthday,
                            Sex = b.Sex,
                            HHTD = b.HHTD,
                            KTCD = b.KTCD,
                            SUPERMAN = b.SUPERMAN,
                            VVG = b.VVG
                        };
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Phone.Contains(searchstring));
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
            if (!string.IsNullOrEmpty(callby))
            {
                model = model.Where(a => a.Callby.Equals(callby));
                ViewBag.callby = callby;
            }

            if (!(User.IsInRole("Admin") || User.IsInRole("Leader")))
            {
                string u = User.Identity.Name;
                model = model.Where(a => a.Callby.Equals(u));
            }

            // Lấy danh sách TeleSale
            var tele = from a in db.AspNetUsers
                       from b in a.AspNetRoles
                       where b.Id == "Telesale"
                       select a;
            TempData["tele"] = tele.ToList();
            //
            url = Request.Url.AbsoluteUri;

            // IEnumerable<GiftModels> result = model as IEnumerable<GiftModels>;
            model = model.OrderByDescending(a => a.Createdate);
            listExc = model.ToList();
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(listExc.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult AddTele(long[] productIDs, string tele)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(tele))
                {
                    msg = "Vui lòng chọn nhân viên phụ trách.";
                }
                else
                {
                    int index = 0;
                    foreach (var item in productIDs)
                    {
                        index++;
                        var gift = db.GiftExchanges.Find(item);
                        gift.Status = 0;
                        gift.Callby = tele;
                        db.Entry(gift).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    msg = "Đã chuyển thành công " + index + " phiếu đổi quà đến " + tele;
                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(msg);

        }
        [HttpPost]
        public ActionResult Edit(int studentId)
        {
            var gift = db.GiftExchanges.Find(studentId);
            return PartialView("~/Areas/Admin/Views/Gift/_Edit.cshtml", gift);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edits([Bind(Include = "")] GiftExchange giftExchange)
        {
            if (ModelState.IsValid)
            {

                var doiqua = db.GiftExchanges.Find(giftExchange.ID);
                // Đang ở trạng thái Đã gửi quà, Đã liên hệ, Chưa liên hệ khi chuyển sang trạng thái Đã nhận quà thì trừ điểm
                if ((doiqua.Status < 3) && (giftExchange.Status == 3))
                {
                    // Cập nhật điểm trong hệ thống Customers
                    var cutomer = db.Customers.SingleOrDefault(a => a.Phone == giftExchange.Phone);
                    if (giftExchange.Product == "HHTD")
                    {
                        cutomer.HHTD = cutomer.HHTD - 6;
                    }
                    else if (giftExchange.Product == "KTCD")
                    {
                        cutomer.KTCD = cutomer.KTCD - 6;
                    }
                    else if (giftExchange.Product == "SUPERMAN")
                    {
                        cutomer.SUPERMAN = cutomer.SUPERMAN - 6;
                    }
                    else if (giftExchange.Product == "VVG")
                    {
                        cutomer.VVG = cutomer.VVG - 6;
                    }
                    db.Entry(cutomer).State = EntityState.Modified;
                }
                else if ((doiqua.Status == 3) && (giftExchange.Status < 3))
                {
                    // Cộng điểm
                    var cutomer = db.Customers.SingleOrDefault(a => a.Phone == giftExchange.Phone);
                    if (giftExchange.Product == "HHTD")
                    {
                        cutomer.HHTD = cutomer.HHTD + 6;
                    }
                    else if (giftExchange.Product == "KTCD")
                    {
                        cutomer.KTCD = cutomer.KTCD + 6;
                    }
                    else if (giftExchange.Product == "SUPERMAN")
                    {
                        cutomer.SUPERMAN = cutomer.SUPERMAN + 6;
                    }
                    else if (giftExchange.Product == "VVG")
                    {
                        cutomer.VVG = cutomer.VVG + 6;
                    }
                    db.Entry(cutomer).State = EntityState.Modified;
                }
                doiqua.Status = giftExchange.Status;
                doiqua.Successdate = DateTime.Now;
                // doiqua.Callby = 
                doiqua.Note = giftExchange.Note;
                db.Entry(doiqua).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(url);
            }
            return Redirect(url);
        }
        // ADD BY TRUNGVD 2021-06-26 Task = 613631
        public FileResult ExportExcel()
        {
            var stream = CreateExcelFileDetail(listExc);
            var buffer = stream as MemoryStream;
            return File(buffer.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GP.giftexchange_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx");
        }
        private Stream CreateExcelFileDetail(List<GiftModels> list, Stream stream = null)
        {
            // ExcelPackage.LicenseContext = LicenseContext.Commercial;
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {
                excelPackage.Workbook.Properties.Author = "GP";
                excelPackage.Workbook.Properties.Company = "DUOCBANHAT";
                excelPackage.Workbook.Properties.Title = "Gift";
                excelPackage.Workbook.Worksheets.Add("Gift");
                var workSheet = excelPackage.Workbook.Worksheets[0];
                //workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                BindingFormatForExcelDetail(workSheet, list);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatForExcelDetail(ExcelWorksheet worksheet, List<GiftModels> listItems)
        {
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 25;
            // Tự động xuống hàng khi text quá dài
            worksheet.Cells.Style.WrapText = false;
            // Tạo header
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 1].AutoFitColumns(6);
            worksheet.Cells[1, 2].Value = "Số điện thoại";
            worksheet.Cells[1, 2].AutoFitColumns(11);
            worksheet.Cells[1, 3].Value = "Tên khách hàng";
            worksheet.Cells[1, 3].AutoFitColumns();
            worksheet.Cells[1, 4].Value = "Ngày sinh";
            worksheet.Cells[1, 4].AutoFitColumns(11);
            worksheet.Cells[1, 5].Value = "Giới tính";
            worksheet.Cells[1, 5].AutoFitColumns(6);
            worksheet.Cells[1, 6].Value = "Địa chỉ";
            worksheet.Cells[1, 6].AutoFitColumns();
            worksheet.Cells[1, 7].Value = "Sản phẩm";
            worksheet.Cells[1, 7].AutoFitColumns(11);
            worksheet.Cells[1, 8].Value = "Số điểm";
            worksheet.Cells[1, 8].AutoFitColumns(11);
            worksheet.Cells[1, 9].Value = "Lần đổi";
            worksheet.Cells[1, 9].AutoFitColumns(6);
            worksheet.Cells[1, 10].Value = "Ngày tạo";
            worksheet.Cells[1, 10].AutoFitColumns(11);
            worksheet.Cells[1, 11].Value = "Trạng thái";
            worksheet.Cells[1, 11].AutoFitColumns(15);
            worksheet.Cells[1, 12].Value = "Ngày cập nhật";
            worksheet.Cells[1, 12].AutoFitColumns(11);
            worksheet.Cells[1, 13].Value = "Phụ trách";
            worksheet.Cells[1, 13].AutoFitColumns(20);
            worksheet.Cells[1, 14].Value = "Ghi chú";
            worksheet.Cells[1, 14].AutoFitColumns();



            // Lấy range vào tạo format cho range đó ở đây là từ A1 tới I1
            using (var range = worksheet.Cells["A1:N1"])
            {
                // Set PatternType
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // Set Màu cho Background
                // range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0xc6d9f1));
                range.Style.Fill.BackgroundColor.SetColor(Color.Black);
                // Canh giữa cho các text
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //// Set Font cho text  trong Range hiện tại
                //range.Style.Font.SetFromFont(new Font("Arial", 10));
                //// Set Border
                //range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                //// Set màu cho Border
                //range.Style.Border.Bottom.Color.SetColor(Color.DarkBlue);

                //range.AutoFilter = true;
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 10;
                range.Style.Font.Color.SetColor(Color.White);
            }

            // Đỗ dữ liệu từ list vào 
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                var rowCur = i + 2;
                string headerRange = "A" + rowCur + ":N" + rowCur;
                worksheet.Cells[headerRange].Style.Font.Bold = false;
                worksheet.Cells[headerRange].Style.Font.Size = 11;
                worksheet.Cells[headerRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells["A" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["H" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowCur, 1].Value = i + 1;
                worksheet.Cells[rowCur, 2].Value = item.Phone;
                worksheet.Cells[rowCur, 3].Value = item.Name;
                worksheet.Cells[rowCur, 4].Value = item.Birthday;
                worksheet.Cells[rowCur, 5].Value = item.Sex;
                worksheet.Cells[rowCur, 6].Value = item.Address;
                worksheet.Cells[rowCur, 7].Value = item.Product;

                int? Point = 0;
                switch (item.Product)
                {
                    case "HHTD":
                        Point = item.HHTD;
                        break;
                    case "KTCD":
                        Point = item.KTCD;
                        break;
                    case "SUPERMAN":
                        Point = item.SUPERMAN;
                        break;
                    case "VVG":
                        Point = item.VVG;
                        break;
                }

                worksheet.Cells[rowCur, 8].Value = Point;
                worksheet.Cells[rowCur, 9].Value = item.Count;
                if (item.Createdate != null)
                {
                    worksheet.Cells[rowCur, 10].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 10].Value = item.Createdate.Value.ToString("dd/MM/yyyy");
                }
                string Status = "";
                switch (item.Status)
                {
                    case 0:
                        Status = "Chưa liên hệ";
                        break;
                    case 1:
                        Status = "Đã liên hệ";
                        break;
                    case 2:
                        Status = "Đã gửi hệ";
                        break;
                    case 3:
                        Status = "Đã nhận quà";
                        break;
                }

                worksheet.Cells[rowCur, 11].Value = Status;

                if (item.Successdate != null)
                {
                    worksheet.Cells[rowCur, 12].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 12].Value = item.Successdate.Value.ToString("dd/MM/yyyy");
                }
                worksheet.Cells[rowCur, 13].Value = item.Callby;
                worksheet.Cells[rowCur, 14].Value = item.Note;

            }
        }
    }
}
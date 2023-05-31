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
    public class CustomerController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        private static List<Customer> listExc = new List<Customer>();
        public ActionResult Index(string searchstring, int? page, string from_date, string sex, string from_year, string to_year, string callby, string from_calldate)
        {
            var model = from a in db.Customers
                        select a;
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Phone.Contains(searchstring) || a.Name.Contains(searchstring) || a.Address.Contains(searchstring) || a.Ward.Contains(searchstring) || a.District.Contains(searchstring));
                ViewBag.searchstring = searchstring;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(s => s.Editdate >= d);
                DateTime dAdd = d.AddDays(1);
                model = model.Where(s => s.Editdate <= dAdd);
                ViewBag.from_date = from_date;
            }
            //if (!string.IsNullOrEmpty(to_date))
            //{
            //    DateTime d = DateTime.Parse(to_date);
            //    d = d.AddDays(1);
            //    model = model.Where(s => s.Createdate <= d);
            //    ViewBag.to_date = to_date;
            //}

            if (!string.IsNullOrEmpty(from_calldate))
            {
                DateTime d = DateTime.Parse(from_calldate);
                model = model.Where(s => s.Calldate >= d);
                DateTime dAdd = d.AddDays(1);
                model = model.Where(s => s.Calldate <= dAdd);
                ViewBag.from_calldate = from_calldate;
            }
            //if (!string.IsNullOrEmpty(to_calldate))
            //{
            //    DateTime d = DateTime.Parse(to_calldate);
            //    d = d.AddDays(1);
            //    model = model.Where(s => s.Calldate <= d);
            //    ViewBag.to_calldate = to_calldate;
            //}

            if (!string.IsNullOrEmpty(from_year))
            {
                int iFromYear = int.Parse(from_year);
                model = model.Where(s => s.YearOfBorn >= iFromYear);
                ViewBag.from_year = from_year;
            }

            if (!string.IsNullOrEmpty(to_year))
            {
                int iToYear = int.Parse(to_year);
                model = model.Where(s => s.YearOfBorn <= iToYear);
                ViewBag.to_year = to_year;
            }

            if (!string.IsNullOrEmpty(sex))
            {
                model = model.Where(s => s.Sex.Equals(sex));
                ViewBag.sex = sex;
            }

            if (!string.IsNullOrEmpty(callby))
            {
                if (callby.StartsWith("--Không")) // Không ai phụ trách
                {
                    model = model.Where(s => s.Callby == null);
                }
                else
                {
                    model = model.Where(s => s.Callby.Equals(callby));
                }
                ViewBag.callby = callby;
            }

            //// var list = tele.ToList();
            //AspNetUser u2 = new AspNetUser()
            //{
            //    UserName = User.Identity.Name;
            //};

            // var tele;
            
            // string u = User.Identity.Name;
            // string Role = User.IsInRole
            var tele = from a in db.AspNetUsers
                       from b in a.AspNetRoles
                       where b.Id == "Telesale"
                       select a;

            if (!(User.IsInRole("Admin") || User.IsInRole("Leader")))
            {
                string u = User.Identity.Name;
                model = model.Where(s => s.Callby.Equals(u));
              
            }
           

            // var list = tele.ToList();
            AspNetUser u1 = new AspNetUser()
            {
                UserName = "--Không có phụ trách--"
            };
            // list = list.Add(u);
            // tele.Append(u1);
            var tele1 = tele.ToList();
            // Thêm phần không có người phụ trách
            tele1.Add(u1);
            // Sắp xếp các thông tin 
            model = model.OrderByDescending(a => a.Createdate);
            listExc = model.ToList();
            // Tele1 dùng để Search
            TempData["tele1"] = tele1.ToList();

            // Tele dùng để Chuyển
            TempData["tele"] = tele.ToList();


            // IEnumerable<Customer> result = model as IEnumerable<Customer>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            // return View(result.ToPagedList(pageNumber, pageSize));
            return View(listExc.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Leader")]
        public ActionResult AddTele(string[] customer, string tele)
        {
            string msg = "";
            try
            {
                if (customer == null)
                {
                    msg = "Vui lòng chọn khách hàng!";
                }
                else if (string.IsNullOrEmpty(tele) || tele.Contains("--"))
                {
                    msg = "Vui lòng chọn nhân viên phụ trách.";
                }
                else
                {
                    int index = 0;
                    foreach (var item in customer)
                    {
                        index++;
                        var customer1 = db.Customers.Where(x => x.Phone == item).SingleOrDefault();
                        // customer.Status = 0;
                        customer1.Callby = tele;
                        db.Entry(customer1).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    msg = "Đã chuyển thành công " + index + " khách hàng cho " + tele + " phụ trách!";
                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(msg);

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Leader")]
        public ActionResult Change(string tele, string target)
        {
            string msg = "";
            try
            {
                if (string.IsNullOrEmpty(tele) || tele.Contains("--"))
                {
                    msg = "Vui lòng chọn nhân viên phụ trách đang phụ trách";
                }
                else if (string.IsNullOrEmpty(target) || target.Contains("--"))
                {
                    msg = "Vui lòng chọn nhân viên phụ trách cần chuyển";
                }
                else if (target.Equals(tele))
                {
                    msg = "Vui lòng chọn nhân viên phụ trách mới khác nhân viên phụ trách cũ!";
                }
                else
                {
                    //int index = 0;
                    //foreach (var item in customer)
                    //{
                    //    index++;
                    //    var customer1 = db.Customers.Where(x => x.Phone == item).SingleOrDefault();
                    //    // customer.Status = 0;
                    //    customer1.Callby = tele;
                    //    db.Entry(customer1).State = EntityState.Modified;
                    //}
                    //db.SaveChanges();
                    var customer1 = db.Customers.Where(x => x.Callby == tele);
                    if (customer1.Count() > 0)
                    {
                        int i = 0;
                        foreach (var item in customer1)
                        {
                            item.Callby = target;
                            db.Entry(item).State = EntityState.Modified;
                            i++;
                        }
                        db.SaveChanges();
                        msg = "Đã chuyển thành công " + i + " khách hàng của nhân viên " + tele + " cho nhân viên " + target + "!";
                    }
                    else
                    {
                        msg = "Không có khách hàng của nhân viên " + tele + " phụ trách";
                    }

                    // msg = "Đã chuyển thành công toàn bộ khách hàng của nhân viên <p style=\"background - color:tomato; \">" + tele + "</p> cho nhân viên <p style=\"background - color:blue; \">" + target + "!";
                }

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(msg);

        }

        // ADD BY TRUNGVD 2021-06-26 Task = 613631
        public FileResult ExportExcel()
        {
            var stream = CreateExcelFileDetail(listExc);
            var buffer = stream as MemoryStream;
            return File(buffer.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "GP.customer_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx");
        }
        private Stream CreateExcelFileDetail(List<Customer> list, Stream stream = null)
        {
            // ExcelPackage.LicenseContext = LicenseContext.Commercial;

            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {
                excelPackage.Workbook.Properties.Author = "GP";
                excelPackage.Workbook.Properties.Company = "DUOCBANHAT";
                excelPackage.Workbook.Properties.Title = "Customer";
                excelPackage.Workbook.Worksheets.Add("Customer");
                var workSheet = excelPackage.Workbook.Worksheets[0];
                //workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                BindingFormatForExcelDetail(workSheet, list);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatForExcelDetail(ExcelWorksheet worksheet, List<Customer> listItems)
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
            worksheet.Cells[1, 7].Value = "Ngày tạo";
            worksheet.Cells[1, 7].AutoFitColumns(11);
            worksheet.Cells[1, 8].Value = "Ngày cập nhật";
            worksheet.Cells[1, 8].AutoFitColumns(11);
            worksheet.Cells[1, 9].Value = "Điểm HHTD";
            worksheet.Cells[1, 9].AutoFitColumns(6);
            worksheet.Cells[1, 10].Value = "Điểm KTCD";
            worksheet.Cells[1, 10].AutoFitColumns(6);
            worksheet.Cells[1, 11].Value = "Điểm Superman";
            worksheet.Cells[1, 11].AutoFitColumns(6);
            worksheet.Cells[1, 12].Value = "Phụ trách";
            worksheet.Cells[1, 12].AutoFitColumns(16);
            worksheet.Cells[1, 13].Value = "Ngày liên hệ";
            worksheet.Cells[1, 13].AutoFitColumns(11);
            worksheet.Cells[1, 14].Value = "Ghi chú";
            worksheet.Cells[1, 14].AutoFitColumns();
            //worksheet.Cells[1, 14].AutoFitColumns(6);
            //worksheet.Cells[1, 15].Value = "Phân loại";
            //worksheet.Cells[1, 15].AutoFitColumns(6);
            //worksheet.Cells[1, 16].Value = "Kích hoạt";
            //worksheet.Cells[1, 16].AutoFitColumns(11);
            //worksheet.Cells[1, 17].Value = "Ngày kích hoạt";
            //worksheet.Cells[1, 17].AutoFitColumns(11);
            //worksheet.Cells[1, 18].Value = "Số điện thoại";
            //worksheet.Cells[1, 18].AutoFitColumns(11);
            //worksheet.Cells[1, 19].Value = "Địa chỉ";
            //worksheet.Cells[1, 19].AutoFitColumns();
            //worksheet.Cells[1, 20].Value = "Hạn bảo hành(tháng)";
            //worksheet.Cells[1, 20].AutoFitColumns(16);
            //worksheet.Cells[1, 21].Value = "Ngày hết hạn";
            //worksheet.Cells[1, 21].AutoFitColumns(11);
            //worksheet.Cells[1, 22].Value = "Ngày mua hàng";
            //worksheet.Cells[1, 22].AutoFitColumns(11);


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
                worksheet.Cells["E" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["I" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["J" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["K" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //worksheet.Cells["S" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //worksheet.Cells["I" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //worksheet.Cells["J" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //worksheet.Cells["P" + rowCur].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells[rowCur, 1].Value = i + 1;
                worksheet.Cells[rowCur, 2].Value = item.Phone;
                worksheet.Cells[rowCur, 3].Value = item.Name;
                worksheet.Cells[rowCur, 4].Value = item.Birthday;
                worksheet.Cells[rowCur, 5].Value = item.Sex;


                worksheet.Cells[rowCur, 6].Value = item.Address;

                if (item.Createdate != null)
                {
                    worksheet.Cells[rowCur, 7].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 7].Value = item.Createdate.Value.ToString("dd/MM/yyyy");
                }
                if (item.Editdate != null)
                {
                    worksheet.Cells[rowCur, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 8].Value = item.Editdate.Value.ToString("dd/MM/yyyy");
                }
                worksheet.Cells[rowCur, 9].Value = item.HHTD;
                worksheet.Cells[rowCur, 10].Value = item.KTCD;
                worksheet.Cells[rowCur, 11].Value = item.SUPERMAN;


                worksheet.Cells[rowCur, 12].Value = item.Callby;
                if (item.Calldate != null)
                {
                    worksheet.Cells[rowCur, 13].Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Cells[rowCur, 13].Value = item.Calldate.Value.ToString("dd/MM/yyyy");
                }
                worksheet.Cells[rowCur, 14].Value = item.Note;
                //worksheet.Cells[rowCur, 15].Value = item.Rank;
                //worksheet.Cells[rowCur, 16].Value = item.Activeby;
                //if (item.Activedate != null)
                //{
                //    worksheet.Cells[rowCur, 17].Style.Numberformat.Format = "dd/MM/yyyy";
                //    worksheet.Cells[rowCur, 17].Value = item.Activedate.Value.ToString("dd/MM/yyyy");
                //}

                //worksheet.Cells[rowCur, 18].Value = item.CusPhone;
                //worksheet.Cells[rowCur, 19].Value = item.Address;
                //worksheet.Cells[rowCur, 20].Value = item.Limited;
                //if (item.Activedate != null)
                //{
                //    worksheet.Cells[rowCur, 21].Style.Numberformat.Format = "dd/MM/yyyy";
                //    worksheet.Cells[rowCur, 21].Value = item.Activedate.Value.AddMonths(item.Limited ?? default(int)).ToString("dd/MM/yyyy");
                //}
                //if (item.Buydate != null)
                //{
                //    worksheet.Cells[rowCur, 22].Style.Numberformat.Format = "dd/MM/yyyy";
                //    worksheet.Cells[rowCur, 22].Value = item.Buydate.Value.ToString("dd/MM/yyyy");
                //}
            }
        }
    }
}
using GPGiaitriviet.Models;
using NLog;
using OfficeOpenXml;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class CodeGPController : Controller
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        GiaitrivietEntities db = new GiaitrivietEntities();
        public ActionResult Index(string searchstring, int? page, string from_date, string to_date, string status,
            string product)
        {
            var model = from a in db.CodeGPs
                       select a;
            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchstring))
            {
                model = model.Where(a => a.Code.Contains(searchstring) || a.Serial.Contains(searchstring));
                ViewBag.searchstring = searchstring;
            }
            if (!string.IsNullOrEmpty(from_date))
            {
                // ViewBag.from_date = from_date;
                DateTime d = DateTime.Parse(from_date);
                model = model.Where(s => s.Activedate >= d);
                ViewBag.from_date = from_date;
            }
            if (!string.IsNullOrEmpty(to_date))
            {
                DateTime d = DateTime.Parse(to_date);
                d = d.AddDays(1);
                model = model.Where(s => s.Activedate <= d);
                ViewBag.to_date = to_date;
            }
            if (!string.IsNullOrEmpty(status))
            {
                if ("1".Equals(status))
                {
                    model = model.Where(a => a.Status.ToString() == status);
                } else
                {
                    model = model.Where(a => a.Status == null);
                }
                ViewBag.status = status;
            }
            if (!string.IsNullOrEmpty(product))
            {
                model = model.Where(a => a.Category.ToString() == product);
                ViewBag.product = product;
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(model.OrderBy(a => a.Code).ToPagedList(pageNumber, pageSize));
        }
        // GET: Admin/Article/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CodeGP code = db.CodeGPs.SingleOrDefault(a => a.ID == id);
            if (code == null)
            {
                return HttpNotFound();
            }
            return View(code);
        }

        // POST: Admin/Article/Edit/5
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "")] CodeGP code)
        {
            try
            {
                // TODO: Add update logic here
                db.Entry(code).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View(code);
            }
        }

        // GET: Admin/Article/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CodeGP code = db.CodeGPs.SingleOrDefault(a => a.ID == id);
            if (code == null)
            {
                return HttpNotFound();
            }
            return View(code);
        }

        // POST: Admin/Article/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long? id)
        {
            CodeGP code = db.CodeGPs.Find(id);
            try
            {
                db.CodeGPs.Remove(code);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View(code);
            }
        }

        public ActionResult UploadFile()
        {
            List<CodeGP> list_code = new List<CodeGP>();
            return View(list_code);
        }
        [HttpPost]
        public ActionResult UploadFile(FormCollection collection)
        {
            List<CodeGP> list_code = new List<CodeGP>();
            try
            {
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                        { 
                            string code;
                            string serial;

                            
                            try { code = workSheet.Cells[rowIterator, 3].Value.ToString(); } catch (Exception) { code = ""; }
                            try { serial = workSheet.Cells[rowIterator, 2].Value.ToString(); } catch (Exception) { serial = ""; }


                            var codes = new CodeGP()
                            {
                                Code = code,
                                Serial = serial,

                            };
                            if (!string.IsNullOrEmpty(code))
                            {
                                var  _code= db.CodeGPs.Where(a => a.Code == code);
                                if (_code.Count() == 0)
                                {
                                    codes.Createdate = DateTime.Now;
                                    db.CodeGPs.Add(codes);
                                    db.SaveChanges();
                                }

                            }
                            list_code.Add(codes);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Error(ex.InnerException);
            }
            return View(list_code);

        }

    }
}
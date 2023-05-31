using GPGiaitriviet.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Areas.Admin.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        GiaitrivietEntities db = new GiaitrivietEntities();
        static string url;
        // ApplicationSignInManager _signInManager;
        ApplicationUserManager _userManager;
        ApplicationDbContext context = new ApplicationDbContext();
        public ActionResult Index(string SearchString, int? page)
        {
            ViewBag.DefaultDescription = "";

            var model = from a in db.AspNetUsers
                        select a;
            if (!string.IsNullOrEmpty(SearchString))
            {
                model = model.Where(a => a.UserName.Contains(SearchString));
                ViewBag.SearchString = SearchString;
            }
            url = Request.Url.AbsoluteUri;

            IEnumerable<AspNetUser> result = model as IEnumerable<AspNetUser>;
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(result.OrderBy(a => a.UserName).ToPagedList(pageNumber, pageSize));
        }
        //// GET: Admin/Article/Edit/5
        //public ActionResult Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    AspNetUser user = db.AspNetUsers.Find(id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(user);
        //}

        //// POST: Admin/Article/Edit/5
        //[HttpPost, ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "")] AspNetUser user)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here
        //        db.Entry(user).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View(user);
        //    }
        //}


        // ADD BY TRUNGVD 2021-06-16 - GPVIET Task = 443569
        [HttpPost]
        public ActionResult Register()
        {
            TempData["role"] = db.AspNetRoles.ToList();
            // return View();
            RegisterViewModel model = new RegisterViewModel();
            return PartialView("~/Areas/Admin/Views/User/_Register.cshtml", model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registers(RegisterViewModel model)
        {
            //AccountController accountController = new AccountController();
            //return await accountController.Register(model);
            if (ModelState.IsValid)
            {
                // Check trạng thái
                if (model.UserName != null && model.Password != null && model.ConfirmPassword != null && model.Role != null && model.Password.Equals(model.ConfirmPassword))
                {
                    var user = new ApplicationUser { UserName = model.UserName };
                    _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        ApplicationUser u = context.Users.Find(user.Id);
                        u.Roles.Add(new IdentityUserRole() { UserId = user.Id, RoleId = model.Role });
                        context.SaveChanges();
                        //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                        // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        //  return Redirect(url);
                    }
                }
                // return Redirect(url);
            }
            TempData["role"] = db.AspNetRoles.ToList();
            return Redirect(url);
            // If we got this far, something failed, redisplay form
            // return View(model);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Registers([Bind(Include = "")] AspNetUser user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user1 = db.AspNetUsers.Find(user.Id);

        //        user1.PhoneNumber = user.PhoneNumber;
        //        user1.Email = user.Email;

        //        // Chưa làm phần này, để sau
        //        // user1.PasswordHash = user.PasswordHash;
        //        // user1.PhoneNumber = user.PhoneNumber;

        //        db.Entry(user1).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return Redirect(url);
        //    }
        //    return Redirect(url);
        //}
        // ADD BY TRUNGVD 2021-06-16 - GPVIET Task = 443569

        // ADD BY TRUNGVD 2021-06-16 - GPVIET Task = 443569
        [HttpPost]
        public ActionResult Edit(string studentId)
        {
            var user = db.AspNetUsers.Find(studentId);
            return PartialView("~/Areas/Admin/Views/User/_Edit.cshtml", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edits([Bind(Include = "")] AspNetUser user)
        {
            if (ModelState.IsValid)
            {
                var user1 = db.AspNetUsers.Find(user.Id);

                user1.PhoneNumber = user.PhoneNumber;
                user1.Email = user.Email;



                // Chưa làm phần này, để sau
                // user1.PasswordHash = user.PasswordHash;
                // user1.PhoneNumber = user.PhoneNumber;
                // Đổi pass
                if (user.PasswordHash != null && !"".Equals(user.PasswordHash))
                {
                    _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ApplicationUser user2 = await _userManager.FindByIdAsync(user.Id);

                    user2.PasswordHash = _userManager.PasswordHasher.HashPassword(user.PasswordHash);
                    var result = await _userManager.UpdateAsync(user2);

                    if (result.Succeeded)
                    {
                        //throw exception......
                        // db.Entry(user1).State = EntityState.Modified;
                        // db.SaveChanges();
                        user1.PasswordHash = user2.PasswordHash;
                        db.Entry(user1).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                } else
                {
                    db.Entry(user1).State = EntityState.Modified;
                    db.SaveChanges();
                }
                // Nếu không muốn thay đổi password
               
                //return Ok();
                //var token = await _userManager.GeneratePasswordResetTokenAsync(user2);

                //var result = await _userManager.ResetPasswordAsync(user2, token, user.PasswordHash);
                return Redirect(url);
            }
            return Redirect(url);
        }
        // ADD BY TRUNGVD 2021-06-16 - GPVIET Task = 443569

        [HttpPost]     
        public ActionResult Delete(string Id)
        {
            string msg = "";
            if (Id == null)
            {
                // return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                msg = "Không tồn tại tài khoản cần xóa";
            }
            AspNetUser user = db.AspNetUsers.Find(Id);
            if (user == null)
            {
                // return HttpNotFound();
                msg = "Không tồn tại tài khoản cần xóa";
            }
            else
            {
                // Thực hiện lệnh xóa
                // Check xem trong Customer có ai phụ trách không
                var customer = db.Customers.Where(x => x.Callby == user.UserName);
                int i = customer.Count();
                if (customer.Count() == 0)
                {
                    db.AspNetUsers.Remove(user);
                    db.SaveChanges();
                    msg = "Xóa thành công!";
                    // return Redirect(url);
                } else
                {
                    msg = "Tài khoản này đang phụ trách " + i + " khách hàng nên không thể xóa.";
                }

            }
            return Json(msg);
            // return Redirect(url);
        }

        //// POST: Admin/Article/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string id)
        //{
        //    AspNetUser user = db.AspNetUsers.Find(id);
        //    try
        //    {
        //        db.AspNetUsers.Remove(user);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View(user);
        //    }
        //}
    }
}
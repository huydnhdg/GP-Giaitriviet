using GPGiaitriviet.Models;
using GPGiaitriviet.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GPGiaitriviet.Controllers
{
    public class HomeController : Controller
    {
        string LINK_API = ConfigurationManager.AppSettings["LINK_API"];
        string SENDER_ID = ConfigurationManager.AppSettings["SENDER_ID"];
        public ActionResult Index()
        {
            return RedirectToAction("Index", new { controller = "Home", area = "Admin" });
        }

        [HttpPost]
        public JsonResult CallKichHoat(string phone1, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(phone1))//không nhập đủ dữ liệu
                {
                    return ReturnResult("Chưa gửi được mã dự thưởng. Vui lòng kiểm tra các thông tin và thực hiện gửi lại.");
                }

                code = code.ToUpper();


                phone1 = MyControl.formatUserId(phone1, 0);
                int phone1_length = phone1.Length;
                if (phone1_length != 11)//kiem tra so dien thoai co hop le hay k 84965433459
                {
                    //checking code
                    if (!ValidateCode(code))
                    {
                        return ReturnResult("Mã dự thưởng không đúng. Vui lòng nhập lại.", "code");
                    }
                    else
                    {
                        return ReturnResult("Số thuê bao không đúng định dạng, Quý khách vui lòng kiểm tra lại.");
                    }
                }

                //checking code
                if (!ValidateCode(code))
                {
                    return ReturnResult("Mã dự thưởng không đúng. Vui lòng nhập lại.", "code");
                }

                //Kich hoat                
                Result jsonResultend = CallAPI(phone1, code, "TICHDIEM");
                if (jsonResultend.status == "3")//request thanh cong trung thuong the cao
                {
                    return ReturnResult("3");
                }
                else//request thanh cong
                {
                    return ReturnResult(jsonResultend.message);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = "false",
                    error = "system",
                    message = e.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CallTraCuu(string phone1, string product)
        {
            try
            {
                // if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(phone1))//không nhập đủ dữ liệu
                if (string.IsNullOrEmpty(phone1))//không nhập đủ dữ liệu
                {
                    return ReturnResult("Vui lòng kiểm tra các thông tin và thực hiện tra cứu lại.");
                }

                phone1 = MyControl.formatUserId(phone1, 0);
                int phone1_length = phone1.Length;
                if (phone1_length != 11)//kiem tra so dien thoai co hop le hay k 84965433459
                {
                    return ReturnResult("Số thuê bao không đúng định dạng, Quý khách vui lòng kiểm tra lại.");
                }

                //Kich hoat                
                Result jsonResultend = CallAPI(phone1, "", "TC");
                if (jsonResultend.status == "3")//request thanh cong trung thuong the cao
                {
                    return ReturnResult("3");
                }
                else//request thanh cong
                {
                    //return Json(new
                    //{
                    //    success = true,
                    //    point = jsonResultend.Point,
                    //    message = jsonResultend.message,
                    //}, JsonRequestBehavior.AllowGet);

                    return ReturnResult(jsonResultend.message);
                }
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = "false",
                    error = "system",
                    message = e.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private Boolean ValidateCode(String code)
        {
            //checking code
            var isDigitPresent = code.Any(c => char.IsDigit(c));
            var isLetterPresent = code.Any(c => char.IsLetter(c));
            //if (code.Length != 10 || !isDigitPresent || !isLetterPresent)
            if (code.Length != 9)
            {
                return false;
            }

            return true;
        }



        private Result CallAPI(string Phone, string Code, string Type)
        {
            // String url = "http://quatang.gpfrance.vn/api/sms/receive?Command_Code=GP&Service_ID=8077&Request_ID=WEB" + DateTime.Now.Ticks +"&User_ID=";
            String url = LINK_API + "Command_Code=GP&Service_ID=8077&Request_ID=WEB" + DateTime.Now.Ticks + "&User_ID=";
            if ("TC".Equals(Type))
            {
                Code = "TC";
            }
            WebRequest request = WebRequest.Create(
                url + Phone + "&Message=GP " + Code);


            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Result result = JsonConvert.DeserializeObject<Result>(responseFromServer);
            reader.Close();
            response.Close();
            return result;
        }

        private JsonResult ReturnResult(string message, string type = "system")
        {
            return Json(new
            {
                success = true,
                type = type,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
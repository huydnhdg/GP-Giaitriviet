using GPGiaitriviet.Models;
using GPGiaitriviet.Utils;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml.ConditionalFormatting;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace GP_Giaitriviet.API
{
    [RoutePrefix("api/sms")]
    public class SmsProviderController : ApiController
    {
        Logger log = LogManager.GetCurrentClassLogger();
        GiaitrivietEntities db = new GiaitrivietEntities();
        private readonly int NUMBER_OVER_QUOTA = 36;
        private readonly int NUMBER_OVER_QUOTA_IN_MONTH = 6;

        [Route("receive")]
        [HttpGet]
        public HttpResponseMessage Receive(string Command_Code, string User_ID, string Service_ID, string Request_ID, string Message)
        {
            //https://localhost:44314/api/sms/receive?command_code=tmas&user_id=84965433459&request_id=xx&service_id=xx&message=tmas%20xxxx%20Nguyenvana%20123%20123
            // Lưu log file

            log.Info(string.Format("[MO] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, Message));
            User_ID = MyControl.formatUserId(User_ID, 0);
            //tách tin nhắn của khách hàng nhắn lên
            string[] subs = Message.Split(' ');

            string mt_trakhachhang = "";
            string category = "TC";
            string chanel = "SMS";
            int status = 0; // 0 - thành công, 1- thất bại, 2 - tra cứu, 3 - sai cú pháp, 4 - trùng mã thẻ, 5 - Quá hạn mức trong 6 tháng, 6- Quá hạn mức trong 1 tháng
            if (subs.Length < 2)
            {
                //nhắn sai cú pháp
                mt_trakhachhang = SmsTemp.SYNTAX_INVALID(chanel);
                category = "SYNTAX_INVALID";
                status = 3;
                var resulter = new Result()
                {
                    status = "0",
                    message = mt_trakhachhang,
                    phone1 = User_ID
                };
                log.Info(string.Format("[MT] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, mt_trakhachhang));
                return ResponseMessage(resulter);
            }
            string CusName = "";
            string Code = "";
            string Product = "";
            string Lisense = "";
            if (subs.Length>=3)
            {
                CusName = subs[2];
            }
            if (subs.Length>=4)
            {
                Product = subs[3];
            }
            if (subs.Length>=5)
            {
                Lisense = subs[4];
            }
            if (subs.Length>=2)
            {
                Code = subs[1];
            }
            var LogMO = new LogMO()
            {
                Command_Code = Command_Code,
                Phone = User_ID,
                Service_ID = Service_ID,
                Request_ID = Request_ID,
                Message = Message,
                Customer_Name = CusName,
                Code = Code,
                Lisense_Code = Lisense,
                Product = Product,
                Createdate = DateTime.Now
            };
            db.LogMOes.Add(LogMO);
            db.SaveChanges();
            //Lưu vào DB
            long id = LogMO.ID;
            // Kết thúc ghi log vào DB
            if (Request_ID.ToUpper().StartsWith("WEB"))
            {
                chanel = "WEB";
            }
            try
            {
                // Nếu gộp thông tin toàn bộ sản phẩm thành Code, đảm bảo khách hàng nhắn tin XXX XXX XXX vẫn OK
                
                log.Info(string.Format("Phone = {0}, Code = {1}", User_ID, Code));
                var codeGP = db.CodeGPs.Where(a => a.Code == Code.Trim()).SingleOrDefault();

                if (codeGP == null)
                {
                    mt_trakhachhang = SmsTemp.CODE_NOTVALID(chanel, Code);
                    status = 1;
                    category = "ERROR_CODE";
                }
                else if (codeGP.Status == null)
                {
                    var check_customer = db.Customers.FirstOrDefault(a => a.Phone == User_ID);
                    if (check_customer != null)
                    {

                    }
                    else
                    {
                        var new_customer = new Customer()
                        {
                            Phone = User_ID,
                            Createdate = DateTime.Now,
                            Name = LogMO.Customer_Name,
                        };
                        db.Customers.Add(new_customer);
                        db.SaveChanges();
                    }
                    mt_trakhachhang = SmsTemp.CODE_SUCCESS(chanel, Code);
                    status = 0;
                    //gạch mã thẻ đi
                    codeGP.Status = 1;
                    codeGP.Activedate = DateTime.Now;
                    codeGP.Phone = User_ID;
                    db.Entry(codeGP).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }
                else
                {
                    string time = codeGP.Activedate.Value.ToString("HH:mm");
                    string date = codeGP.Activedate.Value.ToString("dd/MM/yyyy");
                    mt_trakhachhang = SmsTemp.CODE_USED(chanel, Code, time, date);
                    status = 4;
                }


            }

            catch (Exception ex)
            {
                log.Error(ex.Message);
                mt_trakhachhang = "Co loi ngoai le xay ra";
                status = 3;
            }
            var response = new HttpResponseMessage();
            // Lưu log xử lý vào bảng MT
            var LogMT = new LogMT()
            {
                MO_ID = id,
                Phone = User_ID,
                Createdate = DateTime.Now,
                Message = mt_trakhachhang,
                Product = category,
                Chanel = chanel,
                Status = status
            };
            db.LogMTs.Add(LogMT);
            db.SaveChanges();
            // Kết thúc lưu log MT
            var result = new Result()
            {
                status = "0",
                message = mt_trakhachhang,
                phone1 = User_ID
            };
            log.Info(string.Format("[MT] @Command_Code= {0} @User_ID= {1} @Service_ID= {2} @Request_ID= {3} @Message= {4}", Command_Code, User_ID, Service_ID, Request_ID, mt_trakhachhang));
            return ResponseMessage(result);
        }
        private HttpResponseMessage ResponseMessage(Result result)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
            return response;
        }

    }
}
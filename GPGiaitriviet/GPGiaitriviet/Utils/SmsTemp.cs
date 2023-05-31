using GPGiaitriviet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPGiaitriviet.Utils
{
    public class SmsTemp
    {
        
        public static string SYNTAX_INVALID(string chanel)
        {

            string mtReturn = string.Format("Tin nhan sai cu phap. Yeu cau cua Quy khach khong duoc thuc hien");
           
            return mtReturn;

        }
        public static string CODE_NOTVALID(string chanel,string code)
        {

            string mtReturn = string.Format("Ma du thuong: TMAS{0} khong ton tai. Vui long kiem tra lai, lien he: 0327909399 hoac truy cap: www.trungvang.tmas.vn de biet them chi tiet.", code);

            return mtReturn;

        }
        public static string CODE_USED(string chanel, string code,string time, string date)
        {

            string mtReturn = string.Format("Ma du thuong: TMAS{0} da duoc gui den he thong vao {1} ngay {2}. Vui long kiem tra lai, lien he: 0327909399 hoac truy cap: www.trungvang.tmas.vn",code, time,date);

            return mtReturn;

        }
        public static string CODE_SUCCESS(string chanel, string code)
        {

            string mtReturn = string.Format("Chuc mung ban da tham gia CTKM TMAS TRI AN KHACH HANG - TRUNG NGAY LOC VANG, MDT: TMAS{0}. Hotline: 0327909399 hoac truy cap: www.trungvang.tmas.vn de biet them chi tiet", code);

            return mtReturn;

        }

    }
}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VIPAPI.Common;
using VIPAPI.Models;
using static VIPAPI.Models.APIModel;
using static VIPAPI.Models.APIResponse;
using Newtonsoft.Json;

namespace VIPAPI.Controllers
{
    public class MemberValidationController : ApiController
    {
        private string _SQL = string.Empty;
        private List<SqlParameter> _Parameter = new List<SqlParameter>();
        //private DB_Connection _db = new DB_Connection();
        rtnMsg response = new rtnMsg();
        GiftsEntities _Gifts = new GiftsEntities();
        FEDSEntities _Feds = new FEDSEntities();
        MemberCardEntities _MemberCard = new MemberCardEntities();
        WriteLogTxt WriteLog = new WriteLogTxt();
        private string apToken;

        //public IHttpActionResult Get()
        //{
        //    return Ok("OK");
        //}

        [HttpPost]
        [ApiTokenAuthorizationFilter] //判斷header Aptoken
        public IHttpActionResult Post(MemberValidation Member)
        {
            try
            {
                // 從 Request Properties 中抓取 Token
                if (Request.Properties.TryGetValue("APToken", out object tokenValue))
                {
                    apToken = tokenValue as string;
                }
                response.Status = "False";
                MemberCode _MemberCode = _Feds.MemberCode.FirstOrDefault(m => m.Code == Member.MemberCode && m.Expire > DateTime.Now);
                if (_MemberCode == null)
                {
                    response.Remark = "MemberCode 的值非有效會員條碼或已逾期";
                    WriteLog.Txt($"呼叫MemberValidation_Post，ApToken：{apToken}，{JsonConvert.SerializeObject(Member)}，Error：MemberCode 的值非有效會員條碼或已逾期");
                    return Ok(response);
                }

                var _Result = (from cd in _MemberCard.CardDate
                               join vip in _MemberCard.VIPCard on cd.Id equals vip.CDId
                               join cardInfo in _MemberCard.CardInfo on vip.CIId equals cardInfo.Id
                               where cardInfo.MemberId == _MemberCode.MemberId.ToString() &&
                                     (cardInfo.CardTypeId == 5 || cardInfo.CardTypeId == 4) &&
                                     cardInfo.MallId == "53" &&
                                     cardInfo.Year == DateTime.Now.Year &&
                                     cardInfo.Status == 1 &&
                                     cd.ExpStart < DateTime.Now &&
                                     cd.ExpEnd > DateTime.Now
                               select cd).FirstOrDefault();
                if (_Result == null)
                {
                    response.Remark = "MemberCode 的值不符合貴賓資格";
                    WriteLog.Txt($"呼叫MemberValidation_Post，ApToken：{apToken}，{JsonConvert.SerializeObject(Member)}，Error：MemberCode 的值不符合貴賓資格");
                    return Ok(response);
                }
                response.Status = "True";
                return Ok(response);
            }
            catch (Exception ex)
            {
                string jsonContent = JsonConvert.SerializeObject(Member);
                WriteLog.Txt($"呼叫MemberValidation_Post，ApToken：{apToken}，{jsonContent}，Error：{ex.Message}");
                // 回傳一個通用的錯誤訊息給客戶端
                response.Status = "Error";
                response.Remark = "伺服器內部發生錯誤";

                // 回傳 500 錯誤碼
                return Content(HttpStatusCode.InternalServerError, response);
            }
        }
    }
}

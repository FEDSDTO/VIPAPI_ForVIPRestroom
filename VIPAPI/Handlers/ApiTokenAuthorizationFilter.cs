using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Linq;
using VIPAPI.Models;
using VIPAPI.Common;
using static VIPAPI.Models.APIResponse;

public class ApiTokenAuthorizationFilter : ActionFilterAttribute
{
    GiftsEntities _Gifts = new GiftsEntities();
    WriteLogTxt WriteLog = new WriteLogTxt();
    rtnMsg errorResponse = new rtnMsg
    {
        Status = "False",
        Remark = "無效的資料格式"
    };
    // 在 Action 執行前會觸發這個方法
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
        // 檢查請求的 Header 是否包含 "APToken"
        if (!actionContext.Request.Headers.Contains("APToken"))
        {
            // 如果沒有，回傳 401 Unauthorized
            WriteLog.Txt($"呼叫ApiTokenAuthorizationFilter，Error：缺少APToken");
            // 回傳 401 Unauthorized，並將物件序列化為 JSON
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                errorResponse
            );
            return;
        }

        // 取得 APToken 的值
        var apToken = actionContext.Request.Headers.GetValues("APToken").FirstOrDefault();
        PosToken token = _Gifts.PosToken.FirstOrDefault(r => r.Token == apToken && r.Pos == "VIPAPI");
        if (token == null)
        {
            // 如果 Token 無效，回傳 401 Unauthorized
            WriteLog.Txt($"呼叫ApiTokenAuthorizationFilter，ApToken：{apToken}，Error：APToken錯誤");
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                errorResponse
            );
            return;
        }
        // 將 Token 儲存到 Request Properties 中，以便後續 Controller 存取
        actionContext.Request.Properties["APToken"] = apToken;

        //if (apToken != "96CD8AFB-40F8-4740-9514-1D4796419B73")
        //{
        //    // 如果 Token 無效，回傳 401 Unauthorized
        //    var errorResponse = new
        //    {
        //        Status = "False",
        //        Remark = "ApToken錯誤"
        //    };
        //    actionContext.Response = actionContext.Request.CreateResponse(
        //        HttpStatusCode.Unauthorized,
        //        errorResponse
        //    );
        //    return;
        //}

        base.OnActionExecuting(actionContext);
    }
}
using Microsoft.AspNetCore.Mvc;
using MODEL;
using MODEL.Enums;
using MODEL.FormatModels;

namespace COMMON;

public static class MessageHelper
{
    public static IActionResult RedirectAjax(string message, Status status, string backUrl, object data) =>
        new JsonResult(new AjaxMsgModel
        {
            Message = message,
            Status = status switch
            {
                Status.Success => "success",
                Status.Error => "error",
                _ => "error"
            },
            BackUrl = backUrl,
            Data = data
        });

    #region Send Push Notification

    public static void SendDeletedNotification()
    {
    }

    #endregion
}
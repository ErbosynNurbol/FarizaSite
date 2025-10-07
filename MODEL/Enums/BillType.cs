namespace MODEL.Enums;

public enum BillType
{
    Pending = 0,      // 待处理/检查中
    Rejected = 1,     // 被拒绝/付款失败
    Approved = 2      // 已批准/已付款
}

public static class BillTypeHelper
{
    public static string GetStatusText(int status)
    {
        return status switch
        {
            0 => "ls_Waiting",
            1 => "ls_Invalid", 
            2 => "ls_Verificationsuccessful",
            _ => "ls_Waiting"
        };
    }
}
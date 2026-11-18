using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Server.Envir;

namespace Server.WebApi.Endpoints
{
    /// <summary>
    /// 旧版 Web 端点（邮件链接激活 / 重置密码 / 删除账号、充值页占位、PayPal IPN 回调）。
    /// 由原独立 HttpListener 迁移至 Kestrel 统一托管，路由路径与查询参数保持兼容。
    /// </summary>
    public static class LegacyWebEndpoints
    {
        public static void Map(WebApplication app)
        {
            // 邮件链接命令：/Command/?Type=Activation|Reset|Delete&...
            app.MapGet("/Command/", HandleCommand);
            app.MapGet("/Command", HandleCommand);

            // 充值购买页：上游实现未启用，返回 501 避免请求悬挂。
            app.MapGet("/BuyGameGold/", BuyGameGoldPlaceholder);
            app.MapGet("/BuyGameGold", BuyGameGoldPlaceholder);

            // PayPal IPN 异步回调
            app.MapPost("/IPN/", async (HttpContext context) => await HandleIpn(context));
            app.MapPost("/IPN", async (HttpContext context) => await HandleIpn(context));
        }

        private static IResult BuyGameGoldPlaceholder()
            => Results.StatusCode(StatusCodes.Status501NotImplemented);

        private static IResult HandleCommand(HttpContext context)
        {
            var query = context.Request.Query;
            string? command = query["Type"].FirstOrDefault();
            string? activationKey = query[SEnvir.ActivationKey].FirstOrDefault();
            string? resetKey = query[SEnvir.ResetKey].FirstOrDefault();
            string? deleteKey = query[SEnvir.DeleteKey].FirstOrDefault();

            string? redirect = SEnvir.HandleLegacyCommand(command, activationKey, resetKey, deleteKey);

            // 缺少 / 未知 Type 或 key 为空时明确返回 400，避免调用方连接悬挂。
            if (redirect == null)
                return Results.BadRequest();

            return Results.Redirect(redirect, permanent: false);
        }

        private static async Task<IResult> HandleIpn(HttpContext context)
        {
            string rawMessage;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                rawMessage = await reader.ReadToEndAsync();

            // 校验在后台线程进行，端点立即返回 200，与原 HttpListener 行为一致。
            SEnvir.ProcessIpnCallback(rawMessage);
            return Results.Ok();
        }
    }
}

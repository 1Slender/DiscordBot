
using DiscordBot;

class LAPI
{
    public static void SendLogs(string message)
    {
        Tools.Tsend(message, true, Main.Plugin.Config.LogWebhook);
    }
    public static void SendBan(string message)
    {
        Tools.Tsend(message, true, Main.Plugin.Config.BanWebhook);
    }
    public static void SendCmd(string message)
    {
        Tools.Tsend(message, true, Main.Plugin.Config.CmdWebhook);
    }
}
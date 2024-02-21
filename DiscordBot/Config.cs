using Exiled.API.Interfaces;
using System.Collections.Generic;

namespace DiscordBot
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool BotEnabled { get; set; } = false;
        public string LogWebhook { get; set; } = "";
        public string BanWebhook { get; set; } = "";
        public string CmdWebhook { get; set; } = "";
    }
}

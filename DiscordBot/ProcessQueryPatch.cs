
using System;
using CommandSystem;
using Exiled.API.Features;

namespace DiscordBot
{
    public static class ProcessQueryPatch
    {
        public static void Postfix(ArraySegment<string> arguments, ICommandSender sender, ref bool __result)
        {
            string args = "";

            foreach (var arg in arguments.Array)
            {
                args += arg + " ";
            }

            args = args.Substring(0, args.Length - 1);

            if (__result && Player.Get(sender) is Player player)
            {
                LAPI.SendCmd($"[{DateTime.Now}] **{player.Nickname}** ({player.UserId} - {player.Role.Type}_{player.Id}): `{args}` {__result}");
            }
            else
            {
                LAPI.SendCmd($"[{DateTime.Now}] **{sender.LogName}** - `{args}` {__result}");
            }
        }
    }
}
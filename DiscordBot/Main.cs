
using Exiled.API.Extensions;
using Exiled.API.Features;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventServer = Exiled.Events.Handlers.Server;
using EventPlayer = Exiled.Events.Handlers.Player;
using MEC;
using RemoteAdmin;
using System.Reflection;
using CommandSystem;
using HarmonyLib;

namespace DiscordBot
{
    public class Main : Plugin<Config>
    {
        public static Main Plugin { get; private set; }
        public override string Author { get; } = "Shoulate";
        public override string Name { get; } = "DiscordBot";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(8, 8, 0);

        private static TcpListener listener;
        private static TcpClient client;
        private static Task BotTaskRun = Task.Run(() => Botds());

        private Harmony harmony = new Harmony("sh");
        private HandServer server;
        private HandPlayer player;

        public override async void OnEnabled()
        {
            Log.Debug("OnEnabled");
            Plugin = this;
            server = new HandServer();
            player = new HandPlayer();

            if (Config.BotEnabled) await BotTaskRun;

            EventServer.WaitingForPlayers += server.OnWaitingForPlayers;
            EventServer.RoundStarted += server.OnRoundStarted;
            EventServer.RoundEnded += server.OnRoundEnded;
            EventServer.RespawningTeam += server.OnRespawningTeam;
            EventServer.LocalReporting += server.OnLocalReporting;

            EventPlayer.Left += player.OnLeft;
            EventPlayer.Verified += player.OnVerified;
            EventPlayer.Banning += player.OnBanning;
            EventPlayer.Dying += player.OnDying;
            EventPlayer.Spawned += player.OnSpawned;
            EventPlayer.Escaping += player.OnEscaping;

            PatchCommands();

            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Log.Debug("OnDisabled");
            BotTaskRun.Dispose();
            harmony.UnpatchAll();

            EventServer.WaitingForPlayers -= server.OnWaitingForPlayers;
            EventServer.RoundStarted -= server.OnRoundStarted;
            EventServer.RoundEnded -= server.OnRoundEnded;
            EventServer.RespawningTeam -= server.OnRespawningTeam;
            EventServer.LocalReporting -= server.OnLocalReporting;

            EventPlayer.Left -= player.OnLeft;
            EventPlayer.Verified -= player.OnVerified;
            EventPlayer.Banning -= player.OnBanning;
            EventPlayer.Dying -= player.OnDying;
            EventPlayer.Spawned -= player.OnSpawned;
            EventPlayer.Escaping -= player.OnEscaping;

            server = null;
            player = null;

            base.OnDisabled();
        }
        private void PatchCommands()
        {
            foreach (ICommand command in CommandProcessor.RemoteAdminCommandHandler.AllCommands)
                PatchCommand(command);
        }
        private void PatchCommand(ICommand command)
        {
            if (command is ParentCommand parentCommand)
            {
                PatchParent(parentCommand);
                return;
            }
            try
            {
                harmony.Patch(command.GetType().GetMethod("Execute", BindingFlags.Public | BindingFlags.Instance), postfix: new HarmonyMethod(typeof(ProcessQueryPatch).GetMethod(nameof(ProcessQueryPatch.Postfix), BindingFlags.Public | BindingFlags.Static)));
            }
            catch
            {
                //Log.Info("Warning: Error = harmony.Patch " + command);
            }
        }
        private void PatchParent(ParentCommand parentCommand)
        {
            foreach (ICommand command in parentCommand.AllCommands)
                PatchCommand(command);
        }


        private static async void Botds()
        {
            //Log.Info("Botds");
            //await Task.Delay(1000);

            byte[] data = new byte[256];
            int leng;
            string[] msgcmd;
            Player player;
            string ls;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            try
            {
                while (true)
                {
                    listener = new TcpListener(localAddr, 15150);
                    listener.Start();

                    try
                    {
                        client = await listener.AcceptTcpClientAsync();
                        leng = client.GetStream().Read(data, 0, data.Length);
                        msgcmd = Encoding.UTF8.GetString(data, 0, leng).Split(' ');
                        Log.Info(Encoding.UTF8.GetString(data, 0, leng));
                        client.Close();

                        switch (msgcmd[0])
                        {
                            case "ban":
                                try
                                {
                                    PluginAPI.Core.Server.BanPlayerByUserId(msgcmd[1], msgcmd[3], Convert.ToInt64(msgcmd[2]));
                                    Log.Info($"Удаленный бан {msgcmd[1]} на {msgcmd[2]} по причине {msgcmd[3]}");
                                    Tools.Tsend($"Удаленный бан {msgcmd[1]} на {msgcmd[2]} по причине {msgcmd[3]}", true, Plugin.Config.CmdWebhook);
                                    Tools.Tsend($"### Игрока наказали\n1. {msgcmd[1]}\n2. Удаленный бан\n3. {msgcmd[3]}\n4. {msgcmd[2]} секунд", true, Plugin.Config.BanWebhook);
                                }
                                catch
                                {
                                    Tools.Tsend($"Error: Не известная причина", true, Plugin.Config.LogWebhook);
                                }
                                break;

                            case "unban":
                                try
                                {
                                    PluginAPI.Core.Server.UnbanPlayerByUserId(msgcmd[1]);

                                    Log.Info($"Удаленный разбан {msgcmd[1]}");
                                    Tools.Tsend($"Удаленный разбан {msgcmd[1]}", true, Plugin.Config.CmdWebhook);
                                }
                                catch
                                {
                                    Tools.Tsend($"Error: Не известная причина", true, Plugin.Config.LogWebhook);
                                }
                                break;

                            case "ipban":
                                PluginAPI.Core.Server.UnbanPlayerByIpAddress(msgcmd[1]);

                                Log.Info($"Удаленный разбан {msgcmd[1]} по айпи");
                                Tools.Tsend($"Удаленный разбан {msgcmd[1]} по айпи", true, Plugin.Config.CmdWebhook);
                                break;

                            case "ipunban":
                                PluginAPI.Core.Server.BanPlayerByIpAddress(msgcmd[1], msgcmd[3], Convert.ToInt16(msgcmd[2]));

                                Log.Info($"Удаленный бан {msgcmd[1]} на {msgcmd[2]} по причине {msgcmd[3]} по айпи");
                                Tools.Tsend($"Удаленный бан {msgcmd[1]} на {msgcmd[2]} по причине {msgcmd[3]} по айпи", true, Plugin.Config.CmdWebhook);
                                break;

                            case "idkickplayer":
                                player = Player.Get(Convert.ToInt16(msgcmd[1]));

                                if (player == null)
                                {
                                    Log.Info($"Команда idkickplayer, игрок не найден");
                                    Tools.Tsend($"Игрок не найден", true, Plugin.Config.LogWebhook);

                                    break;
                                }

                                player.Kick(msgcmd[2]);

                                Log.Info($"Удаленный кик {msgcmd[1]} по причине {msgcmd[2]}");
                                Tools.Tsend($"Удаленный кик {msgcmd[1]} - {player.Nickname} по причине {msgcmd[2]}", true, Plugin.Config.CmdWebhook);
                                Tools.Tsend($"Info\n```{player.Nickname}\n{player.Id}\n{player.UserId}\n{player.Role.Type}```", true, Plugin.Config.LogWebhook);
                                break;

                            case "steamidkickplayer":
                                player = Player.Get(msgcmd[1]);

                                if (player == null)
                                {
                                    Log.Info($"Команда steamidkickplayer, игрок не найден");
                                    Tools.Tsend($"Игрок не найден", true, Plugin.Config.LogWebhook);

                                    break;
                                }

                                player.Kick(msgcmd[2]);

                                Log.Info($"Удаленный кик {msgcmd[1]} - {player.Nickname} по причине {msgcmd[2]}");
                                Tools.Tsend($"Удаленный кик {msgcmd[1]} - {player.Nickname} по причине {msgcmd[2]}", true, Plugin.Config.CmdWebhook);
                                Tools.Tsend($"Info\n```{player.Nickname}\n{player.Id}\n{player.UserId}\n{player.Role.Type}```", true, Plugin.Config.LogWebhook);
                                break;

                            case "bc":
                                //Log.Debug("wait");
                                if (msgcmd.Count() >= 2)
                                {
                                    Map.Broadcast(5, msgcmd[1]);
                                }
                                else
                                {
                                    Tools.Tsend("Error", true, Plugin.Config.LogWebhook);
                                    break;
                                }
                                Log.Info("Вроде команда выполнена");
                                Tools.Tsend("Вроде команда выполнена", true, Plugin.Config.LogWebhook);
                                break;

                            case "ls":
                                if (Player.List.Count() == 0)
                                {
                                    Tools.Tsend("### На сервере нет игроков", true, Plugin.Config.LogWebhook);
                                    Log.Info("На сервере нет игроков");
                                    break;
                                }

                                ls = "# Игроки на сервере:\n```";

                                foreach (Player i in Player.List)
                                {
                                    ls += $"{i.Nickname} ({i.Role.Type}) - {i.GroupName}\n";
                                }

                                ls += "```";

                                Tools.Tsend(ls, true, Plugin.Config.LogWebhook);
                                Log.Info("Дискорд команда list");
                                break;

                            case "lsp":
                                if (Player.List.Count() == 0)
                                {
                                    Tools.Tsend("### На сервере нет игроков", true, Plugin.Config.LogWebhook);
                                    Log.Info("На сервере нет игроков");
                                    break;
                                }

                                ls = "# Игроки на сервере:\n```";

                                foreach (Player i in Player.List)
                                {
                                    ls += $"{i.Nickname} ({i.Role.Type}) - {i.RankName}\n";
                                }

                                ls += "```";

                                Tools.Tsend(ls, true, Plugin.Config.LogWebhook);
                                Log.Info("Дискорд команда listp");
                                break;

                            case "listid":
                                if (Player.List.Count() == 0)
                                {
                                    Tools.Tsend("### На сервере нет игроков", true, Plugin.Config.LogWebhook);
                                    Log.Info("На сервере нет игроков");
                                    break;
                                }

                                ls = "# Игроки на сервере:\n```";

                                foreach (Player i in Player.List)
                                {
                                    ls += $"{i.Nickname} - {i.UserId}\n";
                                }

                                ls += "```";

                                Tools.Tsend(ls, true, Plugin.Config.LogWebhook);
                                Log.Info("Дискорд команда listid");
                                break;

                            case "group":
                                player = Player.Get(msgcmd[1]);
                                player.Group = UserGroupExtensions.GetValue(msgcmd[2]);
                                Log.Info($"{player.Nickname} выдали права {msgcmd[2]}");
                                Tools.Tsend($"{player.Nickname} выдали права {msgcmd[2]}", true, Plugin.Config.CmdWebhook);
                                break;

                            case "sr":
                                Map.Broadcast(3, "<color=red>Cервер перезапускается");
                                Timing.CallDelayed(3f, () =>
                                {
                                    Tools.Tsend("# ServerRestart", true, Plugin.Config.LogWebhook);
                                    Server.Restart();
                                    Log.Info("Server.Restart() запущен");
                                });
                                break;

                            case "roundrestart":
                                Map.Broadcast(3, "<color=green>Раунд рестарт");
                                Tools.Tsend("# RoundRestart", true, Plugin.Config.LogWebhook);
                                Timing.CallDelayed(3f, () => { Round.Restart(); });
                                break;

                            case "infoplayer":
                                player = Player.Get(msgcmd[1]);
                                if (player == null)
                                {
                                    Tools.Tsend("Такого игрока нет", true, Plugin.Config.LogWebhook);
                                    break;
                                }
                                Tools.Tsend($"# {player.Nickname}\n```{player.Id}\n{player.NetId}\n{player.Ping}\n{player.RankName}\n{player.RemoteAdminPermissions}\n{player.Role.Type}\n{player.Scale}\n{player.Zone}\n{player.KickPower}\n{player.Items}\n{player.IsVerified}\n{player.IsOverwatchEnabled}\n{player.IsScp}\n{player.IsNoclipPermitted}\n{player.IsMuted}\n{player.IsHuman}\n{player.IsGodModeEnabled}\n{player.IsFriendlyFireEnabled}\n{player.IsDead}\n{player.IsCuffed}\n{player.IsAlive}\n{player.IPAddress}\n{player.Health}\n{player.GroupName}\n{player.GlobalBadge}\n{player.CustomName}\n{player.CurrentRoom}\n{player.BadgeHidden}```", true, Plugin.Config.LogWebhook);
                                break;

                            case "listrole":
                                if (Player.List.Count() == 0)
                                {
                                    Tools.Tsend("### На сервере нет игроков", true, Plugin.Config.LogWebhook);
                                    Log.Info("На сервере нет игроков");
                                    break;
                                }
                                ls = "# Роли игроков\n```";
                                foreach (Player plr in Player.List)
                                {
                                    ls += $"{plr.Nickname} - {plr.Role.Name}\n";
                                }
                                ls += "```";
                                Log.Info("Ds command listrole");
                                Tools.Tsend(ls, true, Plugin.Config.LogWebhook);
                                break;

                            //case "reloadremoteadmin":
                            //    Timing.CallDelayed(5f, () => { ConfigManager.ReloadRemoteAdmin(); });
                            //    ReloadSysLDP();
                            //
                            //    break;

                            default:
                                Tools.Tsend("Error", true, Plugin.Config.LogWebhook);
                                break;
                        }
                    }
                    catch
                    {
                        Log.Warn("Discord Command Error");
                    }

                    listener.Stop();
                }
            }
            catch
            {
                Log.Error("Botds вызвал ошибку, бот отключен");
            }
        }
    }
}

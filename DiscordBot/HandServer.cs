
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using System;

namespace DiscordBot
{
    class HandServer
    {
        public void OnWaitingForPlayers()
        {
            LAPI.SendLogs($"## {DateTime.Now} *Ожидание игроков*");
        }
        public void OnRoundStarted()
        {
            LAPI.SendLogs($"## {DateTime.Now} *Раунд начался*");
        }
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            LAPI.SendLogs($"## {DateTime.Now} *Раунд окончен*");
        }
        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            LAPI.SendLogs($"Тип респавна ***{ev.NextKnownTeam}*** Максимальное число спавна ***{ev.MaximumRespawnAmount}***");
        }
        public void OnLocalReporting(LocalReportingEventArgs ev)
        {
            LAPI.SendLogs($"# [{DateTime.Now}] `{ev.Player.Nickname}` __{ev.Player.Id}__ отправил жалобу на `{ev.Target}` по причине: {ev.Reason}");
        }
    }
}


using Exiled.API.Features;
using System;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;

namespace DiscordBot
{
    class HandPlayer
    {
        public void OnLeft(LeftEventArgs ev)
        {
            LAPI.SendLogs($"[{DateTime.Now}] :arrow_backward: **{ev.Player.Nickname}** ({ev.Player.UserId} - {ev.Player.IPAddress}) {ev.Player.Role.Name} отключился");
        }
        public void OnVerified(VerifiedEventArgs ev)
        {
            LAPI.SendLogs($"[{DateTime.Now}] :arrow_right: **{ev.Player.Nickname}** ({ev.Player.UserId} - {ev.Player.IPAddress}) присоединился");
        }
        public void OnBanning(BanningEventArgs ev)
        {
            Map.Broadcast(3, $"{ev.Target.Nickname} был забанен");

            TimeSpan tm = TimeSpan.FromSeconds(ev.Duration);
            LAPI.SendBan($"### Игрока наказали\n1. {ev.Target.Nickname} {ev.Target.UserId}\n2. {ev.Player.Nickname} {ev.Player.UserId}\n3. {ev.Reason}\n4. {tm.Days}d:{tm.Hours}h:{tm.Minutes}m:{tm.Seconds}s");
        }
        public void OnDying(DyingEventArgs ev)
        {
            Player player = ev.Player;
            Player attacker = ev.Attacker;

            if (player == null || !player.IsConnected || player.Role.Type == RoleTypeId.None)
            {
                return;
            }
            else if (attacker == null)
            {
                LAPI.SendLogs($"[{DateTime.Now}] :skull: **{player.Nickname}** ({player.UserId} - {player.Role.Name}) был убит");
            }
            else if (player.Role.Type == attacker.Role.Type)
            {
                LAPI.SendLogs($"[{DateTime.Now}] :face_with_symbols_over_mouth: **{player.Nickname}**  ({player.UserId} - {player.Role.Name}) был убит союзником **{attacker.Nickname}** ({attacker.UserId} - {attacker.Role.Name})");
            }
            else
            {
                LAPI.SendLogs($"[{DateTime.Now}] :crossed_swords: **{player.Nickname}**  ({player.UserId} - {player.Role.Name}) был убит **{attacker.Nickname}** ({attacker.UserId} - {attacker.Role.Name})");
            }
        }
        public void OnSpawned(SpawnedEventArgs ev)
        {
            Player player = ev.Player;

            if (player != null && player.IsConnected && player.Role.Name != ev.OldRole.Name && ev.OldRole != RoleTypeId.None)
                LAPI.SendLogs($"[{DateTime.Now}] :innocent: **{player.Nickname}** {player.UserId} **{ev.OldRole.Name}** появился в виде **{player.Role.Name}**");
        }
        public void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.IsAllowed)
                LAPI.SendLogs($"{DateTime.Now} :athletic_shoe: **{ev.Player.Nickname}** ({ev.Player.UserId} - {ev.Player.Role.Name}) сбежал и стал {ev.NewRole}");
        }
    }
}

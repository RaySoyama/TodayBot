using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace WorldMachine.Commands
{
    public class WorldMachineBotCommands : ModuleBase<SocketCommandContext>
    {

        [Command("Ping"), Alias("ping"), Summary("Returns the latency")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"MS {Program.latency}");
            return;
        }

        [Command("Water"), Summary("Call when you drink water")]
        public async Task DrinkWater()
        {
            Program.TodayBot_UserWaterData.totalWaterConsumed += 1;
            Program.TodayBot_UserWaterData.calenderWaterData.Add(DateTime.UtcNow);

            var reaction = new Emoji("💦");
            await Context.Message.AddReactionAsync(reaction);
            Program.SaveUserData();
        }
    }
}
using System.Linq;
using System.Text;

namespace AottgBotLib.Commands.Internal
{
    internal class InternalCommandsModule : CommandModule
    {
        private int _CommandsPerMessage = 4;

        [Command("help")]
        [Description("Gets descriptions and list of commands")]
        public void HelpCommand(CommandContext ctx)
        {
            int Result = 0;
            if (ctx.Arguments.Length == 0 || (ctx.Arguments.Length == 1 && int.TryParse(ctx.Arguments[0], out Result)))
            {
                StringBuilder bld = new StringBuilder();
                var CommandsList = ctx.Client.CommandHandler.AllCommands.Where(x => !x.IsHidden);

                var cmds = CommandsList.Skip(Result * _CommandsPerMessage).Take(_CommandsPerMessage);
                if (!cmds.Any()) return;
                if (Result == 0)
                {
                    bld.Append($"\nType {ctx.Client.CommandHandler.Prefix}help *command* to get description of command.\nList of commands:");
                }
                foreach (var cmd in cmds)
                {
                    bld.AppendLine("\n" + cmd.LowerName);
                }
                if (CommandsList.Count() > Result * _CommandsPerMessage + _CommandsPerMessage)
                {
                    bld.AppendLine($"\nType {ctx.Client.CommandHandler.Prefix}help {Result + 1} for the next Page!");
                }
                ctx.Client.SendChatMessage(bld.ToString(), ctx.Sender);
            }
            else
            {
                var cmd = ctx.Client.CommandHandler.AllCommands.FirstOrDefault(c => c.LowerName == ctx.Arguments[0].ToLower());

                object[] args = new object[] { string.Empty, ctx.Arguments[0].ToLower() };

                if (cmd == null)
                {
                    args[0] = "No such command";
                }
                else if (cmd.Description == null)
                {
                    args[0] = "There is no description yet ;(";
                }
                else
                {
                    args[0] = cmd.Description;
                }
                ctx.Client.SendRPC(2, "Chat", args, ctx.Sender.ActorNumber);
            }
        }
    }
}
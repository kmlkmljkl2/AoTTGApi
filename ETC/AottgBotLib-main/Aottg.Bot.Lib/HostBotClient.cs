using AottgBotLib.Internal;

namespace AottgBotLib
{
    /// <summary>
    /// Client for hosting rooms
    /// </summary>
    public sealed class HostBotClient : BotClient
    {
        private HostInRoomCallbacks _roomHostCallbacks;

        /// <summary>
        /// This message will be sent to a just joined player
        /// </summary>
        public string GreetingMessage { get; set; } = "Hello! And welcome to auto-hosted room. Hope you will enjoy playing here! UwU";

        public HostBotClient() : this("HostBot")
        {
        }

        public HostBotClient(string name) : base(name)
        {
            _roomHostCallbacks = new HostInRoomCallbacks(this);
            AddCallbackTarget(_roomHostCallbacks);
        }
    }
}
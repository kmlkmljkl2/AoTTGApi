using System.Text.RegularExpressions;

namespace AoTTGApi.CarrierClasses
{
    public class RoomInfo
    {
        public RoomInfo(string rawName, int maxPlayers, int playerCount)
        {
            RawName = rawName;
            MaxPlayers = maxPlayers;
            PlayerCount = playerCount;

            var args = RawName.Split('`');

            Name = args[0];
            NoColorName = Regex.Replace(Name, @"\[([\W\w]{6})\]", "");
            Map = args[1];
            Difficulty = args[2];

            if (int.TryParse(args[3], out int time))
            {
                Time = time;
            }

            DayTime = args[4];
            EncryptedPassword = args[5];

            if (int.TryParse(args[6], out int number))
            {
                RandomRoomNumber = number;
            }
        }

        public string RawName { get; set; }
        public string Name { get; set; }
        public string Difficulty { get; set; }
        public string NoColorName { get; set; }
        public string Map { get; set; }
        public string DayTime { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
        public int RandomRoomNumber { get; set; }
        public int Time { get; set; }
        public string EncryptedPassword { get; set; }
    }
}
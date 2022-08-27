using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace AottgBotLib.Internal
{
    internal class LobbyCallbacks : ILobbyCallbacks
    {
        private List<RoomInfo> _rooms = new List<RoomInfo>();

        public IReadOnlyList<RoomInfo> Rooms => _rooms;

        public void OnJoinedLobby()
        {
            _rooms = new List<RoomInfo>();
        }

        public void OnLeftLobby()
        {
            _rooms = null;
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo room in roomList)
            {
                var Room = _rooms.FirstOrDefault(inListRoom => inListRoom.Name == room.Name);
                if (Room == null)
                {
                    _rooms.Add(room);
                }
                else
                {
                    lock (_rooms)
                    {
                        _rooms.Remove(Room);
                        _rooms.Add(room);
                    }
                }
            }
        }
    }
}
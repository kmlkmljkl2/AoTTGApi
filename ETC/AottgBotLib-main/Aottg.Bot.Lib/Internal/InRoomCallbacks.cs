﻿using Photon.Realtime;
using System.Collections;

namespace AottgBotLib.Internal
{
    internal class InRoomCallbacks : IInRoomCallbacks
    {
        public void OnMasterClientSwitched(Player newMasterClient)
        {
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            Log.LogInfo($"Player [{newPlayer.ActorNumber}] joined room.");
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.LogInfo($"Player [{otherPlayer.ActorNumber}] left room.");
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }
    }
}
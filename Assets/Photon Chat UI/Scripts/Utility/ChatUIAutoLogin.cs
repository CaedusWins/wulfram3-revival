/*
 * Copyright (C) 2015 Exit Games GmbH
 * by The Knights of Unity
 */

using Photon;
using UnityEngine;

namespace PhotonChatUI
{
    public class ChatUIAutoLogin : PunBehaviour
    {
        bool ischatConnected = false;
        private ChatUI _chatUI;

        public ChatUI chatUI
        {
            get { return _chatUI ?? (_chatUI = GetComponent<ChatUI>()); }
        }

        void Update()
        {
      
            // Wulfram: no chat while PUN is offline (offline play, smoke test). Chat would otherwise
            // reach Photon's cloud with the 2017 Chat App ID, and log "Cannot send op" on leaving.
            if (ischatConnected == false && PhotonNetwork.offlineMode)
            {
                ischatConnected = true;
                return;
            }

            if (ischatConnected == false)
            {
                base.OnJoinedRoom();
                Debug.Log("Joined Room CHAT");
                chatUI.Connect(PhotonNetwork.playerName);
                    Debug.Log("Chat Player Connected");
                ischatConnected = true;
            }
        }
    }
}

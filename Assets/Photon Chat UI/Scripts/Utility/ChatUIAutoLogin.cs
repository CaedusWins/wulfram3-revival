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
      
            // Wulfram: no chat while PUN is offline (offline play, smoke test), nor until the project
            // has its own Photon Chat app (Wulfram.Networking.PhotonCloudSettings.ChatEnabled): the
            // serialized Chat App ID is the original 2017 team's. It also logged "Cannot send op" on leaving.
            if (ischatConnected == false && (PhotonNetwork.offlineMode || !Wulfram.Networking.PhotonCloudSettings.ChatEnabled))
            {
                ischatConnected = true;
                // Hide the whole chat UI (this object, HUDCanvas/Chat: main window with its dock and
                // login form, chatbox, status icon) - never connected, it showed an empty
                // Username/Password/Login form. A CanvasGroup hides it without deactivating anything
                // (no chat OnDisable runs) and stops it taking clicks or keyboard focus.
                CanvasGroup hide = GetComponent<CanvasGroup>();
                if (hide == null)
                {
                    hide = gameObject.AddComponent<CanvasGroup>();
                }
                hide.alpha = 0f;
                hide.interactable = false;
                hide.blocksRaycasts = false;
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

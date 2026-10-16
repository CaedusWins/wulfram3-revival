//using Assets.Plugins.webgljs;
using Assets.InternalApis.Interfaces;
using Assets.Plugins.webgljs;
using Proyecto26;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.InternalApis.Implementations
{
    /// <summary>
    /// This will access the users Discord account, getting data about a user (Username, email, ect.)
    /// </summary>
    public class DiscordApi : IDiscordApi
    {
        private const string discordApiUrl = "https://discordapp.com/api/v6";
        // The webhook that used to be hardcoded here was a live credential committed to source control.
        // It has been removed; set WULFRAM_DISCORD_WEBHOOK_URL locally to re-enable join/leave posts.
        private static readonly string channelUrl = System.Environment.GetEnvironmentVariable("WULFRAM_DISCORD_WEBHOOK_URL");
        private const string joinMessage = "{0} has started playing Wulfram 3!";
        private const string leftMessage = "{0} has left Wulfram 3!";

        public IEnumerator PlayerJoined(string playerName)
        {
            if (string.IsNullOrEmpty(channelUrl)) yield break;

            var greetdiscord = string.Format(joinMessage, playerName);
            var postdiscord = "{ \"content\": \"" + greetdiscord + "\" } ";

            yield return Post(channelUrl, postdiscord);
        }

        public IEnumerator PlayerLeft(string playerName)
        {
            if (string.IsNullOrEmpty(channelUrl)) yield break;

            var greetdiscord = string.Format(leftMessage, playerName);
            var postdiscord = "{ \"content\": \"" + greetdiscord + "\" } ";

            yield return Post(channelUrl, postdiscord);
        }

        private IEnumerator Post(string url, string bodyJsonString)
        {
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.Send();

                Debug.Log("Status Code: " + request.responseCode);
            }
        }

        private IEnumerator Get(string url, string bodyJsonString)
        {
            using (var request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.Send();

                Debug.Log("Status Code: " + request.responseCode);
            }
        }

    }
}
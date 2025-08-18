using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class TwitchLoginUI : MonoBehaviour
{
    public TMP_Text connectionStatusText;

    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField OauthTokenInput;

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public void OnLoginButtonPressed()
    {
        TwitchLoginDetails.Username = usernameInput.text;
        TwitchLoginDetails.OAuthToken = OauthTokenInput.text;

        TryConnect();
    }

    // connect and check the message sent back to confirm a connection can be established.
    private void TryConnect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + OauthTokenInput.text);
        writer.WriteLine("NICK " + usernameInput.text);
        writer.WriteLine("USER " + usernameInput.text + " 8 * :" + usernameInput.text);
        writer.WriteLine("JOIN #" + usernameInput.text);
        writer.Flush();

        string message = reader.ReadLine();
        Debug.Log(message);

        if (message.Contains("Login authentication failed"))
        {
            connectionStatusText.text = "Login authentication failed";
            connectionStatusText.color = Color.red;
        }
        else if (message.Contains("Improperly formatted auth"))
        {
            connectionStatusText.text = "Improperly formatted auth";
            connectionStatusText.color = Color.red;
        }
        else if (message.Contains("Invalid NICK"))
        {
            connectionStatusText.text = "Invalid Username (should be the same as channel name.)";
            connectionStatusText.color = Color.red;
        }
        else if (message.Contains("Welcome, GLHF!"))
        {
            connectionStatusText.text = "Connected to chat";
            connectionStatusText.color = Color.green;
        }
    }
}

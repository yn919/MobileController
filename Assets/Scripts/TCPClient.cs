using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI connectedStatus;

    private const int DEFAULT_PORT = 9003;
    private const string CONNECTED = "Connected";
    private const string DISCONNECTED = "Disconnected";

    private TcpClient tcpClient;

    private void Awake()
    {
        // Ensure UI references are set
        if (inputField == null || connectedStatus == null)
        {
            Debug.LogError("TCPClient: UI references are missing. Please assign in the inspector.");
        }
    }

    public async void Connect()
    {
        // Disconnect existing connection if any
        DisconnectClient();

        try
        {
            if (string.IsNullOrWhiteSpace(inputField.text))
            {
                Debug.LogWarning("Please enter a valid IP address.");
                UpdateConnectionStatus(false);
                return;
            }

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(inputField.text, DEFAULT_PORT);
            UpdateConnectionStatus(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
            UpdateConnectionStatus(false);
        }
    }

    private void UpdateConnectionStatus(bool isConnected)
    {
        if (connectedStatus != null)
        {
            connectedStatus.text = isConnected ? CONNECTED : DISCONNECTED;
        }
    }

    private void DisconnectClient()
    {
        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
    }

    private async Task SendCommand(string command)
    {
        if (tcpClient == null || !tcpClient.Connected)
        {
            Debug.LogWarning("Not connected. Cannot send command.");
            return;
        }

        try
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(command);
            NetworkStream stream = tcpClient.GetStream();
            await stream.WriteAsync(sendBuffer, 0, sendBuffer.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending command: {e.Message}");
        }
    }

    public void OnClickUpButton()
    {
        _ = SendCommand("UP");
    }

    public void OnClickDownButton()
    {
        _ = SendCommand("DOWN");
    }

    public void OnClickLeftButton()
    {
        _ = SendCommand("LEFT");
    }

    public void OnClickRightButton()
    {
        _ = SendCommand("RIGHT");
    }

    private void OnDestroy()
    {
        DisconnectClient();
    }
}

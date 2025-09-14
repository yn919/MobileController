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
        // UIの参照が設定されていることを確認
        if (inputField == null || connectedStatus == null)
        {
            Debug.LogError("TCPClient: UIの参照が不足しています。インスペクターで割り当ててください。");
        }
    }

    public async void Connect()
    {
        // 既存の接続があれば切断
        DisconnectClient();

        try
        {
            if (string.IsNullOrWhiteSpace(inputField.text))
            {
                Debug.LogWarning("有効なIPアドレスを入力してください。");
                UpdateConnectionStatus(false);
                return;
            }

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(inputField.text, DEFAULT_PORT);
            UpdateConnectionStatus(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"接続に失敗しました: {e.Message}");
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
            Debug.LogWarning("接続されていません。コマンドを送信できません。");
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
            Debug.LogError($"コマンドの送信中にエラーが発生しました: {e.Message}");
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

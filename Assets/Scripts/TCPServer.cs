using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;

public class TCPServer : MonoBehaviour
{
    private string ipAddress = "127.0.0.1";
    private int port = 9003;
    private TcpListener tcpListener;

    private void Awake()
    {
        Task.Run(() => this.OnProcess());
    }

    private async Task OnProcess()
    {
        this.tcpListener = new TcpListener(IPAddress.Parse(this.ipAddress), this.port);
        tcpListener.Start();
        using TcpClient acceptedClient = await tcpListener.AcceptTcpClientAsync();

        using NetworkStream stream = acceptedClient.GetStream();

        byte[] buffer = new byte[1024];
        int readByte = 0;
        readByte = await stream.ReadAsync(buffer, 0, buffer.Length);

        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, readByte);
    }

    private void OnDestroy()
    {
        this.tcpListener?.Stop();
    }
}

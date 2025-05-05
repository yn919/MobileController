using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient tcpClient;
    private string ipAddress = "127.0.0.1";
    private int port = 9003;

    private void Awake()
    {
        Task.Run(() => this.OnProcess());
    }

    private async Task OnProcess()
    {
        this.tcpClient = new TcpClient();
        await this.tcpClient.ConnectAsync(ipAddress, port);

        Debug.Log("Connected client");
    }

    public void OnClickUpButton()
    {
        if(!this.tcpClient.Connected)
        {
            return;
        }

        try
        {
            byte[] sendBuffer = Encoding.UTF8.GetBytes(nameof(OnClickUpButton));
            NetworkStream stream = this.tcpClient.GetStream();
            stream.Write(sendBuffer, 0, sendBuffer.Length);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void OnClickDownButton()
    {

    }

    public void OnClickLeftButton()
    {

    }

    public void OnClickRightButton()
    {

    }

    private void OnDestroy()
    {
        this.tcpClient?.Close();
    }
}

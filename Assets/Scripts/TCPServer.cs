using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using TMPro;

public class TCPServer : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipAddressInput;
    private const int DEFAULT_PORT = 9003;
    private const int BufferSize = 1024;

    private TcpListener tcpListener;
    private CancellationTokenSource cancellationTokenSource;

    // Movement control variables
    public bool IsMovingUp { get; private set; } = false;
    public bool IsMovingDown { get; private set; } = false;
    public bool IsMovingLeft { get; private set; } = false;
    public bool IsMovingRight { get; private set; } = false;

    private void Awake()
    {
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void StartServer()
    {
        // Stop existing server if running
        StopServer();

        // Create a new cancellation token source
        cancellationTokenSource = new CancellationTokenSource();

        // Start new server
        Task.Run(async () => await RunServerAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
    }

    public void StopServer()
    {
        // Cancel any existing server operation
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }

        // Close the TCP listener if it exists
        if (tcpListener != null)
        {
            tcpListener.Stop();
            tcpListener = null;
        }
    }

    private async Task RunServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            string ipAddress = ipAddressInput != null ? ipAddressInput.text : "127.0.0.1";
            tcpListener = new TcpListener(IPAddress.Parse(ipAddress), DEFAULT_PORT);
            tcpListener.Start();
            Debug.Log($"Server started on {ipAddress}:{DEFAULT_PORT}");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Server error: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[BufferSize];
                while (!cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    ProcessCommand(command);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Client handling error: {ex.Message}");
        }
    }

    // 移動コマンドのイベント
    public event System.Action<Vector2> OnMovementCommand;

    private void ProcessCommand(string command)
    {
        Debug.Log($"Received command: {command}");

        Vector2 movement = Vector2.zero;

        switch (command.ToUpper())
        {
            case "UP":
                movement.y = 1f;
                IsMovingUp = true;
                IsMovingDown = false;
                break;
            case "DOWN":
                movement.y = -1f;
                IsMovingUp = false;
                IsMovingDown = true;
                break;
            case "LEFT":
                movement.x = -1f;
                IsMovingLeft = true;
                IsMovingRight = false;
                break;
            case "RIGHT":
                movement.x = 1f;
                IsMovingLeft = false;
                IsMovingRight = true;
                break;
            default:
                Debug.LogWarning($"Unknown command: {command}");
                break;
        }

        // イベントを発行
        OnMovementCommand?.Invoke(movement);
    }

    private void Update()
    {
        Vector2 movement = Vector2.zero;
        
        if (IsMovingUp)
            movement.y += 1f;
        else if (IsMovingDown)
            movement.y -= 1f;
        
        if (IsMovingLeft)
            movement.x -= 1f;
        else if (IsMovingRight)
            movement.x += 1f;
        
        // Normalize movement to prevent faster diagonal movement
        movement = movement.normalized;
        
        // Move based on the movement vector
        transform.Translate(movement * Time.deltaTime);
    }

    private void OnDestroy()
    {
        // Cancel all ongoing operations
        cancellationTokenSource?.Cancel();
        tcpListener?.Stop();
    }
}

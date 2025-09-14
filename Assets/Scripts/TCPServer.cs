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

    // 移動制御用の変数
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
        // 既存のサーバーが実行中であれば停止
        StopServer();

        // 新しいキャンセレーショントークンを作成
        cancellationTokenSource = new CancellationTokenSource();

        // 新しいサーバーを開始
        Task.Run(async () => await RunServerAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
    }

    public void StopServer()
    {
        // 既存のサーバー操作をキャンセル
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }

        // TCPリスナーが存在する場合は閉じる
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
            Debug.Log($"サーバーを開始しました: {ipAddress}:{DEFAULT_PORT}");

            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"サーバーエラー: {ex.Message}");
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
            Debug.LogError($"クライアント処理エラー: {ex.Message}");
        }
    }

    // 移動コマンドのイベント
    public event System.Action<Vector2> OnMovementCommand;

    private void ProcessCommand(string command)
    {
        Debug.Log($"コマンドを受信: {command}");

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
                Debug.LogWarning($"不明なコマンド: {command}");
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
        
        // 斜め移動時の速度を正規化
        movement = movement.normalized;
        
        // 移動ベクトルに基づいて移動
        transform.Translate(movement * Time.deltaTime);
    }

    private void OnDestroy()
    {
        // 進行中のすべての操作をキャンセル
        cancellationTokenSource?.Cancel();
        tcpListener?.Stop();
    }
}

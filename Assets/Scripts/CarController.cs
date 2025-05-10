using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("車両設定")]
    [SerializeField] private List<AxleInfo> axleInfos;
    [SerializeField] private float maxMotorTorque = 1000f;
    [SerializeField] private float maxSteeringAngle = 30f;

    // 入力管理
    private float verticalInput = 0f;
    private float horizontalInput = 0f;

    // TCP入力参照
    [SerializeField] private TCPServer tcpServer;

    private void Awake()
    {
        if (tcpServer == null)
        {
            Debug.LogError("TCPサーバーが設定されていません。入力制御ができません。");
            return;
        }

        // イベントにサブスクライブ
        tcpServer.OnMovementCommand += HandleMovementCommand;
    }

    private void OnDestroy()
    {
        // イベントのサブスクライブ解除
        if (tcpServer != null)
        {
            tcpServer.OnMovementCommand -= HandleMovementCommand;
        }
    }

    private void HandleMovementCommand(Vector2 movement)
    {
        // イベントから入力を取得
        verticalInput = movement.y;
        horizontalInput = movement.x;
    }

    private void FixedUpdate()
    {
        // モーターとステアリングを制御
        float motor = maxMotorTorque * verticalInput;
        float steering = maxSteeringAngle * horizontalInput;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                // ステアリング角度の設定
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                // モーター出力の設定
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }
}

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}
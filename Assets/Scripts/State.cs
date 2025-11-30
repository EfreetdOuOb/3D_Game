using System;
using UnityEngine;

public abstract class BaseState
{ 
    public PlayerMove playerMove;

    public BaseState(PlayerMove _playerMove)
    {
        this.playerMove = _playerMove;
    }


    
    public abstract void Update();
    public abstract void FixedUpdate();
}

public class Idle : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Idle(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Idle");
    }
    //處理每幀邏輯
    public override void Update()
    {
        //移動狀態
        if(playerMove.IsMoving())
        {
            //切換到move狀態
            playerMove.SetCurrentState(new Move(playerMove));
            
        }else if(!playerMove.IsMoving()&&playerMove.PressedJumpKey())
            {
                playerMove.SetCurrentState(new Charging(playerMove));
            }
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
public class Move : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Move(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Move");
    }
    //處理每幀邏輯
    public override void Update()
    {
        //移動狀態
        if(!playerMove.IsMoving())
        {
            //切換到MoveToIdle狀態
            playerMove.SetCurrentState(new MoveToIdle(playerMove));
        }else if(playerMove.IsMoving()&&playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
        }
        

        
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        //playerMove.MovePlayer();
        //playerMove.ApplyAirPhysicsModifiers();
    }
}
public class MoveToIdle : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public MoveToIdle(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("MoveToIdle");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("MoveToIdle"))
        {
            playerMove.SetCurrentState(new Idle(playerMove));
        }
        

        
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        //playerMove.MovePlayer();
        //playerMove.ApplyAirPhysicsModifiers();
    }
}
public class Charging : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Charging(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Charging");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("Charging")&&playerMove.ReleaseJumpKey())
        {
            playerMove.SetCurrentState(new Jump(playerMove));
        } 
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
public class Jump : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Jump(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Jump");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("Jump"))
        {
            playerMove.SetCurrentState(new Idle(playerMove));
        } 
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
public class GrappleState : BaseState
{
    private GrapplingHook hook;
    private Vector3 targetPoint;

    public GrappleState(PlayerMove _playerMove, GrapplingHook _hook, Vector3 _targetPoint) : base(_playerMove)
    {
        this.hook = _hook;
        this.targetPoint = _targetPoint;
    }

    public override void Update()
    {
        // 每一幀更新繩索視覺
        hook.UpdateGrappleVisual();

        // 跳躍鍵取消鉤索（半途跳開的操作）
        if (playerMove.PressedJumpKey())
        {
            // 給予一個向上的小跳力，方便脫離
            playerMove.GetComponent<Rigidbody>().AddForce(Vector3.up * 10f, ForceMode.Impulse);
            playerMove.SetCurrentState(new Jump(playerMove)); // 或切換到 Fall 狀態
            return;
        }

        // 到達目標點後自動脫離
        if (hook.HasReachedTarget(playerMove.transform.position))
        {
            // 到達後重置速度並切換到 Idle 或 Air 狀態
            playerMove.GetComponent<Rigidbody>().linearVelocity = Vector3.zero; 
            playerMove.SetCurrentState(new Jump(playerMove)); // 模擬到達邊緣後的慣性跳躍
        }
    }

    public override void FixedUpdate()
    {
        // 執行物理拉取
        hook.ExecutePull(playerMove.GetComponent<Rigidbody>());
    }

    // 這裡我們可以利用構造函數來做 Enter 的邏輯，或者如果 BaseState 有 Enter/Exit 方法更好
    // 由於您的 BaseState 沒有 Enter/Exit，我們在構造函數初始化，在切換到下個狀態時清理
    
    // 注意：因為 State Pattern 在切換時沒有統一的 Exit 方法，
    // 我們需要在 PlayerMove 中處理 "當狀態不再是 GrappleState 時停止鉤索"
    // 或者我們可以在這個 State 結束時手動調用 Stop。
    // 為了架構整潔，建議給 BaseState 加一個 Exit()，但為了不改動您太多底層，
    // 我們在切換狀態前調用 hook.StopGrapple()。
}

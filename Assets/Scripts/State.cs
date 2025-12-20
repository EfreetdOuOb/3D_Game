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
    public Idle(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("Idle");
    }

    public override void Update()
    {
        if (playerMove.IsMoving())
        {
            playerMove.SetCurrentState(new Move(playerMove));
            return;
        }

        

        if (!playerMove.IsMoving() && playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
        }
    }

    public override void FixedUpdate() { }
}
public class Move : BaseState
{
    public Move(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("Move");
        playerMove.PlayMoveSound();
    }

    public override void Update()
    {
        if (!playerMove.IsMoving())
        {
            playerMove.SetCurrentState(new MoveToIdle(playerMove));
            return;
        }

        

        if (playerMove.IsMoving() && playerMove.PressedJumpKey())
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
    public MoveToIdle(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("MoveToIdle");
    }

    public override void Update()
    {
        // 如果在過渡動畫中又開始移動，立刻打斷回 Move
        if (playerMove.IsMoving())
        {
            playerMove.SetCurrentState(new Move(playerMove));
            return;
        }

        if (playerMove.IsAnimationCompleted("MoveToIdle"))
        {
            playerMove.SetCurrentState(new Idle(playerMove));
        }
        if (playerMove.IsMoving() && playerMove.PressedJumpKey())
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
public class IdleToMove : BaseState
{
    public IdleToMove(PlayerMove playerMove) : base(playerMove)
    {
        playerMove.PlayAnimation("IdleToMove");
    }

    public override void Update()
    {
        // IdleToMove 播完之後正式進入 Move 狀態
        if (playerMove.IsAnimationCompleted("IdleToMove"))
        {
            playerMove.SetCurrentState(new Move(playerMove));
        }
        if (!playerMove.IsMoving() && playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
        }
    }

    public override void FixedUpdate() 
    { 

    }
}
public class IdleToCharge : BaseState
{
    public IdleToCharge(PlayerMove playerMove) : base(playerMove)
    {
        playerMove.PlayAnimation("IdleToCharge");
    }

    public override void Update()
    {
        if (playerMove.IsAnimationCompleted("IdleToCharge"))
        {
            playerMove.SetCurrentState(new Charging(playerMove));
        }
    }

    public override void FixedUpdate() { }
}


public class Charging : BaseState
{
    public Charging(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("Charging");
        playerMove.PlayChargingSound();
        
    }

    public override void Update()
    {
        // 放開跳躍鍵就進入 Jump，不必等動畫完全結束
        if (playerMove.ReleaseJumpKey())
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
    private bool hasLeftGround = false;
    public Jump(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("Jump");
        playerMove.PlayJumpSound(); // 進入跳躍時播聲音
    }

    public override void Update()
    {
        if (playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
            return;
        }
        // 1. 先等到真的離開地面一次
        if (!hasLeftGround)
        {
            if (!playerMove.IsGrounded())
                hasLeftGround = true;   // 已經離地
            return;
        }

        // 1. 優先判斷是否落地
        if (playerMove.IsGrounded())
        {
            playerMove.SetCurrentState(new Land(playerMove));
            return;
        }

        // 2. （選擇性）如果動畫播完但還在空中，可以切成一個 Loop 的空中狀態 Air
        /*
        if (playerMove.IsAnimationCompleted("Jump"))
        {
            playerMove.SetCurrentState(new Air(playerMove));
        }
        */
    }

    public override void FixedUpdate() { }
}
public class Land : BaseState
{
    public Land(PlayerMove _playerMove) : base(_playerMove)
    {
        playerMove.PlayAnimation("Land");
        playerMove.PlayLandSound();   // 落地時播聲音
    }

    public override void Update()
    { 
        if (playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
            return;
        }

        // 落地動畫播完後，看玩家有沒有在移動
        if (playerMove.IsAnimationCompleted("Land"))
        {
            if (playerMove.IsMoving())
                playerMove.SetCurrentState(new Move(playerMove));
            else
                playerMove.SetCurrentState(new Idle(playerMove));
        }
        
    }

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

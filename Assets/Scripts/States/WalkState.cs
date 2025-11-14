using UnityEngine;

/// <summary>
/// 行走狀態
/// </summary>
public class WalkState : CharacterState
{
    public override string StateName => "Walk";
    
    public override void OnEnter()
    {
        // 設置 Animator 參數
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsRunning", true);
            stateMachine.SetAnimatorBool("IsJumping", false);
        }
    }
    
    public override void OnUpdate()
    {
        // 行走狀態的更新邏輯
    }
    
    public override void OnExit()
    {
        // 退出行走狀態時的清理工作
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 行走狀態可以轉換到任何狀態
        return true;
    }
}

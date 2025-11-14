using UnityEngine;

/// <summary>
/// 跳躍狀態
/// </summary>
public class JumpState : CharacterState
{
    public override string StateName => "Jump";
    
    public override void OnEnter()
    {
        // 設置 Animator 參數
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsJumping", true);
        }
    }
    
    public override void OnUpdate()
    {
        // 跳躍狀態的更新邏輯
    }
    
    public override void OnExit()
    {
        // 退出跳躍狀態時的清理工作
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsJumping", false);
        }
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 跳躍狀態可以轉換到任何狀態
        return true;
    }
}

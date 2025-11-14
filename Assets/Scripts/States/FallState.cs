using UnityEngine;

/// <summary>
/// 下落狀態
/// </summary>
public class FallState : CharacterState
{
    public override string StateName => "Fall";
    
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
        // 下落狀態的更新邏輯
    }
    
    public override void OnExit()
    {
        // 退出下落狀態時的清理工作
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsJumping", false);
        }
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 下落狀態可以轉換到任何狀態
        return true;
    }
}

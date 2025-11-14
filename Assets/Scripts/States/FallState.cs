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
            // Fall 狀態也使用 Jump 動畫（或可以添加專門的 Fall 動畫）
            // 直接播放動畫，強制立即切換（不等待當前動畫播放完）
            stateMachine.PlayAnimation("骨架|Jump", 0.05f);
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

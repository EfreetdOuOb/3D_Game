using UnityEngine;

/// <summary>
/// 待機狀態
/// </summary>
public class IdleState : CharacterState
{
    public override string StateName => "Idle";
    
    public override void OnEnter()
    {
        // 設置 Animator 參數
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsRunning", false);
            stateMachine.SetAnimatorBool("IsJumping", false);
            // 直接播放 idle 動畫，強制立即切換（不等待當前動畫播放完）
            stateMachine.PlayAnimation("骨架|idle", 0.05f);
        }
    }
    
    public override void OnUpdate()
    {
        // 待機狀態的更新邏輯
        // 可以在這裡檢查是否需要轉換到其他狀態
    }
    
    public override void OnExit()
    {
        // 退出待機狀態時的清理工作
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 待機狀態可以轉換到任何狀態
        return true;
    }
}

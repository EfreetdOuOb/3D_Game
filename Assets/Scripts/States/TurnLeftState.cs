using UnityEngine;

/// <summary>
/// 左轉狀態
/// </summary>
public class TurnLeftState : CharacterState
{
    public override string StateName => "TurnLeft";
    
    public override void OnEnter()
    {
        // 設置 Animator 參數
        if (animator != null)
        {
            // 播放左轉動畫
            stateMachine.PlayAnimation("骨架|turnL", 0.1f);
        }
    }
    
    public override void OnUpdate()
    {
        // 左轉狀態的更新邏輯
        // 檢查動畫是否播放完成，如果完成則根據移動狀態切換
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("骨架|turnL") && stateInfo.normalizedTime >= 0.9f)
            {
                // 動畫即將完成，狀態會由 PlayerAnimationController 根據移動狀態自動切換
            }
        }
    }
    
    public override void OnExit()
    {
        // 退出左轉狀態時的清理工作
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 左轉狀態可以轉換到任何狀態
        return true;
    }
}

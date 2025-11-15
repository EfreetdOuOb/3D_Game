using UnityEngine;

/// <summary>
/// 下落狀態
/// </summary>
public class FallState : CharacterState
{
    public override string StateName => "Fall";
    
    private bool hasStoppedAtEnd = false;
    
    public override void OnEnter()
    {
        hasStoppedAtEnd = false;
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
        // 確保動畫停留在最後一幀，防止重複播放
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("骨架|Jump"))
            {
                // 如果動畫已經播放完成，確保停留在最後一幀
                if (stateInfo.normalizedTime >= 1f)
                {
                    // 持續設置為最後一幀，防止循環
                    animator.Play("骨架|Jump", 0, 1f);
                    // 設置動畫速度為0，確保不會繼續播放
                    animator.speed = 0f;
                    hasStoppedAtEnd = true;
                }
                else if (stateInfo.normalizedTime >= 0.95f)
                {
                    // 接近完成時就開始準備停留在最後一幀
                    animator.speed = 0.1f; // 減慢速度
                }
            }
        }
    }
    
    public override void OnExit()
    {
        // 退出下落狀態時的清理工作
        if (animator != null)
        {
            // 恢復動畫速度
            animator.speed = 1f;
            stateMachine.SetAnimatorBool("IsJumping", false);
            hasStoppedAtEnd = false;
        }
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 下落狀態可以轉換到任何狀態
        return true;
    }
}

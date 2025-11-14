using UnityEngine;

/// <summary>
/// 跳躍狀態
/// </summary>
public class JumpState : CharacterState
{
    private bool hasPlayedAnimation = false;
    private bool hasStoppedAtEnd = false; // 標記是否已經設置為停留在最後一幀
    
    public override string StateName => "Jump";
    
    public override void OnEnter()
    {
        // 重置標記，允許播放動畫
        hasPlayedAnimation = false;
        hasStoppedAtEnd = false;
        
        // 設置 Animator 參數
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsJumping", true);
            // 播放 Jump 動畫一次（不循環）
            stateMachine.PlayAnimationOnce("骨架|Jump", 0);
            hasPlayedAnimation = true;
        }
    }
    
    public override void OnUpdate()
    {
        // 跳躍狀態的更新邏輯
        // 如果動畫已經播放過，確保不會重複播放
        if (animator != null && hasPlayedAnimation && !hasStoppedAtEnd)
        {
            // 檢查動畫是否還在播放
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("骨架|Jump"))
            {
                // 如果動畫已經播放完成（normalizedTime >= 1），確保停留在最後一幀
                if (stateInfo.normalizedTime >= 1f)
                {
                    // 只設置一次，讓動畫停留在最後一幀，防止循環
                    animator.Play("骨架|Jump", 0, 1f);
                    hasStoppedAtEnd = true;
                }
            }
        }
    }
    
    public override void OnExit()
    {
        // 退出跳躍狀態時的清理工作
        if (animator != null)
        {
            stateMachine.SetAnimatorBool("IsJumping", false);
            hasPlayedAnimation = false;
            hasStoppedAtEnd = false;
        }
    }
    
    public override bool CanTransitionTo(CharacterState newState)
    {
        // 跳躍狀態可以轉換到任何狀態
        return true;
    }
}

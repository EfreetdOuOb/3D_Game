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
        // 確保動畫停留在最後一幀，防止重複播放
        if (animator != null && hasPlayedAnimation)
        {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("骨架|Jump"))
            {
            // 只做一次卡最後一幀
            if (!hasStoppedAtEnd && stateInfo.normalizedTime >= 1f)
                {
                animator.Play("骨架|Jump", 0, 1f);
                animator.speed = 0f;
                hasStoppedAtEnd = true;
                }
            else if (!hasStoppedAtEnd && stateInfo.normalizedTime >= 0.95f)
                {
                animator.speed = 0.1f;
                }
            }
        }
    }
    
    public override void OnExit()
    {
        // 退出跳躍狀態時的清理工作
        if (animator != null)
        {
            // 恢復動畫速度
            animator.speed = 1f;
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

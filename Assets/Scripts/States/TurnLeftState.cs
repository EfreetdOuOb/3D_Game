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
            // 如果需要特定的轉向參數，可以在這裡設置
        }
    }
    
    public override void OnUpdate()
    {
        // 左轉狀態的更新邏輯
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

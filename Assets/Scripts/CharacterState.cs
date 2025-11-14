using UnityEngine;

/// <summary>
/// 角色狀態抽象基類
/// 所有角色狀態都應該繼承此類
/// </summary>
public abstract class CharacterState
{
    protected CharacterStateMachine stateMachine;
    protected Animator animator;
    protected GameObject gameObject;
    protected Transform transform;
    
    /// <summary>
    /// 狀態名稱（用於調試）
    /// </summary>
    public abstract string StateName { get; }
    
    /// <summary>
    /// 初始化狀態
    /// </summary>
    public virtual void Initialize(CharacterStateMachine stateMachine, Animator animator, GameObject gameObject)
    {
        this.stateMachine = stateMachine;
        this.animator = animator;
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
    }
    
    /// <summary>
    /// 進入狀態時調用
    /// </summary>
    public abstract void OnEnter();
    
    /// <summary>
    /// 每幀更新時調用
    /// </summary>
    public abstract void OnUpdate();
    
    /// <summary>
    /// 固定更新時調用（物理更新）
    /// </summary>
    public virtual void OnFixedUpdate() { }
    
    /// <summary>
    /// 退出狀態時調用
    /// </summary>
    public abstract void OnExit();
    
    /// <summary>
    /// 檢查是否可以轉換到其他狀態
    /// </summary>
    public abstract bool CanTransitionTo(CharacterState newState);
}

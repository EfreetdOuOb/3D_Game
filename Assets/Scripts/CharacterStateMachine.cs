using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 角色狀態機管理器
/// 負責管理角色狀態轉換和動畫播放
/// </summary>
public class CharacterStateMachine : MonoBehaviour
{
    [Header("動畫設定")]
    [Tooltip("角色的 Animator 組件")]
    public Animator animator;
    
    [Header("狀態設定")]
    [Tooltip("初始狀態類型")]
    public StateType initialState = StateType.Idle;
    
    // 狀態字典
    private Dictionary<StateType, CharacterState> states = new Dictionary<StateType, CharacterState>();
    private CharacterState currentState;
    private CharacterState previousState;
    
    // 狀態類型枚舉
    public enum StateType
    {
        Idle,
        Walk,
        Jump,
        Fall,
        TurnLeft,
        TurnRight
    }
    
    void Start()
    {
        // 如果沒有指定 Animator，嘗試自動獲取
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // 初始化所有狀態
        InitializeStates();
        
        // 設置初始狀態
        if (states.ContainsKey(initialState))
        {
            ChangeState(initialState);
        }
        else
        {
            Debug.LogError($"找不到初始狀態: {initialState}");
        }
    }
    
    void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
    }
    
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.OnFixedUpdate();
        }
    }
    
    /// <summary>
    /// 初始化所有狀態
    /// </summary>
    private void InitializeStates()
    {
        // 創建並初始化各個狀態
        states[StateType.Idle] = new IdleState();
        states[StateType.Walk] = new WalkState();
        states[StateType.Jump] = new JumpState();
        states[StateType.Fall] = new FallState();
        states[StateType.TurnLeft] = new TurnLeftState();
        states[StateType.TurnRight] = new TurnRightState();
        
        // 初始化每個狀態
        foreach (var state in states.Values)
        {
            state.Initialize(this, animator, gameObject);
        }
    }
    
    /// <summary>
    /// 切換到指定狀態
    /// </summary>
    public void ChangeState(StateType newStateType)
    {
        if (!states.ContainsKey(newStateType))
        {
            Debug.LogWarning($"狀態不存在: {newStateType}");
            return;
        }
        
        CharacterState newState = states[newStateType];
        
        // 如果已經是目標狀態，不需要切換
        if (currentState == newState)
        {
            return;
        }
        
        // 檢查是否可以轉換
        if (currentState != null && !currentState.CanTransitionTo(newState))
        {
            return;
        }
        
        // 退出當前狀態
        if (currentState != null)
        {
            currentState.OnExit();
            previousState = currentState;
        }
        
        // 進入新狀態
        currentState = newState;
        currentState.OnEnter();
        
        // 強制 Animator 立即更新，確保動畫立即切換
        if (animator != null && animator.isInitialized)
        {
            animator.Update(0f);
        }
        
        Debug.Log($"狀態轉換: {previousState?.StateName ?? "None"} -> {currentState.StateName}");
    }
    
    /// <summary>
    /// 獲取當前狀態
    /// </summary>
    public CharacterState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 獲取當前狀態類型
    /// </summary>
    public StateType GetCurrentStateType()
    {
        foreach (var kvp in states)
        {
            if (kvp.Value == currentState)
            {
                return kvp.Key;
            }
        }
        return initialState;
    }
    
    /// <summary>
    /// 獲取指定類型的狀態
    /// </summary>
    public CharacterState GetState(StateType stateType)
    {
        return states.ContainsKey(stateType) ? states[stateType] : null;
    }
    
    /// <summary>
    /// 設置 Animator 參數（立即更新，不等待動畫播放完）
    /// </summary>
    public void SetAnimatorBool(string parameterName, bool value, bool forceUpdate = false)
    {
        if (animator != null && animator.isInitialized)
        {
            // 檢查參數是否已經是指定值，避免不必要的更新
            bool currentValue = animator.GetBool(parameterName);
            if (currentValue != value || forceUpdate)
            {
                animator.SetBool(parameterName, value);
                // 只在參數改變時才強制更新，避免頻繁更新導致抽搐
                if (forceUpdate)
                {
                    animator.Update(0f);
                }
            }
        }
    }
    
    /// <summary>
    /// 設置 Animator 參數
    /// </summary>
    public void SetAnimatorFloat(string parameterName, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(parameterName, value);
        }
    }
    
    /// <summary>
    /// 設置 Animator 參數
    /// </summary>
    public void SetAnimatorInt(string parameterName, int value)
    {
        if (animator != null)
        {
            animator.SetInteger(parameterName, value);
        }
    }
    
    /// <summary>
    /// 設置 Animator 觸發器
    /// </summary>
    public void SetAnimatorTrigger(string parameterName)
    {
        if (animator != null)
        {
            animator.SetTrigger(parameterName);
        }
    }
    
    /// <summary>
    /// 直接播放動畫（強制立即切換，不等待當前動畫）
    /// </summary>
    public void PlayAnimation(string stateName, float normalizedTransitionTime = 0.1f, int layer = 0)
    {
        if (animator != null && animator.isInitialized)
        {
            // 使用 Play 方法直接播放動畫，完全繞過過渡系統和 Has Exit Time
            // 這樣可以確保動畫立即切換，不等待當前動畫播放完
            animator.Play(stateName, layer, 0f);
            // 強制立即更新，確保動畫立即切換
            animator.Update(0f);
        }
    }
    
    /// <summary>
    /// 播放動畫一次（不循環，播放完成後保持在最後一幀）
    /// </summary>
    public void PlayAnimationOnce(string stateName, int layer = 0)
    {
        if (animator != null && animator.isInitialized)
        {
            // 使用 Play 方法播放動畫，從開始播放
            animator.Play(stateName, layer, 0f);
            // 獲取動畫狀態信息
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            // 設置動畫速度為正常速度（確保播放）
            animator.speed = 1f;
            // 強制立即更新
            animator.Update(0f);
        }
    }
    
    /// <summary>
    /// 檢查指定動畫是否正在播放
    /// </summary>
    public bool IsAnimationPlaying(string stateName, int layer = 0)
    {
        if (animator != null && animator.isInitialized)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.IsName(stateName);
        }
        return false;
    }
    
    /// <summary>
    /// 獲取當前動畫的播放進度（0-1）
    /// </summary>
    public float GetAnimationNormalizedTime(int layer = 0)
    {
        if (animator != null && animator.isInitialized)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.normalizedTime;
        }
        return 0f;
    }
    
    /// <summary>
    /// 立即播放動畫（無過渡，瞬間切換）
    /// </summary>
    public void PlayAnimationImmediate(string stateName, int layer = 0)
    {
        if (animator != null && animator.isInitialized)
        {
            animator.Play(stateName, layer, 0f);
            // 強制立即更新
            animator.Update(0f);
        }
    }
}

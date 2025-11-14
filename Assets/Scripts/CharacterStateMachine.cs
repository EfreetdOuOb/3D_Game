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
    /// 設置 Animator 參數
    /// </summary>
    public void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(parameterName, value);
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
}

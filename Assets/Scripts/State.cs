public abstract class BaseState
{ 
    public PlayerMove playerMove;

    public BaseState(PlayerMove _playerMove)
    {
        this.playerMove = _playerMove;
    }


    
    public abstract void Update();
    public abstract void FixedUpdate();
}

public class Idle : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Idle(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Idle");
    }
    //處理每幀邏輯
    public override void Update()
    {
        //移動狀態
        if(playerMove.IsMoving())
        {
            //切換到move狀態
            playerMove.SetCurrentState(new Move(playerMove));
            
        }else if(!playerMove.IsMoving()&&playerMove.PressedJumpKey())
            {
                playerMove.SetCurrentState(new Charging(playerMove));
            }
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
public class Move : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Move(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Move");
    }
    //處理每幀邏輯
    public override void Update()
    {
        //移動狀態
        if(!playerMove.IsMoving())
        {
            //切換到MoveToIdle狀態
            playerMove.SetCurrentState(new MoveToIdle(playerMove));
        }else if(playerMove.IsMoving()&&playerMove.PressedJumpKey())
        {
            playerMove.SetCurrentState(new Charging(playerMove));
        }
        

        
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        //playerMove.MovePlayer();
        //playerMove.ApplyAirPhysicsModifiers();
    }
}
public class MoveToIdle : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public MoveToIdle(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("MoveToIdle");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("MoveToIdle"))
        {
            playerMove.SetCurrentState(new Idle(playerMove));
        }
        

        
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        //playerMove.MovePlayer();
        //playerMove.ApplyAirPhysicsModifiers();
    }
}
public class Charging : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Charging(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Charging");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("Charging")&&playerMove.ReleaseJumpKey())
        {
            playerMove.SetCurrentState(new Jump(playerMove));
        } 
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
public class Jump : BaseState
{
    //構造函數，類在被實例化時的初始化方法，執行一次
    public Jump(PlayerMove _playerMove) : base(_playerMove)
    {
        //播放動畫
        playerMove.PlayAnimation("Jump");
    }
    //處理每幀邏輯
    public override void Update()
    {
        if(playerMove.IsAnimationCompleted("Jump"))
        {
            playerMove.SetCurrentState(new Idle(playerMove));
        } 
    }
    //處理人物運動
    public override void FixedUpdate()
    {
        
    }
}
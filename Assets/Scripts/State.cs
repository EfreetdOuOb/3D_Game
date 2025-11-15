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
            //切換到idle狀態
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
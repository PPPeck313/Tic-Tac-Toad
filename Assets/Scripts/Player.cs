public class Player
{
    
//==============================================================================
// Fields
//==============================================================================

    public int id;
    public PlayerType playerType;
    
//==============================================================================
// Constructor
//==============================================================================

    public Player(int id, PlayerType playerType)
    {
        this.id = id;
        this.playerType = playerType;
    }
}

public enum PlayerType
{
    Human,
    AI
}

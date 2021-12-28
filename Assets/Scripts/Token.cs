using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Token : MonoBehaviour
{

    private static readonly string[] PlayerSprites = {Resources.P1Sprite,  Resources.P2Sprite};

//==============================================================================
// Fields
//==============================================================================

    private SpriteRenderer _spriteRenderer;
    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
    
    private Board _board;
    
    public int id;
    public int playerOwner;
    public bool isDisabled;

//==============================================================================
// Constructor
//==============================================================================

    public Token Init(int id, Board board)
    {
        this.id = id;
        _board = board;
        return this;
    }

//==============================================================================
// Lifecycle
//==============================================================================

    private void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Reset();
    }
    
    private void Start()
    {
        InitSprites(Resources.TokenLabel);
    }
    
//==============================================================================
// Initialize
//==============================================================================

    public void Reset()
    {
        _spriteRenderer.sprite = null;
        playerOwner = -1;
        isDisabled = false;
    }
    
    private async Task InitSprites(string assetLabel)
    {
        var locations = await Addressables.LoadResourceLocationsAsync(assetLabel, typeof(Sprite)).Task;
        var tasks = locations.Select(location => Addressables.LoadAssetAsync<Sprite>(location).Task).ToList();
        var loadedSprites = await Task.WhenAll(tasks);

        foreach (var sprite in loadedSprites)
        {
            _sprites.Add(sprite.name, sprite);
        }
    }

//==============================================================================
// Listeners
//==============================================================================

    public void OnMouseDown() 
    {
        if (!isDisabled 
            && !_board.isDisabled
            && _sprites.Any())
        {
            TakeTurn();
        }
    }

    public void TakeTurn()
    {
        playerOwner = _board.GetPlayerTurn();
        _spriteRenderer.sprite = _sprites[PlayerSprites[playerOwner]];
        isDisabled = true;
            
        _board.FinishTurn(id);
    }
}

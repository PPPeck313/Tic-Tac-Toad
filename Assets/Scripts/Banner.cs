using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class Banner : MonoBehaviour 
{
    
//==============================================================================
// Constants
//==============================================================================

    private const string BannerLabel = "banner";
    
    private const string WinSprite = "win";
    private const string LoseSprite = "lose";
    
//==============================================================================
// Fields
//==============================================================================

    private SpriteRenderer _spriteRenderer;
    
    private Board _board;
    private bool _hasWon;

    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
    
//==============================================================================
// Constructor
//==============================================================================

    public Banner Init(Board board, bool hasWon)
    {
        _board = board;
        _hasWon = hasWon;
        return this;
    }
    
//==============================================================================
// Lifecycle
//==============================================================================

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = null;
    }
    
    private async void Start()
    {
        await InitSprites(BannerLabel);
        _spriteRenderer.sprite = _sprites[_hasWon ? WinSprite : LoseSprite];
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

    private void OnMouseDown() 
    {
        _board.ResetGame();
    }
}
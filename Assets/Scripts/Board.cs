using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour 
{

//==============================================================================
// Constants
//==============================================================================

    private const int Rows = 3;
    private const int Columns = 3;
    private const int InARowRequired = 3;

    private static readonly PlayerType[] Players = { PlayerType.Human,  PlayerType.AI };
    private const int AIDelay = 2;
    
    public const int OneBasedIndexing = 1;
    
    public bool isDisabled;

//==============================================================================
// Fields
//==============================================================================
    
    private SpriteRenderer _spriteRenderer;
    private Bounds _bounds;

    private Banner _banner;
    private readonly List<Token> _tokens = new List<Token>();
    
    private int _turn;
    private readonly Dictionary<int, List<List<int>>> _victoryRoutes = new Dictionary<int, List<List<int>>>();

    private List<Player> _players = new List<Player>();

    private MoveController _moveController;
    
//==============================================================================
// Lifecycle
//==============================================================================

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _bounds = _spriteRenderer.bounds;

        GeneratePlayers();
        GenerateTokens();
        GenerateRoutes();
        
        _moveController = new MoveController(Rows, Columns, InARowRequired, 
            _tokens, 
            _victoryRoutes,
            _players);
    }
    
//==============================================================================
// Initialize
//==============================================================================

    private void GeneratePlayers()
    {
        for (var i = 0; i < Players.Length; i++)
        {
            _players.Add(new Player(i, Players[i]));
        }
    }
    
    private void GenerateTokens() 
    {
        const int numTokens = Rows * Columns;
        const float middleRow = Rows - (Rows / 2);
        const float middleColumn = Columns - (Columns / 2);
    
        for (var i = 0; i < numTokens; i++) {
            var vector = new Vector3(0, 0, 2);
            
            var row = (i / Rows) + OneBasedIndexing;
            var column = (i % Columns) + OneBasedIndexing;

            vector.y = (middleRow - row) * 3f;
            vector.x = (column - middleColumn) * 3.33f;
            
            _tokens.Add(ObjectFactory.CreateToken(i + OneBasedIndexing, this, vector));
        }
    }

    private void GenerateRoutes() 
    {
        for (var r = 0; r < Rows; r++) {
            for (var c = 0; c < Columns; c++) {
                var currentWrappedIndex = StartAtRow(r) + c;
            


                var verticalRoute = new List<int>();

                for (var rr = 0; rr < Rows; rr++) 
                {
                    verticalRoute.Add(StartAtRow(rr) + c);
                }



                var horizontalRoute = new List<int>();

                for (var cc = 0; cc < Columns; cc++) 
                {
                    horizontalRoute.Add(StartAtRow(r) + cc);
                }



                var diagonalTlbrRoute = new List<int>();

                var backwards = Math.Min(r, c);
                var dr = r - backwards;
                var dc = c - backwards;

                var forwards = Math.Min(Rows - dr, Columns - dc);

                if (forwards >= InARowRequired) 
                {
                    for (int s = 0; s < forwards; s++) {
                        diagonalTlbrRoute.Add(StartAtRow(dr + s) + (dc + s));
                    }
                }



                var diagonalBltrRoute = new List<int>();

                backwards = Math.Min((Rows - OneBasedIndexing) - r, c);
                dr = r + backwards;
                dc = c - backwards;

                forwards = Math.Min(dr + OneBasedIndexing, Columns - dc);

                if (forwards >= InARowRequired) 
                {
                    for (var s = 0; s < forwards; s++) {
                        diagonalBltrRoute.Add(StartAtRow(dr - s) + (dc + s));
                    }
                }



                _victoryRoutes.Add(currentWrappedIndex, new List<List<int>>
                {
                    verticalRoute,
                    horizontalRoute,
                    diagonalTlbrRoute,
                    diagonalBltrRoute
                });
                
                _victoryRoutes[currentWrappedIndex].RemoveAll(item => !item.Any());
            }
        }
    }
    
    private static int StartAtRow(int row)
    {
        return row * Columns + OneBasedIndexing;
    }

//==============================================================================
// Methods
//==============================================================================

    public int GetPlayerTurn()
    {
        return _turn % _players.Count;
    }
    
    private bool IsBoardExhausted()
    {
        return _tokens.All(token => token.playerOwner > -1);
    }

    private void DisableTokens()
    {
        foreach (var token in _tokens)
        {
            token.isDisabled = true;
        }
    }

    
    
    public void FinishTurn(int index)
    {
        var playerId = GetPlayerTurn();
        
        if (_moveController.CheckWinCondition(index, GetPlayerTurn(), InARowRequired) != null)
        {
            DisableTokens();
            _banner = ObjectFactory.CreateBanner(this, _players[playerId].playerType != PlayerType.AI);
        }

        else if (IsBoardExhausted())
        {
            _banner = ObjectFactory.CreateBanner(this);
        }

        else
        {
            _turn += 1;
            var newPlayerId = GetPlayerTurn();

            if (_players[newPlayerId].playerType == PlayerType.AI)
            {
                StartCoroutine(TakeAITurn(newPlayerId));
            }
        }
    }

    private IEnumerator TakeAITurn(int playerId)
    {
        isDisabled = true;

        yield return new WaitForSeconds(AIDelay);

        var token = _moveController.GetWinningMove(playerId);
        token = token == null ? _moveController.GetDefendingMove(playerId) : token;
        token = token == null ? _tokens[Random.Range(0, Rows * Columns)] : token;
        
        while (true)
        {
            if (token.playerOwner == -1)
            {
                token.TakeTurn();
                isDisabled = false;
                break;
            }

            token = _tokens[Random.Range(0, Rows * Columns)];
        }
    }

    public void ResetGame()
    {
        foreach (var token in _tokens)
        {
            token.Reset();
        }
        
        _turn = 0;
        Destroy(_banner.gameObject);
    }
}
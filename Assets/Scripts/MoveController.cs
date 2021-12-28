using System;
using System.Collections.Generic;
using System.Linq;

public class MoveController
{
//==============================================================================
// Fields
//==============================================================================

    private readonly int _rows;
    private readonly int _columns;
    private readonly int _inARowRequired;
    
    private readonly List<Token> _tokens;
    
    private readonly Dictionary<int, List<List<int>>> _victoryRoutes;
    
    private readonly List<Player> _players;
    
//==============================================================================
// Constructor
//==============================================================================

    public MoveController(int rows, int columns, int inARowRequired, 
        List<Token> tokens, 
        Dictionary<int, List<List<int>>> victoryRoutes,
        List<Player> players)
    {
        _rows = rows;
        _columns = columns;
        _inARowRequired = inARowRequired;
        
        _tokens = tokens;
    
        _victoryRoutes = victoryRoutes;

        _players = players;
    }

//==============================================================================
// Methods
//==============================================================================

    public List<int> CheckWinCondition(int playerId, int required)
    {
        for (var i = 1; i < _rows * _columns + Board.OneBasedIndexing; i++)
        {
            var indexVictoryRoute = CheckWinCondition(i, playerId, required);

            if (indexVictoryRoute != null)
            {
                return indexVictoryRoute;
            }
        }

        return null;
    }
    
    public List<int> CheckWinCondition(int index, int playerId, int required) 
    {
        var indexVictoryRoutes = _victoryRoutes[index];

        foreach (var indexVictoryRoute in indexVictoryRoutes) 
        {
            var numContiguousTokens = 0;
            
            var allowedBreaks = _inARowRequired - required;
            var breaks = 0;
            
            foreach (var token in indexVictoryRoute.Select(id => _tokens[id - Board.OneBasedIndexing]))
            {
                if (token.playerOwner == playerId)
                {
                    numContiguousTokens += 1;
                }

                if (token.playerOwner == -1)
                {
                    breaks += 1;
                }
                
                if (breaks > allowedBreaks)
                {
                    numContiguousTokens = 0;
                    breaks = 1;
                }

                if (numContiguousTokens + breaks == _inARowRequired)
                {
                    return indexVictoryRoute;
                }
            }
        }

        return null;
    }
    
    public Token GetWinningMove(int playerId)
    {
        int required = _inARowRequired - 1;
        var indexVictoryRoute = CheckWinCondition(playerId, required);

        return indexVictoryRoute != null ? GetMove(indexVictoryRoute, playerId, required) : null;
    }

    public Token GetDefendingMove(int playerId)
    {
        var players = _players.FindAll(player => player.id != playerId);
        return players.Select(player => GetWinningMove(player.id)).FirstOrDefault(token => token != null);
    }

    private Token GetMove(IReadOnlyList<int> indexVictoryRoute, int playerId, int required)
    {
        var numContiguousTokensForwards = 0;
        var numContiguousTokensBackwards = 0;
        
        var allowedBreaks = _inARowRequired - required;
        var breaksForwards = 0;
        var breaksBackwards = 0;

        for (var i = 0; i < indexVictoryRoute.Count; i++)
        {
            var tokenForwards = _tokens[indexVictoryRoute[i] - Board.OneBasedIndexing];
            numContiguousTokensForwards += Convert.ToInt32(tokenForwards.playerOwner == playerId);

            if (numContiguousTokensForwards == required)
            {
                var fillGap = breaksForwards > 0;
                var nextIndex = i + (fillGap ? (breaksForwards * -1) : 1);
                return _tokens[indexVictoryRoute[nextIndex] - Board.OneBasedIndexing];
            }

            if (tokenForwards.playerOwner == -1)
            {
                breaksForwards += 1;
            }
            
            if (breaksForwards > allowedBreaks)
            {
                numContiguousTokensForwards = 0;
                breaksForwards = 1;
            }

            var indexBackwards = indexVictoryRoute.Count - 1 - i;
            var tokenBackwards = _tokens[indexVictoryRoute[indexBackwards] - Board.OneBasedIndexing];
            numContiguousTokensBackwards += Convert.ToInt32(tokenBackwards.playerOwner == playerId);
            
            if (numContiguousTokensBackwards == required)
            {
                var fillGap = breaksBackwards > 0;
                var nextIndex = i + (fillGap ? breaksBackwards : -1);
                return _tokens[indexVictoryRoute[nextIndex] - Board.OneBasedIndexing];
            }

            if (tokenBackwards.playerOwner == -1)
            {
                breaksBackwards += 1;
            }
            
            if (breaksBackwards > allowedBreaks)
            {
                numContiguousTokensBackwards = 0;
                breaksBackwards = 1;
            }
        }

        return null;
    }
}
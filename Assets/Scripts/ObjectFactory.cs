using UnityEngine;

public class ObjectFactory : MonoBehaviour {

//==============================================================================
// Fields
//==============================================================================

    private static ObjectFactory _instance;
    
    public GameObject token;
    public GameObject banner;

//==============================================================================
// Lifecycle
//==============================================================================

    private void Awake() {
        _instance = this;
    }
    
//==============================================================================
// Methods
//==============================================================================

    public static Banner CreateBanner(Board board, bool hasWon = false, Vector3 vector = new Vector3()) {
        var banner = Instantiate(_instance.banner, vector, Quaternion.identity).GetComponent<Banner>();
        return banner.Init(board, hasWon);
    }
    
    public static Token CreateToken(int id, Board board, Vector3 vector = new Vector3()) {
        var token = Instantiate(_instance.token, vector, Quaternion.identity).GetComponent<Token>();
        return token.Init(id, board);
    }
}

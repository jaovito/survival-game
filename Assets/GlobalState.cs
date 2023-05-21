using UnityEngine;

[CreateAssetMenu(menuName = "Global State")]
public class GlobalState : MonoBehaviour
{    
    private static GlobalState _instance;
    public static GlobalState Instance { get { return _instance; } }

    public float points;
    public float playerHealth;

    public GameObject player;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void DestroyPlayer()
    {
        Destroy(player);
    }
}
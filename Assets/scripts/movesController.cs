using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class movesController : MonoBehaviour
{
    // Start is called before the first frame update

    private int moves = 344;
    
    void Start()
    {
        Text text = this.gameObject.GetComponent<Text>();
        text.text = ""+ moves;
    }

    public void reduceMovesByOne()
    {
        moves--;
        
        Text text = this.gameObject.GetComponent<Text>();
        text.text = ""+ moves;
        
        if (moves == 0)
        {
            BlockSpawner blockSpawner = GameObject.Find("spawner").GetComponent<BlockSpawner>();
            blockSpawner.SetGameOver(true);
        }
    }
    
}

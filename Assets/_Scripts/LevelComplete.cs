using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelComplete : MonoBehaviour
{
    public string playerTag = "Player";

    private GameManager manager;
    private void Start()
    {
        var managerObj = GameObject.Find("GameManager");
        if(managerObj != null)
            manager = managerObj.GetComponent<GameManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log("Level Beat");
            manager.state = GameManager.GameState.Beatlevel;
        }
    }
}

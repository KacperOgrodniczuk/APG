using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarTrigger : MonoBehaviour
{
    //trigger for the player to be forced to walk to the next room if he walks into the collider and then close the ground behind him

    private EnvironmentGenerator envGen;
    private EnemySpawner spawner;
    private TwitchChat chatPolls;

    private void Start()
    {
        envGen = GameObject.FindWithTag("envGen").GetComponent<EnvironmentGenerator>();
        spawner = GameObject.FindWithTag("Spawner").GetComponent<EnemySpawner>();
        chatPolls = GameObject.FindWithTag("Polls").GetComponent<TwitchChat>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the player enters the trigger
        if (collision.gameObject.CompareTag("Player"))
        {
            //teleport the player int the next room
            Vector3 tp = transform.position;
            tp.x += 2;
            tp.y += 2;
            collision.transform.position = tp;

            //close the room and update the camera bounds regardless of what the next room should be
            envGen.CloseRoom();
            envGen.UpdateCameraBounds();

            chatPolls.blockPollRunning = false;
            

            //if shop room
            //do whatever needs to be done in shop room
            if (envGen.roomNumber % 5 == 0)
                chatPolls.StartCoroutine("ShopPoll");
            //else start spawning enemies
            else
            {
                if (envGen.roomType == RoomType.Normal)
                {
                    spawner.StartCoroutine(spawner.Spawn());
                }
                else if (envGen.roomType == RoomType.Interactive)
                {
                    chatPolls.StartCoroutine("InteractiveRoomPoll");
                    envGen.OpenRoom(envGen.ReturnNextOffset());
                }
            }
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TwitchChat : MonoBehaviour
{
    //This script is responsible for the twitch chat and the voting system including the ui elements 
    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    //bool for the routines, if true play version with positive interactions if false play negative interacitons
    public static GameMode gamemode = GameMode.positive;

    [SerializeField] private string username, password, channelName; // Get the password from https://twitchapps.com/tmi

    [SerializeField] Text voteText1;
    [SerializeField] Text voteText2;
    [SerializeField] Text voteText3;
    [SerializeField] TMP_Text voteCountAText;
    [SerializeField] TMP_Text voteCountBText;
    [SerializeField] TMP_Text voteCountCText;
    [SerializeField] Text currentMod;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerSpeech playerSpeech;
    [SerializeField] EnvironmentGenerator envGen;

    //variables for normal poll
    private float timeBetweenPolls = 30f;
    int voteCountA, voteCountB, voteCountC;
    string outcome;

    //variables for shop poll
    private float shopPollTime = 10f;
    int shopVoteCountA, shopVoteCountB, shopVoteCountC;
    string shopOutcome;
    string aItem, bItem, cItem = null;

    //variables for interactive block poll
    private float blockPollTime = 5f;
    int blockVoteA, blockVoteB, blockVoteC;
    string blockOutcome;
    public bool blockPollRunning = false;

    //multipliers for all the stats, this should probably be adjusted better but for now use a single value for most increases and another for most stat decreases
    float statIncreaseMultiplier = 1.5f;
    float statDecreaseMultiplier = 0.75f;

    private void Awake()
    {
        username = TwitchLoginDetails.Username;
        channelName = TwitchLoginDetails.Username;
        password = TwitchLoginDetails.OAuthToken;
    }

    private void Start()
    {
        PopulatePollList();
        Connect();
        StartCoroutine("PollRoutine");
    }

    private void Update()
    {
        if (!twitchClient.Connected)
        {
            Connect();
        }

        ReadChat();
    }

    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
    }

    private void ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            string message = reader.ReadLine();
            Debug.Log(message);

            //If it is a chat message parse it
            if (message.Contains("PRIVMSG"))
            {
                //Get username
                var splitPoint = message.IndexOf("!", 1);
                var chatName = message.Substring(0, splitPoint);
                chatName = chatName.Substring(1);

                //Get user message
                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);
                print(String.Format("{0}: {1}", chatName, message));

                if (message.ToLower().Contains("votea")) voteCountA++;
                else if (message.ToLower().Contains("voteb")) voteCountB++;
                else if (message.ToLower().Contains("votec")) voteCountC++;
                else if (aItem != null && message.ToLower().Contains(aItem)) shopVoteCountA++;
                else if (aItem != null && message.ToLower().Contains(bItem)) shopVoteCountB++;
                else if (aItem != null && message.ToLower().Contains(cItem)) shopVoteCountC++;
                else if (message.ToLower().Contains("left")) blockVoteA++;
                else if (message.ToLower().Contains("middle")) blockVoteB++;
                else if (message.ToLower().Contains("right")) blockVoteC++;
            }

            //update the numbers to player can see votes in real time.

            voteCountAText.text = "A. " + voteCountA.ToString();
            voteCountBText.text = "B. " + voteCountB.ToString();
            voteCountCText.text = "C. " + voteCountC.ToString();
        }
    }

    //Poll List
    List<string> polls = new List<String>();

    //Funciton to populate poll list
    void PopulatePollList()
    {
        polls.Clear();

        if (gamemode == GameMode.positive)
        {
            //player related polls
            polls.Add("Increase player damage");
            polls.Add("Increase player movement speed");
            polls.Add("Increase player roll distance");
            polls.Add("Increase player block duration");
            polls.Add("Increase player attack speed");
            polls.Add("PLAYER BUFF");

            //Enemy related polls
            polls.Add("Increase enemy spawn time");
            polls.Add("Slower enemy movement speed");
            polls.Add("Reduced enemy damage");
            polls.Add("Slower enemy attacks");
            polls.Add("ENEMY DEBUFF");

            //mix of both
            polls.Add("EASY MODE");
        }
        else if (gamemode == GameMode.negative)
        {
            //player polls
            polls.Add("Reduce player damage");
            polls.Add("Reduce player movement speed");
            polls.Add("Reduce player roll distance");
            polls.Add("Reduce player block duration");
            polls.Add("Reduce player attack speed");
            polls.Add("PLAYER DEBUFF");

            //enemy polls
            polls.Add("Reduce enemy spawn time");
            polls.Add("Faster enemy movement speed");
            polls.Add("Increased enemy damage");
            polls.Add("Faster enemy attacks");
            polls.Add("ENEMY BUFF");

            //mix of both
            polls.Add("HARD MODE");
        }
    }

    //function to call poll function according to the poll outcome
    void PollFunc(string pollOutcome)
    {
        switch (pollOutcome)
        {
            //positive outcomes
            //player cases
            case "Increase player damage":
                IncreaseAttack();
                break;
            case "Increase player movement speed":
                IncreaseMovementSpeed();
                break;
            case "Increase player roll distance":
                IncreaseRollDistance();
                break;
            case "Increase player block duration":
                IncreaseBlockDuration();
                break;
            case "Increase player attack speed":
                IncreaseAttackSpeed();
                break;
            case "PLAYER BUFF":
                MegaBuff();
                break;
            //enemy cases
            case "Increase enemy spawn time":
                IncreaseEnemySpawnTime();
                break;
            case "Slower enemy movement speed":
                DecreaseEnemyMovementSpeed();
                break;
            case "Reduced enemy damage":
                DecreaseEnemyDamage();
                break;
            case "Slower enemy attacks":
                DecreaseEnemyAttackSpeed();
                break;
            case "ENEMY DEBUFF":
                MakeEnemiesWeak();
                break;
            //mix of both cases
            case "EASY MODE":
                EasyMode();
                break;

            //negative outcomes
            case "Reduce player damage":
                DecreaseAttack();
                break;
            case "Reduce player movement speed":
                DecreaseMovementSpeed();
                break;
            case "Reduce player roll distance":
                DecreaseRollDistance();
                break;
            case "Reduce player block duration":
                DecreaseBlockDuration();
                break;
            case "Reduce player attack speed":
                DecreaseAttackSpeed();
                break;
            case "PLAYER DEBUFF":
                MegaDeBuff();
                break;
            case "Reduce enemy spawn time":
                DecreaseEnemySpawnTime();
                break;
            case "Faster enemy movement speed":
                IncreaseEnemyMovementSpeed();
                break;
            case "Increased enemy damage":
                IncreaseEnemyDamage();
                break;
            case "Faster enemy attacks":
                IncreaseEnemyAttackSpeed();
                break;
            case "ENEMY BUFF":
                MakeEnemiesStrong();
                break;
            case "HARD MODE":
                HardMode();
                break;
        }
    }

    public IEnumerator InteractiveRoomPoll()    //Poll for dropping the blocks
    {
        blockPollRunning = true;
        if (gamemode == GameMode.positive) playerSpeech.SetText("Help me climb up by dropping blocks, type 'left', 'right' or 'middle' in chat!");
        else if (gamemode == GameMode.negative) playerSpeech.SetText("I dare you to crush me, Type 'left', 'right' or 'middle' in chat!");

        GameObject[] blockSpawners = envGen.ReturnInstancedBlockSpawners();
        while (blockPollRunning)
        {
            blockVoteA = 0;
            blockVoteB = 0;
            blockVoteC = 0;

            yield return new WaitForSeconds(blockPollTime);

            //Determine interactive room poll outcome
            if (blockVoteA == blockVoteB && blockVoteB == blockVoteC && blockVoteC == blockVoteA)
            {
                // randomise the outcome if all have same number of votes
                int x = Random.Range(0, 3);
                if (x == 0) blockOutcome = "left";
                if (x == 1) blockOutcome = "middle";
                if (x == 2) blockOutcome = "right";
            }
            else
            {
                //if a has more votes than b
                if (blockVoteA > blockVoteB)
                {
                    //if a has the same amount of votes as c choose randomly between a and c
                    if (blockVoteA == blockVoteC)
                    {
                        int x = Random.Range(0, 2);
                        if (x == 0) blockOutcome = "left";
                        if (x == 1) blockOutcome = "right";
                    }
                    // if a has more votes than c then choose a
                    else if (blockVoteA > blockVoteC) blockOutcome = "left";
                    else blockOutcome = "right";
                }
                else
                {
                    // if a has the same amount of votes as b choose randomly
                    if (blockVoteA == blockVoteB)
                    {
                        int x = Random.Range(0, 2);
                        if (x == 0) blockOutcome = "left";
                        if (x == 1) blockOutcome = "middle";
                    }
                    // if b has more votes than c choose b
                    if (blockVoteB > blockVoteC) blockOutcome = "middle";
                    else blockOutcome = "right";
                }
            }
            if (blockOutcome == "left") blockSpawners[0].GetComponent<BlockSpawner>().SpawnBlock();
            else if (blockOutcome == "middle") blockSpawners[1].GetComponent<BlockSpawner>().SpawnBlock();
            else if (blockOutcome == "right") blockSpawners[2].GetComponent<BlockSpawner>().SpawnBlock();
            Debug.Log("Votes left middle right      " + blockVoteA + " " + blockVoteB + " " + blockVoteC);
        }

        playerSpeech.SetText("");
        foreach (GameObject bs in blockSpawners)
        {
            Destroy(bs);
        }
        Destroy(GameObject.FindWithTag("TempTP"));
    }

    public IEnumerator ShopPoll()   //Poll for the shop to select which item
    {
        //reset the votes
        shopVoteCountA = 0;
        shopVoteCountB = 0;
        shopVoteCountC = 0;

        if (gamemode == GameMode.positive) playerSpeech.SetText("Help me choose a good item by typing its name in chat!");
        else if (gamemode == GameMode.negative) playerSpeech.SetText("Choose the worst item by typing its name in chat!");

        GameObject[] items = envGen.ReturnInstancedItems();

        aItem = items[0].GetComponent<ItemScript>().itemName.text.ToLower();    //we convert every string to lower because my votes wouldn't register cause of capitalizaiton in items names
        bItem = items[1].GetComponent<ItemScript>().itemName.text.ToLower();
        cItem = items[2].GetComponent<ItemScript>().itemName.text.ToLower();

        yield return new WaitForSeconds(shopPollTime);

        //Determine shop poll outcome
        if (shopVoteCountA == shopVoteCountB && shopVoteCountB == shopVoteCountC && shopVoteCountC == shopVoteCountA)
        {
            //outcome = "null"; // randomise the outcome if all have same number of votes
            int x = Random.Range(0, 3);
            if (x == 0) shopOutcome = aItem;
            if (x == 1) shopOutcome = bItem;
            if (x == 2) shopOutcome = cItem;
        }
        else
        {
            //if a has more votes than b
            if (shopVoteCountA > shopVoteCountB)
            {
                //if a has the same amount of votes as c choose randomly between a and c
                if (shopVoteCountA == shopVoteCountC)
                {
                    int x = Random.Range(0, 2);
                    if (x == 0) shopOutcome = aItem;
                    if (x == 1) shopOutcome = cItem;
                }
                // if a has more votes than c then choose a
                else if (shopVoteCountA > shopVoteCountC) shopOutcome = aItem;
                else shopOutcome = cItem;
            }
            else
            {
                // if a has the same amount of votes as b choose randomly
                if (shopVoteCountA == shopVoteCountB)
                {
                    int x = Random.Range(0, 2);
                    if (x == 0) shopOutcome = aItem;
                    if (x == 1) shopOutcome = bItem;
                }
                // if b has more votes than c choose b
                if (shopVoteCountB > shopVoteCountC) shopOutcome = bItem;
                else shopOutcome = cItem;
            }
        }
        Debug.Log("shop poll votes for a, b, c: " + shopVoteCountA + " " + shopVoteCountB + " " + shopVoteCountC);
        Debug.Log("Shop outcome: " + shopOutcome);

        foreach (GameObject item in items)
        {
            if (item.GetComponent<ItemScript>().itemName.text.ToLower() == shopOutcome)
            {
                item.GetComponent<BoxCollider2D>().enabled = true;
                item.GetComponent<ItemScript>().StartCoroutine("FlyDown");
            }
            else
            {
                Destroy(item);
            }
        }

        playerSpeech.SetText("");
    }

    private IEnumerator PollRoutine()   //pollroutine coroutine 
    {
        while (true) //while true because i want to run it forever
        {
            //reset the vote count
            voteCountA = 0;
            voteCountB = 0;
            voteCountC = 0;

            //update the numbers to player can see votes in real time.
            voteCountAText.text = "A. " + voteCountA.ToString();
            voteCountBText.text = "B. " + voteCountB.ToString();
            voteCountCText.text = "C. " + voteCountC.ToString();

            //risky frisky while loops (not really, they just reroll the string from the list if it's the same as one that's already rolled)
            string a = polls[Random.Range(0, polls.Count)];
            string b = polls[Random.Range(0, polls.Count)];
            while (b == a)
                b = polls[Random.Range(0, polls.Count)];

            string c = polls[Random.Range(0, polls.Count)];
            while (c == b || c == a)
                c = polls[Random.Range(0, polls.Count)];

            voteText1.text = "A. " + a;
            voteText2.text = "B. " + b;
            voteText3.text = "C. " + c;

            yield return new WaitForSeconds(timeBetweenPolls);
            //reset the values back to default before calling the outcome function of poll
            playerController.DefaultStats();
            EnemyBase.DefaultValues();
            EnemySpawner.minSpawnTime = 2f;
            EnemySpawner.maxSpawnTime = 4f;
            //Determine outcome
            if (voteCountA == voteCountB && voteCountB == voteCountC && voteCountC == voteCountA)
            {
                //outcome = "null";
                int x = Random.Range(0, 3);
                if (x == 0) outcome = a;
                if (x == 1) outcome = b;
                if (x == 2) outcome = c;
            }
            else
            {
                //if a has more votes than b
                if (voteCountA > voteCountB)
                {
                    //if a has the same amount of votes as c choose randomly between a and c
                    if (voteCountA == voteCountC)
                    {
                        int x = Random.Range(0, 2);
                        if (x == 0) outcome = a;
                        if (x == 1) outcome = c;
                    }
                    // if a has more votes than c then choose a
                    else if (voteCountA > voteCountC) outcome = a;
                    else outcome = c;
                }
                else
                {
                    // if a has the same amount of votes as b choose randomly
                    if (voteCountA == voteCountB)
                    {
                        int x = Random.Range(0, 2);
                        if (x == 0) outcome = a;
                        if (x == 1) outcome = b;
                    }
                    // if b has more votes than c choose b
                    if (voteCountB > voteCountC) outcome = b;
                    else outcome = c;
                }
            }
            Debug.Log("poll votes for a, b, c: " + voteCountA + " " + voteCountB + " " + voteCountC);
            currentMod.text = outcome;
            //Call the outcome function 
            PollFunc(outcome);

        }
    }

    //______________________________________________________________________________________________________________________________________________
    //Gracefully chucking all of this into a single file 
    //definitions for functions to execute based on the poll outcome
    //positive interactions
    //player affecting poll functions
    void IncreaseAttack()
    {
        playerController.attackDamage = playerController.defaultAttackDamage * statIncreaseMultiplier;
    }
    void IncreaseMovementSpeed()
    {
        playerController.moveSpeed = playerController.defaultMoveSpeed * statIncreaseMultiplier;
    }
    void IncreaseRollDistance()
    {
        playerController.rollDistance = playerController.defaultRollDistance * statIncreaseMultiplier;
    }
    void IncreaseBlockDuration()
    {
        playerController.IncreaseBlockDuration();
    }
    void IncreaseAttackSpeed()
    {
        playerController.IncreaseAttackSpeed();
    }
    void MegaBuff()
    {
        IncreaseAttack();
        IncreaseMovementSpeed();
        IncreaseRollDistance();
        IncreaseBlockDuration();
        IncreaseAttackSpeed();
    }
    //enemy affecting poll functions
    void IncreaseEnemySpawnTime()
    {
        EnemySpawner.minSpawnTime = EnemySpawner.minSpawnTime * statIncreaseMultiplier;
        EnemySpawner.maxSpawnTime = EnemySpawner.maxSpawnTime * statIncreaseMultiplier;
    }
    void DecreaseEnemyMovementSpeed()
    {
        EnemyBase.moveSpeed = EnemyBase.defaultMoveSpeed * statDecreaseMultiplier;
    }
    void DecreaseEnemyDamage()
    {
        EnemyBase.attackDamage = EnemyBase.defaultAttackDamage * statDecreaseMultiplier;
    }
    void DecreaseEnemyAttackSpeed()
    {
        EnemyBase.attackDelay = EnemyBase.attackDelay * statIncreaseMultiplier;
    }
    void MakeEnemiesWeak()
    {
        IncreaseEnemySpawnTime();
        DecreaseEnemyMovementSpeed();
        DecreaseEnemyDamage();
        DecreaseEnemyAttackSpeed();
    }
    //mixed interactions
    void EasyMode()
    {
        MegaBuff();
        MakeEnemiesWeak();
    }
    //negative interactions________________________________________________________________________________________
    // player
    void DecreaseAttack()
    {
        playerController.attackDamage = playerController.defaultAttackDamage * statDecreaseMultiplier;
    }
    void DecreaseMovementSpeed()
    {
        playerController.moveSpeed = playerController.defaultMoveSpeed * statDecreaseMultiplier;
    }
    void DecreaseRollDistance()
    {
        playerController.rollDistance = playerController.defaultRollDistance * statDecreaseMultiplier;
    }
    void DecreaseBlockDuration()
    {
        playerController.DecreaseBlockDuration();
    }
    void DecreaseAttackSpeed()
    {
        playerController.DecreaseAttackSpeed();
    }
    void MegaDeBuff()
    {
        DecreaseAttack();
        DecreaseMovementSpeed();
        DecreaseRollDistance();
        DecreaseBlockDuration();
        DecreaseAttackSpeed();
    }
    //enemy affecting poll functions
    void DecreaseEnemySpawnTime()
    {
        EnemySpawner.minSpawnTime = EnemySpawner.minSpawnTime * statDecreaseMultiplier;
        EnemySpawner.maxSpawnTime = EnemySpawner.maxSpawnTime * statDecreaseMultiplier;
    }
    void IncreaseEnemyMovementSpeed()
    {
        EnemyBase.moveSpeed = EnemyBase.moveSpeed * statIncreaseMultiplier;
    }
    void IncreaseEnemyDamage()
    {
        EnemyBase.attackDamage = EnemyBase.attackDamage * statIncreaseMultiplier;
    }
    void IncreaseEnemyAttackSpeed()
    {
        EnemyBase.attackDelay = 0.25f;
    }
    void MakeEnemiesStrong()
    {
        IncreaseEnemyMovementSpeed();
        IncreaseEnemyDamage();
        IncreaseEnemyAttackSpeed();
    }
    //mixed interactions
    void HardMode()
    {
        MegaDeBuff();
        MakeEnemiesStrong();
    }
}

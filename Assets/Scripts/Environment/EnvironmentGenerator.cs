using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnvironmentGenerator : MonoBehaviour
{
    public GameObject pillarTrigger;
    public GameObject spawnPoint;
    public GameObject cameraBounds;
    public GameObject cameraObj;
    public GameObject blockSpawner;
    public GameObject playerTpLoc;

    public GameObject item;
    public Item[] items;
    GameObject[] instancedItems = new GameObject[3]; //lock the size cause i don't expect there to be more than 3 items in the shop
    GameObject[] instancedBlockSpawners = new GameObject[3]; //Similarly to above but with block spawners instead of items

    public Text scoreUI;
    int score = -1;

    public int roomNumber = 0;

    [SerializeField] Tile mud, grassLeft, grassRight, grassNormal, grassSingle, underGrassLeft, underGrassRight, bottomLeftRock, bottomRightRock, bottomRock, singleRock, leftRock, rightRock;
    [SerializeField] Tilemap ground;

    float maxHeight = 10;
    float smoothHeight = 20;
    int floorThickness = 20;

    //variables used for the OpenNextRoom function and later on added to another vec3Int value so that new rooms can
    //be generated in line with the openings;
    int openingWidth = 4;
    int openingHeight = 6;
    int pillarWidth = 4;
    int pillarHeight = 10;

    //variable used for the interactive environment room;
    int pitDepth = 10;

    //chance for the interactive block room to spawn instead of a normal enemy room
    float blockRoomChance = 0f;

    int minRoomWidth = 15;
    int maxRoomWidth = 30;

    Vector2Int currentOffset;
    Vector2Int nextOffset;

    //Values for updating camera bounds y values are initialized as they are not as straightforward to obtain due
    // to the use of perlin noise and forloops
    int xStart;
    int xEnd;
    int yPeak = -999999;
    int yLow = 999999;

    public RoomType roomType;

    public void OpenRoom(Vector2Int offset)
    {
        roomNumber++;

        for (int x = 0; x < openingWidth; x++)
        {
            for (int y = 0; y < openingHeight; y++)
            {
                Vector3Int tilePlace = new Vector3Int(offset.x - x, offset.y + y, 0);

                if (y == 0)
                    ground.SetTile(tilePlace, grassNormal);
                else if (y == 5)
                {
                    if (x == 0)
                        ground.SetTile(tilePlace, bottomRightRock);
                    else if (x == 3)
                        ground.SetTile(tilePlace, bottomLeftRock);
                    else
                        ground.SetTile(tilePlace, bottomRock);
                }
                else
                    ground.SetTile(tilePlace, null);
            }
        }

        Instantiate(pillarTrigger, new Vector3(offset.x, offset.y, 0), Quaternion.identity);

        //sort out the score
        score++;
        scoreUI.text = score.ToString();
        FindObjectOfType<AudioManager>().Play("AddScore");

        currentOffset = offset;
        //after the opening is created pass a new offset for the room generation so that new rooms are aligned with the opening
        //generate a "shop" room every 5 rooms
        if (roomNumber % 5 == 0)
        {
            Vector2Int roomOffset = new Vector2Int(offset.x, offset.y);
            GenerateShop(roomOffset);
        }
        //else generate a normal room with enemies 
        else
        {
            if (Random.value < blockRoomChance)
            {
                blockRoomChance = 0f;
                roomType = RoomType.Interactive;
                Vector2Int roomOffset = offset;
                GenerateInteractiveRoom(roomOffset);
            }
            else
            {
                blockRoomChance += 0.1f;
                roomType = RoomType.Normal;
                Vector2Int roomOffset = new Vector2Int(offset.x, offset.y - (openingHeight - 1));
                GenerateRoom(roomOffset);
            }
        }
    }

    public void CloseRoom()
    {
        Vector2Int offset = currentOffset;
        for (int x = 0; x < openingWidth; x++)
        {
            for (int y = 0; y < openingHeight; y++)
            {
                Vector3Int tilePlace = new Vector3Int(offset.x - x, offset.y + y, 0);
                if (x == 0)
                {
                    if (y == 0 && ground.GetTile(new Vector3Int(offset.x + 1, offset.y, 0)) != null)
                        ground.SetTile(tilePlace, underGrassRight);
                    else
                        ground.SetTile(tilePlace, rightRock);
                }
                else if (x == openingWidth - 1)
                {
                    if (y == 0)
                        ground.SetTile(tilePlace, underGrassLeft);
                    else
                        ground.SetTile(tilePlace, leftRock);
                }
                else
                    ground.SetTile(tilePlace, mud);
            }
        }
    }

    //Function to generate a shop room of a fixed size
    void GenerateShop(Vector2Int offset)
    {
        int shopWidth = 20;
        //clear the array of instanced items
        System.Array.Clear(instancedItems, 0, instancedItems.Length);
        //generate a shop that's 10 blocks wide and floorThickness value blocks in height
        for (int x = 1; x <= shopWidth; x++)
        {
            for (int y = 0; y < floorThickness; y++)
            {
                //find the tile placement for the current interation of the nested loop
                int tileX = offset.x + x;
                int tileY = offset.y - y;
                Vector3Int tilePlace = new Vector3Int(tileX, tileY, 0);

                //If top ground block
                if (y == 0)
                {
                    //set normal grass block
                    ground.SetTile(tilePlace, grassNormal);

                    //if the last top grass block
                    if (x == shopWidth)
                    {
                        Vector2Int pillarOffset = new Vector2Int(offset.x + shopWidth, offset.y);
                        GeneratePillar(pillarOffset);
                    }
                }
                //else place mud tiles
                else
                    ground.SetTile(tilePlace, mud);
            }
        }

        //roll for those random items while making sure they're not the same
        Item i1 = items[Random.Range(0, items.Length)];
        Item i2 = items[Random.Range(0, items.Length)];
        while (i2 == i1)
            i2 = items[Random.Range(0, items.Length)];

        Item i3 = items[Random.Range(0, items.Length)];
        while (i3 == i2 || i3 == i1)
            i3 = items[Random.Range(0, items.Length)];

        //spawn in the prefab for the items
        GameObject item1 = Instantiate(item, new Vector3(offset.x + 5, offset.y + 5, 0), Quaternion.identity);
        GameObject item2 = Instantiate(item, new Vector3(offset.x + 10, offset.y + 5, 0), Quaternion.identity);
        GameObject item3 = Instantiate(item, new Vector3(offset.x + 15, offset.y + 5, 0), Quaternion.identity);

        //assing the data containers to items and update them to show properly
        item1.GetComponent<ItemScript>().item = i1;
        item2.GetComponent<ItemScript>().item = i2;
        item3.GetComponent<ItemScript>().item = i3;
        item1.GetComponent<ItemScript>().UpdateItemData();
        item2.GetComponent<ItemScript>().UpdateItemData();
        item3.GetComponent<ItemScript>().UpdateItemData();

        instancedItems[0] = item1;
        instancedItems[1] = item2;
        instancedItems[2] = item3;

        //set camera x bounds
        xStart = offset.x;
        xEnd = offset.x + shopWidth;
    }

    //function for generating interactive rooms
    void GenerateInteractiveRoom(Vector2Int offset)
    {
        int roomWidth = 15;
        Vector3 TemptTPLoc;

        for (int x = 1; x <= roomWidth; x++)        //standard, double for loop for room generation
        {
            for (int y = 0; y < floorThickness; y++)
            {
                int tileX = offset.x + x;
                int tileY = offset.y - y - pitDepth;  //take away and extra 10 from all y values because we want this room to be a pit
                Vector3Int tilePlace = new Vector3Int(tileX, tileY, 0);

                //deal with the Y values for the camera bounds here
                if (tileY > yPeak) yPeak = tileY;
                if (tileY < yLow) yLow = tileY;

                if (y == 0) //set the top tiles to grass
                {
                    ground.SetTile(tilePlace, grassNormal);

                    if (x == roomWidth) //if last x tile spawn in a pillar
                    {
                        Vector2Int pillarOffset = new Vector2Int(offset.x + roomWidth, offset.y);
                        GeneratePillar(pillarOffset);
                        TemptTPLoc = new Vector3(pillarOffset.x + 2, pillarOffset.y + 3, 0);
                        Instantiate(playerTpLoc, TemptTPLoc, Quaternion.identity);
                    }
                }
                else
                    ground.SetTile(tilePlace, mud);
            }
        }
        //clear the array from any previous blocks
        System.Array.Clear(instancedBlockSpawners, 0, instancedBlockSpawners.Length);

        //bs stands for block spawner, also ugly solution but short on time
        GameObject bs1 = Instantiate(blockSpawner, new Vector3(offset.x + 3.5f, offset.y + 10, 0), Quaternion.identity);
        GameObject bs2 = Instantiate(blockSpawner, new Vector3(offset.x + 8.5f, offset.y + 10, 0), Quaternion.identity);
        GameObject bs3 = Instantiate(blockSpawner, new Vector3(offset.x + 13.5f, offset.y + 10, 0), Quaternion.identity);

        instancedBlockSpawners[0] = bs1;
        instancedBlockSpawners[1] = bs2;
        instancedBlockSpawners[2] = bs3;

        //Deal with x camera bounds here
        xStart = offset.x;
        xEnd = offset.x + roomWidth;

        FixInteractiveGround(offset, roomWidth);
    }

    //Function for generating rooms
    void GenerateRoom(Vector2Int offset)
    {
        int height;

        //Random seed and width of the room
        int seed = Random.Range(0, 1000000);
        int roomWidth = Random.Range(minRoomWidth, maxRoomWidth);
        for (int x = 1; x <= roomWidth; x++)
        {
            //perlin noise to make nice unflat environment
            height = Mathf.RoundToInt(maxHeight * Mathf.PerlinNoise(x / smoothHeight + seed, seed));

            for (int y = 0; y < floorThickness; y++)
            {
                int tileX = offset.x + x;
                int tileY = offset.y + height - y;
                Vector3Int tilePlace = new Vector3Int(tileX, tileY, 0);

                //deal with the Y values for the camera bounds here
                if (tileY > yPeak) yPeak = tileY;
                if (tileY < yLow) yLow = tileY;

                //Fill in the top layer with grass
                if (y == 0)
                {
                    ground.SetTile(tilePlace, grassNormal);

                    //Instantiate a possible spawnpoint for enemies every 3 tiles or something
                    if (x % 3 == 0)
                        Instantiate(spawnPoint, new Vector3(offset.x + x, offset.y + height + 2, 0), Quaternion.identity);

                    //If top grass block and last x generate a pillar
                    if (x == roomWidth)
                    {
                        Vector2Int pillarOffset = new Vector2Int(offset.x + roomWidth, offset.y + height);
                        GeneratePillar(pillarOffset);
                    }
                }
                //Fill in the bottom layer with bottom rocks
                else if (y == floorThickness - 1)
                    ground.SetTile(tilePlace, bottomRock);
                // fill in the rest with mud
                else
                    ground.SetTile(tilePlace, mud);
            }
        }
        //Deal with x camera bounds here
        xStart = offset.x;
        xEnd = offset.x + roomWidth;

        //call the fix funciton 
        FixGround(offset, roomWidth, seed);
    }

    //Funciton to generate the pillars that open up for another level
    private void GeneratePillar(Vector2Int pillarOffset)
    {
        Vector3Int tilePlace;
        //Generate the pillar top layer as grass, bottom and sides above ground level as rock,
        //and the rest as mud 
        for (int x = 1; x <= pillarWidth; x++)
        {
            for (int y = -floorThickness; y <= pillarHeight; y++)
            {
                tilePlace = new Vector3Int(pillarOffset.x + x, pillarOffset.y + y, 0);
                ground.SetTile(tilePlace, mud);
                if (x == 1)
                {
                    if (y > 0)
                        ground.SetTile(tilePlace, leftRock);
                    else if (y == 0)
                        ground.SetTile(tilePlace, underGrassLeft);
                }
                else if (x == pillarWidth)
                    if (y > 0)
                        ground.SetTile(tilePlace, rightRock);
                if (y == pillarHeight)
                {
                    if (x == 1)
                        ground.SetTile(tilePlace, grassLeft);
                    else if (x == pillarWidth)
                        ground.SetTile(tilePlace, grassRight);
                    else
                        ground.SetTile(tilePlace, grassNormal);
                }
            }
        }

        nextOffset = new Vector2Int(pillarOffset.x + pillarWidth, pillarOffset.y);
    }

    //function to fix the ground for the interactive room
    void FixInteractiveGround(Vector2Int offset, int roomWidth)
    {
        Vector3Int tilePlace;
        Vector3Int leftNeighbour;
        Vector3Int rightNeighbour;
        Vector3Int upNeighbour;
        Vector3Int downNeighbour;

        for (int x = 0; x <= roomWidth + 1; x++)
        {
            for (int y = 0; y < floorThickness + pitDepth; y++)
            {
                //determine the location of tile neighbours
                tilePlace = new Vector3Int(offset.x + x, offset.y - y, 0);
                leftNeighbour = new Vector3Int(offset.x + x - 1, offset.y - y, 0);
                rightNeighbour = new Vector3Int(offset.x + x + 1, offset.y - y, 0);
                upNeighbour = new Vector3Int(offset.x + x, offset.y - y + 1, 0);
                downNeighbour = new Vector3Int(offset.x + x, offset.y - y - 1, 0);

                if (x == 0)
                {
                    if (ground.GetTile(upNeighbour) != null && ground.GetTile(downNeighbour) != null)
                    {
                        if (ground.GetTile(rightNeighbour) == null) ground.SetTile(tilePlace, rightRock);
                        else if (ground.GetTile(rightNeighbour) != null)
                        {
                            if (ground.GetTile(rightNeighbour) == grassNormal) ground.SetTile(tilePlace, underGrassRight);
                        }
                    }
                }
                else if (x == roomWidth + 1)
                {
                    if (ground.GetTile(upNeighbour) != null && ground.GetTile(downNeighbour) != null)
                    {
                        if (ground.GetTile(leftNeighbour) == null) ground.SetTile(tilePlace, leftRock);
                        else if (ground.GetTile(leftNeighbour) != null)
                        {
                            if (ground.GetTile(leftNeighbour) == grassNormal) ground.SetTile(tilePlace, underGrassLeft);
                        }
                    }
                }
            }
        }
    }

    //funciton to fix the ground for normal rooms
    void FixGround(Vector2Int offset, int roomWidth, int roomSeed)
    {
        int height;

        Vector3Int tilePlace;
        Vector3Int leftNeighbour;
        Vector3Int rightNeighbour;
        Vector3Int upNeighbour;

        //Fix the ground tiles
        for (int x = 0; x <= roomWidth; x++)
        {
            //fix the top grass
            //height should remain the same because it is given the same x, smoothHeight and seed values. 
            //Has to be redone every for loop because of the incrementing x value that affects it
            height = Mathf.RoundToInt(maxHeight * Mathf.PerlinNoise(x / smoothHeight + roomSeed, roomSeed));
            for (int y = 0; y < floorThickness; y++)
            {
                //determine the location of tile neighbours
                tilePlace = new Vector3Int(offset.x + x, offset.y + height - y, 0);
                leftNeighbour = new Vector3Int(offset.x + x - 1, offset.y + height - y, 0);
                rightNeighbour = new Vector3Int(offset.x + x + 1, offset.y + height - y, 0);
                upNeighbour = new Vector3Int(offset.x + x, offset.y + height - y + 1, 0);

                //Correct the top layer of the terrain
                if (y == 0)
                {
                    //If there's no neighbour to the left
                    if (ground.GetTile(leftNeighbour) == null)
                    {
                        //If no neighbour to the right either then place a single grass tile and to make it look nicer place a matching
                        //rock tile underneath it
                        if (ground.GetTile(rightNeighbour) == null)
                        {
                            ground.SetTile(tilePlace, grassSingle);
                        }
                        //if no tile to the left but tile to the right place the appropriate grass tile
                        else if (ground.GetTile(rightNeighbour) != null)
                        {
                            ground.SetTile(tilePlace, grassLeft);
                        }
                    }
                    //If there's no neighbour to the right
                    else if (ground.GetTile(rightNeighbour) == null)
                    {
                        //but there is a neighbour to the left set the correct grass tile
                        if (ground.GetTile(leftNeighbour) != null)
                        {
                            ground.SetTile(tilePlace, grassRight);
                        }
                    }
                }
                //Fix the mud tiles that are under the side grass tiles
                else if (y == 1)
                {
                    if (ground.GetTile(upNeighbour) == grassLeft && ground.GetTile(leftNeighbour) == grassNormal)
                        ground.SetTile(tilePlace, underGrassLeft);
                    else if (ground.GetTile(upNeighbour) == grassRight && ground.GetTile(rightNeighbour) == grassNormal)
                        ground.SetTile(tilePlace, underGrassRight);
                }
            }
        }
    }

    public void UpdateCameraBounds()
    {
        Vector2 topLeft = new Vector2(xStart - 1, yPeak + (pillarHeight * 2));
        Vector2 topRight = new Vector2(xEnd + 3, yPeak + (pillarHeight * 2));
        Vector2 bottomRight = new Vector2(xEnd + 3, yLow);
        Vector2 bottomLeft = new Vector2(xStart - 1, yLow);
        Vector2[] newPoints = new Vector2[] { topRight, topLeft, bottomLeft, bottomRight };
        cameraBounds.GetComponent<PolygonCollider2D>().SetPath(0, newPoints);

        cameraObj.GetComponent<CinemachineConfiner>().InvalidatePathCache();
    }

    public Vector2Int ReturnNextOffset()
    {
        return nextOffset;
    }

    public GameObject[] ReturnInstancedItems()
    {
        return instancedItems;
    }
    public GameObject[] ReturnInstancedBlockSpawners()
    {
        return instancedBlockSpawners;
    }
}

public enum RoomType    //enum used to represent what type of room is currently in play
{
    Normal,
    Interactive
}
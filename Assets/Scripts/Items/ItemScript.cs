using System.Collections;
using TMPro;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public Item item;
    public SpriteRenderer icon;
    public TextMeshPro itemName;
    public TextMeshPro description;
    ItemType itemType;

    Vector3 desiredPosition;

    private void Start()
    {
        desiredPosition = transform.position - new Vector3(0, 2);
    }

    public IEnumerator FlyDown()
    {
        while (transform.position != desiredPosition)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 0.05f);
            yield return new WaitForSeconds(0.0166f);
        }
    }

    public void UpdateItemData()    //Funciton used to update the item prefab with data from the scriptable object
    {
        icon.sprite = item.icon;
        itemName.text = item.itemName;
        description.text = item.description;
        itemType = item.itemType;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))      //if the player enters the trigger call the item function and destroy the item
        {
            ItemFunc(collision);
            FindObjectOfType<AudioManager>().Play("PickUp");
            FindObjectOfType<EnvironmentGenerator>().OpenRoom(FindObjectOfType<EnvironmentGenerator>().ReturnNextOffset());
            Destroy(gameObject);
        }
    }

    private void ItemFunc(Collider2D player)
    {
        switch (itemType)    //item funciton doing different things depending on what the item type is
        {
            case ItemType.coal:
                break;
            case ItemType.emptyBottle:
                break;
            case ItemType.paper:
                break;
            case ItemType.smallRedPotion:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(10);
                break;
            case ItemType.mediumRedPotion:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(15);
                break;
            case ItemType.largeRedPotion:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(20);
                break;
            case ItemType.beer:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-10);
                break;
            case ItemType.wine:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-15);
                break;
            case ItemType.spirit:
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-20);
                break;
            case ItemType.smallGreenPotion:
                player.GetComponent<PlayerController>().TakeDamage(5);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(10);
                break;
            case ItemType.mediumGreenPotion:
                player.GetComponent<PlayerController>().TakeDamage(10);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(20);
                break;
            case ItemType.largeGreenPotion:
                player.GetComponent<PlayerController>().TakeDamage(20);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(40);
                break;
            case ItemType.smallBluePotion:
                player.GetComponent<PlayerController>().Heal(10);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-10);
                break;
            case ItemType.mediumBluePotion:
                player.GetComponent<PlayerController>().Heal(20);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-20);
                break;
            case ItemType.largeBluePotion:
                player.GetComponent<PlayerController>().Heal(30);
                player.GetComponent<PlayerController>().IncreaseMaxHealth(-40);
                break;
            case ItemType.cheese:
                player.GetComponent<PlayerController>().Heal(5);
                break;
            case ItemType.apple:
                player.GetComponent<PlayerController>().Heal(10);
                break;
            case ItemType.bread:
                player.GetComponent<PlayerController>().Heal(15);
                break;
            case ItemType.fishSteak:
                player.GetComponent<PlayerController>().TakeDamage(5);
                break;
            case ItemType.ham:
                player.GetComponent<PlayerController>().TakeDamage(10);
                break;
            case ItemType.meat:
                player.GetComponent<PlayerController>().TakeDamage(15);
                break;
            case ItemType.waterBottle:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(2.5f);
                break;
            case ItemType.leatherBoots:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(5f);
                break;
            case ItemType.ironBoots:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(7.5f);
                break;
            case ItemType.obsidian:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(-2.5f);
                break;
            case ItemType.crystal:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(-5f);
                break;
            case ItemType.ruby:
                player.GetComponent<PlayerController>().IncreaseMoveSpeed(-7.5f);
                break;
            case ItemType.woodenSword:
                player.GetComponent<PlayerController>().IncreaseDamage(5);
                break;
            case ItemType.ironSword:
                player.GetComponent<PlayerController>().IncreaseDamage(7.5f);
                break;
            case ItemType.goldenSword:
                player.GetComponent<PlayerController>().IncreaseDamage(10);
                break;
            case ItemType.bluntAxe:
                player.GetComponent<PlayerController>().IncreaseDamage(-2.5f);
                break;
            case ItemType.bluntShovel:
                player.GetComponent<PlayerController>().IncreaseDamage(-5);
                break;
            case ItemType.bluntPickaxe:
                player.GetComponent<PlayerController>().IncreaseDamage(-7.5f);
                break;
            case ItemType.woodenArmour:
                EnemyBase.IncreaseDamage(-1);
                break;
            case ItemType.leatherArmour:
                EnemyBase.IncreaseDamage(-2);
                break;
            case ItemType.ironArmour:
                EnemyBase.IncreaseDamage(-4);
                break;
            case ItemType.magicWand:
                EnemyBase.IncreaseDamage(2);
                break;
            case ItemType.woodenStaff:
                EnemyBase.IncreaseDamage(4);
                break;
            case ItemType.emeraldStaff:
                EnemyBase.IncreaseDamage(8);
                break;
            case ItemType.candle:
                EnemyBase.IncreaseAttackDelay(-0.025f);
                break;
            case ItemType.lantern:
                EnemyBase.IncreaseAttackDelay(-0.05f);
                break;
            case ItemType.torch:
                EnemyBase.IncreaseAttackDelay(-0.1f);
                break;
            case ItemType.scroll:
                EnemyBase.IncreaseAttackDelay(0.025f);
                break;
            case ItemType.runeStone:
                EnemyBase.IncreaseAttackDelay(0.05f);
                break;
            case ItemType.book:
                EnemyBase.IncreaseAttackDelay(0.1f);
                break;
        }
    }
}

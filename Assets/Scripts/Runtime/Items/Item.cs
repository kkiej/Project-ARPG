using UnityEngine;

namespace LZ
{
    public class Item : ScriptableObject
    {
        [Header("Item Information")]
        public string itemName;
        public Sprite itemIcon;

        //  DECIDES IF THIS ITEM CAN HAVE A STACKABLE AMOUNT
        public int maxItemAmount = 1;
        public int currentItemAmount = 1;


        [TextArea] public string itemDescription;
        public int itemID;
    }
}
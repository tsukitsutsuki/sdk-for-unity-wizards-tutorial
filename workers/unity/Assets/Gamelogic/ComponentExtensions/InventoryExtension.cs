using Improbable.Core;
using UnityEngine;

namespace Assets.Gamelogic.ComponentExtensions
{
    public static class InventoryExtension
    {
        public static bool HasResources(this Inventory.Requirable.Reader inventory)
        {
            return inventory.Data.Resources > 0;
        }

        public static void AddToInventory(this Inventory.Requirable.Writer inventory, int quantity)
        {
            inventory.Send(new Inventory.Update() { Resources = inventory.Data.Resources + quantity });
        }

        public static void RemoveFromInventory(this Inventory.Requirable.Writer inventory, int quantity)
        {
            inventory.Send(new Inventory.Update() { Resources = Mathf.Max(0, inventory.Data.Resources - quantity) });
        }
    }
}

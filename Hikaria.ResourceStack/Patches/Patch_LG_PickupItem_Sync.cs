using HarmonyLib;
using LevelGeneration;
using Player;
using SNetwork;

namespace Hikaria.ResourceStack.Patches
{
    [HarmonyPatch]
    internal static class Patch_LG_PickupItem_Sync
    {
        [HarmonyPatch(typeof(LG_PickupItem_Sync), nameof(LG_PickupItem_Sync.AttemptInteract))]
        [HarmonyPrefix]
        private static void LG_PickupItem_Sync__AttemptInteract__Prefix(LG_PickupItem_Sync __instance, pPickupItemInteraction interaction)
        {
            if (!SNet.IsMaster)
            {
                return;
            }
            if (interaction.type == ePickupItemInteractionType.Pickup)
            {
                if (interaction.pPlayer.TryGetPlayer(out SNet_Player player) && player.HasCharacterSlot) //排除掉空玩家
                {
                    TryStackItem(__instance.item, player);
                }
            }
            return;
        }

        public static void TryStackItem(Item item, SNet_Player player)
        {
            //堆叠只针对 资源包 和 可消耗品
            if (item.pItemData.slot != InventorySlot.ResourcePack && item.pItemData.slot != InventorySlot.Consumable)
            {
                return;
            }
            if (!PlayerBackpackManager.TryGetBackpack(player, out PlayerBackpack backpack))
            {
                return;
            }
            if (!backpack.TryGetBackpackItem(item.pItemData.slot, out BackpackItem backpackItem))
            {
                return;
            }
            if (backpackItem.Instance.pItemData.itemID_gearCRC == item.pItemData.itemID_gearCRC) //itemID_gearCRC 类型唯一识别码
            {
                float consumableAmmoMax = item.ItemDataBlock.ConsumableAmmoMax;
                if (item.pItemData.slot == InventorySlot.ResourcePack)
                {
                    consumableAmmoMax = 100f; //防止过于影响平衡，资源包的上限设定为 5 包
                }
                AmmoType ammoType = (item.pItemData.slot == InventorySlot.ResourcePack) ? AmmoType.ResourcePackRel : AmmoType.CurrentConsumable;
                float ammoInPack = backpack.AmmoStorage.GetAmmoInPack(ammoType);
                if (ammoInPack >= consumableAmmoMax)
                {
                    return;
                }
                float totalAmmo = item.pItemData.custom.ammo + ammoInPack;
                pItemData_Custom customData = item.GetCustomData();
                if (totalAmmo > consumableAmmoMax)
                {
                    customData.ammo = consumableAmmoMax;
                    item.TryCast<ItemInLevel>().GetSyncComponent().SetCustomData(customData, true);
                    backpack.AmmoStorage.SetAmmo(ammoType, totalAmmo - consumableAmmoMax);
                    return;
                }
                PlayerBackpackManager.MasterRemoveItem(backpackItem.Instance, player);
                customData.ammo = totalAmmo;
                item.TryCast<ItemInLevel>().GetSyncComponent().SetCustomData(customData, true);
            }
        }
    }
}

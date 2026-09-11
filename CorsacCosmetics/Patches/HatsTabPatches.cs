using AmongUs.Data;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace CorsacCosmetics.Patches;

public static class HatsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.HatLabel);
        }
        else
        {
            name = CosmeticsCatalog.Instance.HatGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsCatalog.Instance.HatGroups.Count;
        return $"{name} ({_pagination.CurrentTab} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;

        var data = CosmeticsCatalog.Instance.Get(id);

        if (_pagination.CurrentTab == 0) return data == null;
        if (data == null) return false;

        var currentGroup = CosmeticsCatalog.Instance.HatGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);
        return currentGroup == data.GroupId;
    }

    [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
    public static class HatsTabOnEnablePatch
    {
        public static bool Prefix(HatsTab __instance)
        {
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsCatalog.Instance.HatGroups.Count,
                GetText);

            // ---------- Original Game Code -----------
            InventoryTabReversePatch.OnEnable(__instance);

            HatData[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            __instance.currentHat =
                DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat);

            var num = 0;
            foreach (var hat in unlockedHats)
            {
                if (!ShowOnPage(hat.ProductId)) continue;

                var num2 = __instance.XRange.Lerp(num % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var num3 = __instance.YStart - num / __instance.NumPerRow * __instance.YOffset;
                var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num2, num3, -1f);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    var hat1 = hat;
                    colorChip.Button.OnMouseOver.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectHat(hat1);
                    }));
                    colorChip.Button.OnMouseOut.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectHat(HatManager.Instance.GetHatById(DataManager.Player.Customization.Hat));
                    }));
                    colorChip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        __instance.ClickEquip();
                    }));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectHat(hat);
                    }));
                }
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.SimpleUI);
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
                hat.SetPreview(colorChip.Inner.FrontLayer, __instance.GetDisplayColor());
                colorChip.Tag = hat;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                num++;
                if (!HatManager.Instance.CheckLongModeValidCosmetic(hat.ProdId, __instance.PlayerPreview.GetIgnoreLongMode()))
                {
                    colorChip.SetUnavailable();
                }
            }

            __instance.currentHatIsEquipped = true;
            __instance.SetScrollerBounds();
            return false;
        }
    }
}
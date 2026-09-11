using AmongUs.Data;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace CorsacCosmetics.Patches;

public static class VisorsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.Visors);
        }
        else
        {
            name = CosmeticsCatalog.Instance.VisorGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsCatalog.Instance.VisorGroups.Count;
        return $"{name} ({_pagination.CurrentTab} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;
        
        var data = CosmeticsCatalog.Instance.Get(id);

        if (_pagination.CurrentTab == 0) return data == null;
        if (data == null) return false;
        
        var currentGroup = CosmeticsCatalog.Instance.VisorGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);
        return currentGroup == data.GroupId;
    }

    [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
    public static class VisorsTabOnEnablePatch
    {
        public static bool Prefix(VisorsTab __instance)
        {
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsCatalog.Instance.VisorGroups.Count,
                GetText);
            
            // ---------- Original Game Code -----------
            VisorData[] unlockedVisors = DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
            var num = 0;
            foreach (var visor in unlockedVisors)
            {
                if (!ShowOnPage(visor.ProductId)) continue;

                var num2 = __instance.XRange.Lerp(num % __instance.NumPerRow / (__instance.NumPerRow - 1f));
                var num3 = __instance.YStart - num / __instance.NumPerRow * __instance.YOffset;
                var colorChip = Object.Instantiate(__instance.ColorTabPrefab, __instance.scroller.Inner);
                colorChip.transform.localPosition = new Vector3(num2, num3, -1f);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    var visor1 = visor;
                    colorChip.Button.OnMouseOver.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectVisor(visor1);
                    }));
                    colorChip.Button.OnMouseOut.AddListener((UnityAction)(()=>
                    {
                        __instance.SelectVisor(DestroyableSingleton<HatManager>.Instance.GetVisorById(DataManager.Player.Customization.Visor));
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
                        __instance.SelectVisor(visor);
                    }));
                }
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.ProductId = visor.ProductId;
                __instance.UpdateMaterials(colorChip.Inner.FrontLayer, visor);
                visor.SetPreview(colorChip.Inner.FrontLayer, __instance.GetDisplayColor());
                colorChip.Tag = visor.ProdId;
                colorChip.SelectionHighlight.gameObject.SetActive(false);
                __instance.ColorChips.Add(colorChip);
                num++;
                if (!DestroyableSingleton<HatManager>.Instance.CheckLongModeValidCosmetic(visor.ProdId, __instance.PlayerPreview.GetIgnoreLongMode()))
                {
                    colorChip.SetUnavailable();
                }
            }
            if (unlockedVisors.Length != 0)
            {
                __instance.GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
            }
            __instance.visorId = DataManager.Player.Customization.Visor;
            __instance.currentVisorIsEquipped = true;
            __instance.SetScrollerBounds();

            return false;
        }
    }
}
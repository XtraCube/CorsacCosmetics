using System.Collections;
using System.Diagnostics;
using System.Linq;
using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CorsacCosmetics.Components;
using CorsacCosmetics.Cosmetics;
using CorsacCosmetics.Tools;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace CorsacCosmetics.Patches;

public static class PetsTabPatches
{
    private static InventoryTabPaginationBehaviour _pagination = null!;

    private static string GetText()
    {
        string name;
        if (!_pagination || _pagination.CurrentTab == 0)
        {
            name = TranslationController.Instance.GetString(StringNames.PetLabel);
        }
        else
        {
            name = CosmeticsCatalog.Instance.PetGroups.GetGroupNameByIndex(_pagination.CurrentTab - 1);
        }

        var max = CosmeticsCatalog.Instance.PetGroups.Count + 1;
        return $"{name} ({_pagination.CurrentTab + 1} / {max})";
    }

    private static bool ShowOnPage(string id)
    {
        if (!_pagination) return true;

        var data = CosmeticsCatalog.Instance.Get(id);

        if (_pagination.CurrentTab == 0) return data == null;
        if (data == null) return false;

        var currentGroup = CosmeticsCatalog.Instance.PetGroups.GetGroupIdByIndex(_pagination.CurrentTab - 1);
        return currentGroup == data.GroupId;
    }

    [HarmonyPatch(typeof(PetsTab), nameof(PetsTab.OnEnable))]
    public static class PetsTabOnEnablePatch
    {
        public static bool Prefix(PetsTab __instance)
        {
            // --------- Pagination ----------------
            _pagination = __instance.GetComponent<InventoryTabPaginationBehaviour>();
            if (!_pagination)
            {
                _pagination = __instance.gameObject.AddComponent<InventoryTabPaginationBehaviour>();
            }

            _pagination.Setup(
                __instance,
                CosmeticsCatalog.Instance.PetGroups.Count,
                GetText);

            // ---------- Original Game Code -----------
            InventoryTabReversePatch.OnEnable(__instance);

            // ---------- Coroutine -----------
            __instance.StartCoroutine(CoOnEnable(__instance).WrapToIl2Cpp());
            return false;
        }

        private static IEnumerator CoOnEnable(PetsTab petsTab)
        {
            var unlockedPets = HatManager.Instance.GetUnlockedPets().Where(h => ShowOnPage(h.ProductId)).ToArray();

            petsTab.initialized = false;
            petsTab.scroller.CalculateAndSetYBounds(unlockedPets.Length, petsTab.NumPerRow, petsTab.NumRowsVisible, petsTab.YOffset);
            petsTab.petId = DataManager.Player.Customization.Pet;
            petsTab.currentPetIsEquipped = true;
            petsTab.initialized = true; 

            var num = 0;
            var targetFrameTime = Utilities.GetTargetFrameTimeMilliseconds();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var pet in unlockedPets)
            {
                if (!ShowOnPage(pet.ProductId)) continue;

                if (stopwatch.ElapsedMilliseconds >= targetFrameTime)
                {
                    stopwatch.Restart();
                    yield return null;
                }

                var num2 = petsTab.XRange.Lerp(num % petsTab.NumPerRow / (petsTab.NumPerRow - 1f));
                var num3 = petsTab.YStart - num / petsTab.NumPerRow * petsTab.YOffset;
                var chip = Object.Instantiate(petsTab.ColorTabPrefab, petsTab.scroller.Inner);
                chip.transform.localPosition = new Vector3(num2, num3, -1f);
                chip.PlayerEquippedForeground.SetActive(pet.ProdId == DataManager.Player.Customization.Pet);
                chip.SelectionHighlight.gameObject.SetActive(false);
                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    var pet1 = pet;
                    chip.Button.OnMouseOver.AddListener((UnityAction)(()=>
                    {
                        petsTab.SelectPet(chip, pet1);
                    }));
                    chip.Button.OnMouseOut.AddListener((UnityAction)(()=>
                    {
                        petsTab.SelectPet(chip, HatManager.Instance.GetPetById(DataManager.Player.Customization.Pet));
                    }));
                    chip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        petsTab.ClickEquip();
                    }));
                }
                else
                {
                    chip.Button.OnClick.AddListener((UnityAction)(()=>
                    {
                        petsTab.SelectPet(chip, pet);
                    }));
                }
                chip.Button.ClickMask = petsTab.scroller.Hitbox;
                chip.Tag = pet;
                petsTab.UpdateMaterials(chip.Inner.FrontLayer, pet);
                pet.SetPreview(chip.Inner.FrontLayer, petsTab.GetDisplayColor());
                petsTab.ColorChips.Add(chip);
                num++;
                if (!HatManager.Instance.CheckLongModeValidCosmetic(pet.ProdId, petsTab.PlayerPreview.GetIgnoreLongMode()))
                {
                    chip.SetUnavailable();
                }
            }
        }
    }
}

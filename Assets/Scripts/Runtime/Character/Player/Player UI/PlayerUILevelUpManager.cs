using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LZ
{
    public class PlayerUILevelUpManager : PlayerUIMenu
    {
        [Header("Levels")]
        [SerializeField] int[] playerLevels = new int[100];
        [SerializeField] int baseLevelCost = 83;
        [SerializeField] int totalLevelUpCost = 0;

        [Header("Character Stats")]
        [SerializeField] TextMeshProUGUI characterLevelText;
        [SerializeField] TextMeshProUGUI runesHeldText;
        [SerializeField] TextMeshProUGUI runesNeededText;
        [SerializeField] TextMeshProUGUI vigorLevelText;
        [SerializeField] TextMeshProUGUI mindLevelText;
        [SerializeField] TextMeshProUGUI enduranceLevelText;
        [SerializeField] TextMeshProUGUI strengthLevelText;
        [SerializeField] TextMeshProUGUI dexterityLevelText;
        [SerializeField] TextMeshProUGUI intelligenceLevelText;
        [SerializeField] TextMeshProUGUI faithLevelText;

        [Header("Projected Character Stats")]
        [SerializeField] TextMeshProUGUI projectedCharacterLevelText;
        [SerializeField] TextMeshProUGUI projectedRunesHeldText;
        [SerializeField] TextMeshProUGUI projectedVigorLevelText;
        [SerializeField] TextMeshProUGUI projectedMindLevelText;
        [SerializeField] TextMeshProUGUI projectedEnduranceLevelText;
        [SerializeField] TextMeshProUGUI projectedStrengthLevelText;
        [SerializeField] TextMeshProUGUI projectedDexterityLevelText;
        [SerializeField] TextMeshProUGUI projectedIntelligenceLevelText;
        [SerializeField] TextMeshProUGUI projectedFaithLevelText;

        [Header("Sliders")]
        public CharacterAttribute currentSelectedAttribute;
        public Slider vigorSlider;
        public Slider mindSlider;
        public Slider enduranceSlider;
        public Slider strengthSlider;
        public Slider dexteritySlider;
        public Slider intelligenceSlider;
        public Slider faithSlider;

        [Header("Buttons")]
        [SerializeField] Button confirmLevelsButton;

        private void Awake()
        {
            SetAllLevelsCost();
        }

        public override void OpenMenu()
        {
            base.OpenMenu();

            SetCurrentStats();
        }

        //  THIS IS CALLED WHEN OPENING THE MENU
        private void SetCurrentStats()
        {
            //  CHARACTER LEVEL
            characterLevelText.text = PlayerUIManager.instance.localPlayer.characterStatsManager.CalculateCharacterLevelBasedOnAttributes().ToString();
            projectedCharacterLevelText.text = PlayerUIManager.instance.localPlayer.characterStatsManager.CalculateCharacterLevelBasedOnAttributes().ToString();

            //  RUNES
            runesHeldText.text = PlayerUIManager.instance.localPlayer.playerStatsManager.runes.ToString();
            projectedRunesHeldText.text = PlayerUIManager.instance.localPlayer.playerStatsManager.runes.ToString();
            runesNeededText.text = "0";

            //  ATTRIBUTES
            vigorLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.vigor.Value.ToString();
            projectedVigorLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.vigor.Value.ToString();
            vigorSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.vigor.Value;

            mindLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.mind.Value.ToString();
            projectedMindLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.mind.Value.ToString();
            mindSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.mind.Value;

            enduranceLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.endurance.Value.ToString();
            projectedEnduranceLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.endurance.Value.ToString();
            enduranceSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.endurance.Value;

            strengthLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.strength.Value.ToString();
            projectedStrengthLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.strength.Value.ToString();
            strengthSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.strength.Value;

            dexterityLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.dexterity.Value.ToString();
            projectedDexterityLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.dexterity.Value.ToString();
            dexteritySlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.dexterity.Value;

            intelligenceLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.intelligence.Value.ToString();
            projectedIntelligenceLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.intelligence.Value.ToString();
            intelligenceSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.intelligence.Value;

            faithLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.faith.Value.ToString();
            projectedFaithLevelText.text = PlayerUIManager.instance.localPlayer.playerNetworkManager.faith.Value.ToString();
            faithSlider.minValue = PlayerUIManager.instance.localPlayer.playerNetworkManager.faith.Value;

            vigorSlider.Select();
            vigorSlider.OnSelect(null);
        }

        public void UpdateSliderBasedOnCurrentlySelectedAttribute()
        {
            PlayerManager player = PlayerUIManager.instance.localPlayer;

            switch (currentSelectedAttribute)
            {
                case CharacterAttribute.Vigor:
                    projectedVigorLevelText.text = vigorSlider.value.ToString();
                    break;
                case CharacterAttribute.Mind:
                    projectedMindLevelText.text = mindSlider.value.ToString();
                    break;
                case CharacterAttribute.Endurance:
                    projectedEnduranceLevelText.text = enduranceSlider.value.ToString();
                    break;
                case CharacterAttribute.Strength:
                    projectedStrengthLevelText.text = strengthSlider.value.ToString();
                    break;
                case CharacterAttribute.Dexterity:
                    projectedDexterityLevelText.text = dexteritySlider.value.ToString();
                    break;
                case CharacterAttribute.Intelligence:
                    projectedIntelligenceLevelText.text = intelligenceSlider.value.ToString();
                    break;
                case CharacterAttribute.Faith:
                    projectedFaithLevelText.text = faithSlider.value.ToString();
                    break;
                default:
                    break;
            }

            //  PASSES OUR CURRENT LEVEL AND OUR PROJECTED LEVEL TO SET OUR COST FOR LEVELING UP
            CalculateLevelCost(
                player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(),
                player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(true));

            projectedCharacterLevelText.text = player.characterStatsManager.CalculateCharacterLevelBasedOnAttributes(true).ToString();
            runesNeededText.text = totalLevelUpCost.ToString();

            //  CHECK COST
            if (totalLevelUpCost > player.playerStatsManager.runes)
            {
                //  DISABLE CONFIRM BUTTON SO YOU CANT LEVEL UP
                confirmLevelsButton.interactable = false;

                //  OPTIONALLY CHANGE LEVEL UP FIELDS TEXT TO RED
            }
            else
            {
                confirmLevelsButton.interactable = true;
            }
        }

        public void ConfirmLevels()
        {
            //  1. CALCULATE COST

            //  2. CHANGE STAT TEXTS TO COLORS DEPENDING ON IF PLAYER CAN AFFORD IT OR NOT, AND IF LEVELS ARE HIGHER

            //  3. DEDUCT COST FROM TOTAL RUNES

            //  4. SET NEW STATS
            PlayerManager player = PlayerUIManager.instance.localPlayer;

            //  DEDUCT COST FROM TOTAL RUNES
            player.playerStatsManager.runes -= totalLevelUpCost;

            //  SET NEW STATS
            player.playerNetworkManager.vigor.Value = Mathf.RoundToInt(vigorSlider.value);
            player.playerNetworkManager.mind.Value = Mathf.RoundToInt(mindSlider.value);
            player.playerNetworkManager.endurance.Value = Mathf.RoundToInt(enduranceSlider.value);
            player.playerNetworkManager.strength.Value = Mathf.RoundToInt(strengthSlider.value);
            player.playerNetworkManager.dexterity.Value = Mathf.RoundToInt(dexteritySlider.value);
            player.playerNetworkManager.intelligence.Value = Mathf.RoundToInt(intelligenceSlider.value);
            player.playerNetworkManager.faith.Value = Mathf.RoundToInt(faithSlider.value);

            SetCurrentStats();
        }

        private void SetAllLevelsCost()
        {
            for (int i = 0; i < playerLevels.Length; i++)
            {
                //  LEVEL 0 HAS NO COST
                if (i == 0)
                    continue;

                playerLevels[i] = baseLevelCost + (50 * i);
            }
        }

        private void CalculateLevelCost(int currentLevel, int projectedLevel)
        {
            int totalCost = 0;

            //  WE DONT WANT TO CHARGE FOR LEVELS WE ALREADY PAID FOR
            //  EX, IF YOU ARE LEVEL 21 WE DONT ADD THE COST OF THE FIRST 21 LEVELS
            for (int i = 0; i < projectedLevel; i++)
            {
                if (i < currentLevel)
                    continue;

                totalCost += playerLevels[i];
            }

            totalLevelUpCost = totalCost;

            projectedRunesHeldText.text = (PlayerUIManager.instance.localPlayer.playerStatsManager.runes - totalCost).ToString();

            if (totalCost > PlayerUIManager.instance.localPlayer.playerStatsManager.runes)
            {
                projectedRunesHeldText.color = Color.red;
            }
            else
            {
                projectedRunesHeldText.color = Color.white;
            }
        }
    }
}

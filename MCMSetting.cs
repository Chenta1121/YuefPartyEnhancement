using System;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace YuefPartyEnhancement
{
    internal class MCMSetting : AttributeGlobalSettings<MCMSetting>
    {
        public override string Id
        {
            get
            {
                return "YuefPartyEnhancement";
            }
        }

        public override string DisplayName
        {
            get
            {
                // 使用本地化字符串为显示名称提供文本
                string text = new TextObject("{=Yuef_ModName}Troop Enhancement Yuef").ToString();

                // 获取当前程序集的版本号
                Version version = typeof(MCMSetting).Assembly.GetName().Version;

                // 返回带版本号的模块名称
                return text + (version != null ? " v" + version.ToString(3) : string.Empty);
            }
        }

        public override string FolderName
        {
            get
            {
                return "Yuef_Config";
            }
        }

        public override string FormatType
        {
            get
            {
                return "json2";
            }
        }

        [SettingProperty("{=Yuef_Option_11}Daily Store Supply Amount", 0, 1000000, RequireRestart = false, HintText = "{=Yuef_HinText_11}The daily supply amount at town stores", Order = 1)]
        [SettingPropertyGroup("{=Yuef_Group_1}Town Money", GroupOrder = 1)]
        public int dailyGoldAmount { get; set; } = 300000;

        [SettingProperty("{=Yuef_Option_12}Town Store Supply Limit", 0, 5000000, RequireRestart = false, HintText = "{Yuef_HinText_12}The supply limit for town stores, supplies won't be provided if it exceeds the limit", Order = 2)]
        [SettingPropertyGroup("{=Yuef_Group_1}Town Money", GroupOrder = 1)]
        public int maxGoldLimit { get; set; } = 500000;


        // 是否启用部队募兵功能
        [SettingProperty("{=Yuef_Option_21}Enable Troop Recruitment Feature", RequireRestart = false, HintText = "{=Yuef_HinText_21}Turn on/off the troop recruitment feature", Order = 1)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 2)]
        public bool IsRecruitmentEnabled { get; set; } = true; // 默认启用募兵功能

        // 每周领主奖励金额，AI领主获得的金额
        [SettingProperty("{=Yuef_Option_22}Weekly Lord Reward Amount", 0, 1000000, RequireRestart = false, HintText = "{=Yuef_HinText_22}Amount received by AI lords, no reward will be given if the amount exceeds 1,000,000", Order = 2)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 2)]
        public int LordWeeklyGold { get; set; } = 300000;

        // 最低募兵费用
        [SettingProperty("{=Yuef_Option_23}Minimum Recruitment Cost", 0, 100000, RequireRestart = false, HintText = "{=Yuef_HinText_23}Minimum cost setting, lords will not recruit troops if their current funds are below the minimum cost", Order = 3)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 2)]
        public int MinRecruitmentCost { get; set; } = 50000;

        // 单次募兵费用（上限为最低募兵费用）
        [SettingProperty("{=Yuef_Option_24}Single Recruitment Cost", 0, 100000, RequireRestart = false, HintText = "{=Yuef_HinText_24}The cost per recruitment (the upper limit will not exceed the minimum recruitment cost)", Order = 4)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 1)]
        public int SingleRecruitmentCost
        {
            get => _singleRecruitmentCost;
            set
            {
                // 确保单次募兵费用不超过最低募兵费用
                _singleRecruitmentCost = Math.Min(value, MinRecruitmentCost);
            }
        }
        private int _singleRecruitmentCost = 10000;


        // AI每日部队粮食消耗量
        [SettingProperty("{=Yuef_Option_25}AI Daily Troop Food Supply", 0, 1000, RequireRestart = false, HintText = "{=Yuef_HinText_25}Daily food supply for AI-controlled troops, supplies will not be provided if it exceeds 300", Order = 5)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 1)]
        public int AIDailyPartyGrainAmount { get; set; } = 100;

        // AI最大平民兵种阶数
        [SettingProperty("{=Yuef_Option_26}AI Maximum Civilian Troop Tier", 0, 20, RequireRestart = false, HintText = "{=Yuef_HinText_26}The maximum tier of civilian troops available for AI recruitment, note that the upper limit also affects the number of troops of certain tiers", Order = 6)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 1)]
        public int AIMaxBasicTroopsTier { get; set; } = 9;

        // AI最大贵族兵种阶数
        [SettingProperty("{=Yuef_Option_27}AI Maximum Noble Troop Tier", 0, 20, RequireRestart = false, HintText = "{=Yuef_HinText_27}The maximum tier of noble troops available for AI recruitment, note that the upper limit also affects the number of troops of certain tiers", Order = 7)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 1)]
        public int AIMaxEliteTroopsTier { get; set; } = 9;

        // 平民兵种一次招募数量
        [SettingProperty("{=Yuef_Option_28}Civilian Troops Recruitment Amount", 1, 100, RequireRestart = false, HintText = "{=Yuef_HinText_28}The total number of civilian troops that can be recruited at once", Order = 8)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 2)]
        public int BasicSoldierOneTimeRecruitmentAmount { get; set; } = 10;

        // 贵族兵种一次招募数量
        [SettingProperty("{=Yuef_Option_29}Noble Troops Recruitment Amount", 1, 100, RequireRestart = false, HintText = "{=Yuef_HinText_29}The total number of noble troops that can be recruited at once", Order = 9)]
        [SettingPropertyGroup("{=Yuef_Group_2}Troop Recruitment Enhancement", GroupOrder = 2)]
        public int EliteSoldierOneTimeRecruitmentAmount { get; set; } = 10;

        [SettingProperty("{=Yuef_Option_31}Enable for Player", RequireRestart = false, HintText = "{=Yuef_HinText_31}Once enabled, even when AFK, your troops will automatically level up, though the result is random", Order = 1)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isPlayerAutoUpgradeEnabled { get; set; } = false; // 默认不启用

        [SettingProperty("{=Yuef_Option_32}Enable for Player's Family Troops", RequireRestart = false, HintText = "{=Yuef_HinText_32}Family troops do not include the player", Order = 2)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isFamilyTroopAutoUpgradeEnabled { get; set; } = false; // 默认不启用，表示家族部队每日自动升阶

        [SettingProperty("{=Yuef_Option_33}Enable Auto Upgrade for Bandit Troops", RequireRestart = false, HintText = "{=Yuef_HinText_33}When enabled, bandit troops will automatically level up", Order = 3)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeForBanditsEnabled { get; set; } = false;

        // 是否对村民部队启用自动升阶
        [SettingProperty("{=Yuef_Option_34}Enable Auto Upgrade for Villager Troops", RequireRestart = false, HintText = "{=Yuef_HinText_34}When enabled, villager troops will automatically level up (if auto-upgrade for irregular troops is disabled, they will not upgrade)", Order = 4)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeForVillagersEnabled { get; set; } = false;

        // 是否对商队部队启用自动升阶
        [SettingProperty("{=Yuef_Option_35}Enable Auto Upgrade for Caravan Troops", RequireRestart = false, HintText = "{=Yuef_HinText_35}When enabled, caravan troops will automatically level up", Order = 5)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeForCaravansEnabled { get; set; } = false;

        // 是否对城市驻军启用自动升阶
        [SettingProperty("{=Yuef_Option_36}Enable Auto Upgrade for Garrison Troops", RequireRestart = false, HintText = "{=Yuef_HinText_36}When enabled, garrison troops in cities will automatically level up", Order = 6)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeForGarrisonsEnabled { get; set; } = false;

        // 是否对民兵启用自动升阶
        [SettingProperty("{=Yuef_Option_37}Enable Auto Upgrade for Militia", RequireRestart = false, HintText = "{=Yuef_HinText_37}When enabled, militia (original militia without upgrade targets) will automatically level up", Order = 7)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeForMilitiaEnabled { get; set; } = false;

        private bool _isPlayerCityTroopsUpgradeEnabled = false;

        // 是否对玩家城市驻军和民兵启用自动升阶
        [SettingProperty("{=Yuef_Option_38}Enable for Player's City Garrisons and Militia", RequireRestart = false, HintText = "{=Yuef_HinText_38}When enabled, city garrisons and militia will automatically level up (at least one of garrisons or militia must be enabled, cannot be enabled separately)", Order = 8)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isPlayerCityTroopsUpgradeEnabled
        {
            get
            {
                return _isPlayerCityTroopsUpgradeEnabled;
            }
            set
            {
                // 如果城市驻军和民兵的自动升阶功能都未启用，则保持为false
                if (!isUpgradeForGarrisonsEnabled && !isUpgradeForMilitiaEnabled)
                {
                    _isPlayerCityTroopsUpgradeEnabled = false;
                }
                else
                {
                    _isPlayerCityTroopsUpgradeEnabled = value;
                }
            }
        }

        [SettingProperty("{=Yuef_Option_39}Allow Irregular Troops to Upgrade to Regular Troops", RequireRestart = false, HintText = "{=Yuef_HinText_39}When enabled, bandits, robbers, villagers, etc., will automatically upgrade to regular troops (if possible)", Order = 9)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public bool isUpgradeToRegularAllowed { get; set; } = false; // 默认不启用

        [SettingProperty("{=Yuef_Option_310}Troop Upgrade Base Probability", 0, 100, RequireRestart = false, HintText = "{=Yuef_HinText_310}Base probability for upgrading, with the final upgrade probability affected by tier; higher tiers have a lower probability (at 100% base, tier 8 troops have a 46.67% chance to upgrade)", Order = 10)]
        [SettingPropertyGroup("{=Yuef_Group_3}Troop Upgrade", GroupOrder = 3)]
        public float upgradeProbability { get; set; } = 25;

    }
}

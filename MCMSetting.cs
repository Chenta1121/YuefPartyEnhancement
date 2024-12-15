using System;
using System.Collections.Generic;
using System.Globalization;

using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v1;
using MCM.Abstractions.Base.Global;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace YuefPartyEnhancement
{
    internal class MCMSetting : AttributeGlobalSettings<MCMSetting>
    {
        public override string Id {
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
                string text = new TextObject("{=YueF_ModName}AI部队增强 Yuef ", null).ToString();
                // 获取当前程序集的版本号
                Version version = typeof(MCMSetting).Assembly.GetName().Version;
                // 返回带版本号的模块名称
                return text + ((version != null) ? version.ToString(3) : null);
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



        // 是否启用部队募兵功能
        [SettingProperty("启用部队募兵功能", RequireRestart = true, HintText = "开启/关闭部队募兵功能", Order = 1)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 0)]
        public bool IsRecruitmentEnabled { get; set; } = true; // 默认启用募兵功能

        // 每周领主奖励金额，AI领主获得的金额
        [SettingProperty("每周领主奖励金额", 0, 1000000, RequireRestart = false, HintText = "AI领主获得的金额,已有金额高于1000000不会进行奖励", Order = 2)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
        public int LordWeeklyGold { get; set; } = 300000;

        // 最低募兵费用
        [SettingProperty("最低募兵费用", 0, 100000, RequireRestart = false, HintText = "最低费用设置,领主当前资金低于最低费用时不会进行募兵", Order = 3)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
        public int MinRecruitmentCost { get; set; } = 50000;

        // 单次募兵费用（上限为最低募兵费用）
        [SettingProperty("单次募兵费用", 0, 100000, RequireRestart = false, HintText = "单次募兵消耗的资金(上限不会超过最低募兵费用)", Order = 4)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
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
        [SettingProperty("AI每日部队粮食补给量", 0, 1000, RequireRestart = false, HintText = "AI控制的部队每日粮食补给量,高于300不会进行补给", Order = 5)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
        public int AIDailyPartyGrainAmount { get; set; } = 100;

        // AI最大平民兵种阶数
        [SettingProperty("AI最大平民兵种阶数", 0, 20, RequireRestart = false, HintText = "AI控制的部队募兵可获得的最大平民兵种阶数,注意该值的上限也会影响获得的某阶兵种的获取数量", Order = 6)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
        public int AIMaxBasicTroopsTier { get; set; } = 9;

        // AI最大贵族兵种阶数
        [SettingProperty("AI最大贵族兵种阶数", 0, 20, RequireRestart = false, HintText = "AI控制的部队募兵可获得的最大贵族兵种阶数,注意该值的上限也会影响获得的某阶兵种的获取数量", Order = 7)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 1)]
        public int AIMaxEliteTroopsTier { get; set; } = 9;

        // 平民兵种一次招募数量
        [SettingProperty("平民兵种一次招募数量", 1, 100, RequireRestart = false, HintText = "每次可以招募的平民兵种总数量", Order = 8)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 2)]
        public int BasicSoldierOneTimeRecruitmentAmount { get; set; } = 10;

        // 贵族兵种一次招募数量
        [SettingProperty("贵族兵种一次招募数量", 1, 100, RequireRestart = false, HintText = "每次可以招募的贵族兵种总数量", Order = 9)]
        [SettingPropertyGroup("部队募兵强化", GroupOrder = 2)]
        public int EliteSoldierOneTimeRecruitmentAmount { get; set; } = 10;

        [SettingProperty("是否对玩家启用", RequireRestart = false, HintText = "开启后,即使你在挂机,你的部队也会自己升级了,当然结果是随机的", Order = 1)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 3)]
        public bool isPlayerAutoUpgradeEnabled { get; set; } = false; // 默认不启用

        [SettingProperty("是否对玩家家族部队启用", RequireRestart = false, HintText = "家族部队不包含玩家自己", Order = 2)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 3)]
        public bool isFamilyTroopAutoUpgradeEnabled { get; set; } = false; // 默认不启用，表示家族部队每日自动升阶

        [SettingProperty("是否对强盗部队启用自动升阶", RequireRestart = false, HintText = "开启后，强盗部队(包括藏身处)将自动升级", Order = 3)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 4)]
        public bool isUpgradeForBanditsEnabled { get; set; } = false;

        // 是否对村民部队启用自动升阶
        [SettingProperty("是否对村民部队启用自动升阶", RequireRestart = false, HintText = "开启后，村民部队将自动升级(未开启允许非正规军部队升阶为正规军时无法升级)", Order = 4)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 4)]
        public bool isUpgradeForVillagersEnabled { get; set; } = false;

        // 是否对商队部队启用自动升阶
        [SettingProperty("是否对商队部队启用自动升阶", RequireRestart = false, HintText = "开启后，商队部队将自动升级", Order = 5)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 4)]
        public bool isUpgradeForCaravansEnabled { get; set; } = false;

        // 是否对城市驻军启用自动升阶
        [SettingProperty("是否对城市驻军启用自动升阶", RequireRestart = false, HintText = "开启后城市驻军会自动升级", Order = 6)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 5)]
        public bool isUpgradeForGarrisonsEnabled { get; set; } = false;

        // 是否对民兵启用自动升阶
        [SettingProperty("是否对民兵启用自动升阶", RequireRestart = false, HintText = "开启后民兵(原版民兵无升级对象)会自动升级", Order = 7)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 5)]
        public bool isUpgradeForMilitiaEnabled { get; set; } = false;

        private bool _isPlayerCityTroopsUpgradeEnabled = false;

        // 是否对玩家城市驻军和民兵启用自动升阶
        [SettingProperty("是否对玩家城市驻军和民兵启用", RequireRestart = false, HintText = "开启后玩家城市驻军和民兵等会自动升级(至少启用驻军或民兵中的一项,无法单独开启)", Order = 8)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 5)]
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

        [SettingProperty("允许非正规军部队升阶为正规军", RequireRestart = false, HintText = "开启后,强盗,劫匪,村民等自动升级会升级为正规军(若可能)", Order = 9)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 6)]
        public bool isUpgradeToRegularAllowed { get; set; } = false; // 默认不启用

        [SettingProperty("兵种升阶基础概率", 0, 100, RequireRestart = false, HintText = "根据基础概率每日结算,结算时升阶综合概率受到阶数影响,阶数越高综合概率越低(100%基础时8阶兵会有46.67%升阶)", Order = 10)]
        [SettingPropertyGroup("部队升阶强化", GroupOrder = 6)]
        public float upgradeProbability { get; set; } = 25;

    }
}

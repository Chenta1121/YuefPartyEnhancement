using System;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using MCM.Abstractions.Base.Global;

namespace YuefPartyEnhancement
{
    internal class AddGoldBehavior : CampaignBehaviorBase
    {
        private MCMSetting settings;
        private int GoldAmount = 300000;
        private int MaxGoldLimit = 300000;

        public AddGoldBehavior()
        {
            
        }

        private void InitializeSettings()
        {
            settings = GlobalSettings<MCMSetting>.Instance;
            if (settings != null)
            {
                GoldAmount = settings.dailyGoldAmount;
                MaxGoldLimit = settings.maxGoldLimit;
            }
        }

        private void SettlementGoldGiver(Town town)
        {
            if (town == null) return;

            if (town.Gold >= MaxGoldLimit)
            {
                return; 
            }

            town.ChangeGold(GoldAmount);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        // 注册事件监听
        public override void RegisterEvents()
        {
            // 延迟初始化设置
            InitializeSettings();

            if (settings != null)
            {
                CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(this.SettlementGoldGiver));
            }
        }
    }
}



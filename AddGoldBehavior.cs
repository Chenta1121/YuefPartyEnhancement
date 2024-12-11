using System;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;

namespace YuefPartyEnhancement
{

    internal class AddGoldBehavior : CampaignBehaviorBase
    {
        private const int GoldAmount = 200000; // 可配置的金钱增加量
        private const int MaxGoldLimit = 300000; // 设定的金币上限
        private void SettlementGoldGiver(Town Town)
        {
            if (Town.Gold >= MaxGoldLimit)
            {
                return; // 直接返回，不增加金币
            }
            Town.ChangeGold(GoldAmount);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, new Action<Town>(this.SettlementGoldGiver));
        }
    }


}

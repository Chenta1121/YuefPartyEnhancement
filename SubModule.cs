using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace YuefPartyEnhancement
{
    public class SubModule : MBSubModuleBase
    {

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            InformationManager.DisplayMessage(new InformationMessage("PartyEnhancement Author:YuefChen", Colors.Green));
        }

        protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
        {
            // 确保只在战役模式下初始化
            if (!(game.GameType is Campaign))
            {
                return;
            }

            if (gameStarterObject is CampaignGameStarter campaignGameStarter)
            {
                // 添加自定义行为类
                AddBehaviors(campaignGameStarter);

            }
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            // 将自定义行为类添加到CampaignGameStarter中                   
            campaignGameStarter.AddBehavior(new AddGoldBehavior()); 
            campaignGameStarter.AddBehavior(new LordPartyEnhancement()); 
        }
    }
}

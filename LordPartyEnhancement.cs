using System;
using System.Collections.Generic;
using System.Linq;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;



namespace YuefPartyEnhancement
{
    internal class LordPartyEnhancement : CampaignBehaviorBase
    {
        private Dictionary<CharacterObject, List<CharacterObject>> basicTroopCache = new Dictionary<CharacterObject, List<CharacterObject>>();
        private Dictionary<CharacterObject, List<CharacterObject>> eliteTroopCache = new Dictionary<CharacterObject, List<CharacterObject>>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(
                this, new Action(this.DailyTick));
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(
                this, new Action(this.WeeklyTick));
            CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(
                this, new Action<MobileParty, Settlement, Hero>(this.AddSoliderToParty));
        }

        private void DailyTick()
        {
            if (GlobalSettings<MCMSetting>.Instance.isPlayerAutoUpgradeEnabled) UpgradeAIParty(MobileParty.MainParty.Party);        
            foreach (MobileParty mobileParty in MobileParty.AllLordParties)
            {
                bool isNotPlayerClanParty = mobileParty.ActualClan != null&& mobileParty.ActualClan != Clan.PlayerClan && mobileParty != MobileParty.MainParty;
                if (isNotPlayerClanParty)
                {
                    HandleDailyPartyGrain(mobileParty);
                }
                if (mobileParty.ActualClan == Clan.PlayerClan && GlobalSettings<MCMSetting>.Instance.isFamilyTroopAutoUpgradeEnabled) {
                    UpgradeAIParty(mobileParty.Party);                    
                }
            }
            if (GlobalSettings<MCMSetting>.Instance.isUpgradeForBanditsEnabled) {//强盗部队
                foreach (MobileParty mobileParty in MobileParty.AllBanditParties) {
                    UpgradeAIParty(mobileParty.Party);
                }         
            }
            if (GlobalSettings<MCMSetting>.Instance.isUpgradeForCaravansEnabled) {//商队部队
                foreach (MobileParty mobileParty in MobileParty.AllCaravanParties) {
                    UpgradeAIParty(mobileParty.Party);
                }
            }
            if (GlobalSettings<MCMSetting>.Instance.isUpgradeForVillagersEnabled){//村民部队
                foreach (MobileParty mobileParty in MobileParty.AllVillagerParties)
                {
                    UpgradeAIParty(mobileParty.Party);
                }
            }
            if (GlobalSettings<MCMSetting>.Instance.isUpgradeForGarrisonsEnabled){//驻军部队
                foreach (MobileParty mobileParty in MobileParty.AllGarrisonParties)
                {
                    if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan &&
                        !GlobalSettings<MCMSetting>.Instance.isPlayerCityTroopsUpgradeEnabled)continue;
                    UpgradeAIParty(mobileParty.Party);
                }
            }
            if (GlobalSettings<MCMSetting>.Instance.isUpgradeForMilitiaEnabled){//民兵部队
                foreach (MobileParty mobileParty in MobileParty.AllMilitiaParties)
                {
                    if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan &&
                        !GlobalSettings<MCMSetting>.Instance.isPlayerCityTroopsUpgradeEnabled) continue;
                    UpgradeAIParty(mobileParty.Party);
                }
            }
        }

        private void HandleDailyPartyGrain(MobileParty mobileParty)
        {
            if (!GlobalSettings<MCMSetting>.Instance.IsRecruitmentEnabled) return;
            // 如果粮食分配量不为0，进行处理
            if (GlobalSettings<MCMSetting>.Instance.AIDailyPartyGrainAmount != 0)
            {
                ItemRoster itemRoster = mobileParty.ItemRoster;
                // 如果粮食数量小于等于300，则增加粮食
                if (itemRoster.GetItemNumber(DefaultItems.Grain) <= 300)
                {
                    itemRoster.AddToCounts(DefaultItems.Grain, GlobalSettings<MCMSetting>.Instance.AIDailyPartyGrainAmount);
                }
            }
        }


        private void WeeklyTick()
        {
            if (GlobalSettings<MCMSetting>.Instance.LordWeeklyGold == 0 || !GlobalSettings<MCMSetting>.Instance.IsRecruitmentEnabled) return;
            foreach (Hero hero in Hero.AllAliveHeroes)
            {

                // 检查该英雄是否符合条件：是领主、不是主角、属于其他家族，并且该家族不等于玩家家族
                if (hero != null && hero.IsLord && hero != Hero.MainHero && hero.Clan != null && hero.Clan != Clan.PlayerClan)
                {
                    if (hero.Gold > 500000) return;
                    hero.ChangeHeroGold(GlobalSettings<MCMSetting>.Instance.LordWeeklyGold);
                }
            }
        }

        private void UpgradeAIParty(PartyBase party)
        {

            TroopRoster memberRoster = party.MemberRoster;
            Random random = new Random(); // 用于生成随机数

            // 遍历每个成员
            for (int i = 0; i < memberRoster.Count; i++)
            {
                TroopRosterElement elementCopyAtIndex = memberRoster.GetElementCopyAtIndex(i);
                CharacterObject[] upgradeTargets = elementCopyAtIndex.Character.UpgradeTargets;

                // 如果非正规军部队升阶为正规军功能未启用，且当前角色的职业不是士兵
                if (!GlobalSettings<MCMSetting>.Instance.isUpgradeToRegularAllowed &&
                    elementCopyAtIndex.Character.Occupation != Occupation.Soldier)
                {
                    // 过滤掉 upgradeTargets 中 Occupation == Occupation.Soldier 的元素
                    upgradeTargets = upgradeTargets
                        .Where(target => target.Occupation != Occupation.Soldier)
                        .ToArray();
                }

                if (upgradeTargets == null || upgradeTargets.Length == 0)
                {
                    continue;
                }

                // 获取当前兵员的数量
                int number = elementCopyAtIndex.Number;
                double baseUpgradeProbability = GlobalSettings<MCMSetting>.Instance.upgradeProbability;

                // 根据角色的阶数调整概率
                int tier = elementCopyAtIndex.Character.Tier;
                double adjustedProbability = baseUpgradeProbability * (1 - tier / 15.0);

                // 确保调整后的概率在 [0, 100] 范围内
                adjustedProbability = Math.Min(100.0, Math.Max(0.0, adjustedProbability));

                // 计算预期升级的兵员数量
                int upgradeCount = (int)Math.Floor(number * adjustedProbability / 100.0);

                // 给升级数量加上一些随机波动
                int fluctuation = upgradeCount; // 如果需要，可以为波动添加额外的系数
                int finalUpgradeCount = upgradeCount + random.Next(-fluctuation, fluctuation + 1);

                // 确保最终的升级数量在有效范围内
                finalUpgradeCount = Math.Max(0, Math.Min(finalUpgradeCount, number));

                // 如果没有选中任何要升级的兵员，则跳过
                if (finalUpgradeCount == 0)
                {
                    continue;
                }

                // 随机选择一个升级目标
                CharacterObject selectedUpgradeTarget = upgradeTargets[random.Next(upgradeTargets.Length)];

                // 执行升级操作：移除旧兵种并加入新兵种
                party.AddMember(elementCopyAtIndex.Character, -finalUpgradeCount, 0);
                party.AddMember(selectedUpgradeTarget, finalUpgradeCount, 0);
            }
        }




        public void AddSoliderToParty(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (!GlobalSettings<MCMSetting>.Instance.IsRecruitmentEnabled) return;
            // 检查输入的有效性
            if (mobileParty == null || settlement == null || hero == null || hero.CharacterObject.IsPlayerCharacter || settlement.IsHideout)
            {
                return; // 输入无效时直接返回
            }

            // 确保领主的文化有基本兵种
            if (mobileParty.LeaderHero?.Culture?.BasicTroop != null)
            {
                // 调用添加兵种的方法
                AddSoldiersToParty(mobileParty, mobileParty.LeaderHero, settlement);
            }
        }

        private void AddSoldiersToParty(MobileParty mobileParty, Hero lord, Settlement settlement)
        {
            // 检查输入有效性
            if (mobileParty == null || lord == null || Hero.MainHero.IsPrisoner || lord.Culture != settlement.Culture || settlement == null)
            {
                return; // 如果无效则返回
            }

            int maxNumber = mobileParty.Party.PartySizeLimit; // 获取队伍的最大人数
            int currentNumber = mobileParty.Party.NumberOfAllMembers; // 获取当前队伍人数

            // 如果当前人数已经达到上限或者资金小于50000，直接返回
            if (currentNumber + 20 > maxNumber || lord.Gold < GlobalSettings<MCMSetting>.Instance.MinRecruitmentCost)
            {
                return; // 如果人数已满，则无需继续添加
            }

            // 获取文化下的所有兵种
            var (basicTroops, eliteTroops) = GetAllSoldiers(lord.Culture);

            // 如果兵种列表为空，则返回
            if (basicTroops == null || eliteTroops == null)
            {
                return;
            }

            // 优先处理高阶兵种，按阶级降序排序
            var sortedBasicTroops = basicTroops.OrderByDescending(troop => troop.Tier).ToList();
            var sortedEliteTroops = eliteTroops.OrderByDescending(troop => troop.Tier).ToList();

            lord.ChangeHeroGold(-GlobalSettings<MCMSetting>.Instance.SingleRecruitmentCost); // 扣除资金

            if (settlement.IsTown)
            {
                AddBasicTroopsToParty(mobileParty, sortedBasicTroops, ref currentNumber, maxNumber);
            }
            else if (settlement.IsVillage)
            {
                AddEliteTroopsToParty(mobileParty, sortedEliteTroops, ref currentNumber, maxNumber);
            }
            else
            {
                return;
            }
        }


        private void AddBasicTroopsToParty(MobileParty mobileParty, List<CharacterObject> sortedBasicTroops, ref int currentNumber, int maxNumber)
        {
            // 设置最大添加兵员数量
            int maxAddNumber = GlobalSettings<MCMSetting>.Instance.BasicSoldierOneTimeRecruitmentAmount;
            int havenAddNumber = 0;  // 已添加兵员数量

            // 如果已经没有剩余空间，就直接返回
            int remainingSpace = maxNumber - currentNumber;
            if (remainingSpace <= 0)
            {
                return;
            }

            // 遍历排序后的基础兵种列表，按阶级降序添加
            foreach (var soldierType in sortedBasicTroops)
            {
                // 根据兵种阶级计算需要添加的兵员数量，低阶兵种数量较多
                int calculatedSoldierCount = (soldierType.Tier >= 3) ? Math.Max(0, (MCMSetting.Instance.AIMaxBasicTroopsTier - soldierType.Tier + 1) * 3) : 0;

                // 如果剩余可添加的空间已经不足，直接跳出循环
                if (havenAddNumber >= maxAddNumber || remainingSpace <= 0)
                {
                    break;
                }

                // 限制每次添加的数量，不能超过剩余空间
                calculatedSoldierCount = Math.Min(calculatedSoldierCount, remainingSpace);

                // 更新剩余可添加空间
                remainingSpace -= calculatedSoldierCount;

                // 将计算得到的兵员数量添加到 MobileParty
                mobileParty.MemberRoster.AddToCounts(soldierType, calculatedSoldierCount, false, 0, 0, true, -1);

                // 更新已添加的兵员数量
                havenAddNumber += calculatedSoldierCount;
                currentNumber += calculatedSoldierCount;
            }
        }


        private void AddEliteTroopsToParty(MobileParty mobileParty, List<CharacterObject> sortedEliteTroops, ref int currentNumber, int maxNumber)
        {
            // 设置最大添加兵员数量
            int maxAddNumber = GlobalSettings<MCMSetting>.Instance.EliteSoldierOneTimeRecruitmentAmount;
            int havenAddNumber = 0;  // 已添加兵员数量

            // 计算剩余可添加的兵员数量
            int remainingSpace = maxNumber - currentNumber;
            if (remainingSpace <= 0)
            {
                return; // 如果没有剩余空间，直接返回
            }

            // 遍历排序后的精英兵种列表，按阶级降序添加
            foreach (var soldierType in sortedEliteTroops)
            {
                // 根据兵种阶级计算需要添加的兵员数量，低阶兵种数量较多
                int calculatedSoldierCount = (soldierType.Tier >= 2) ? Math.Max(0, (GlobalSettings<MCMSetting>.Instance.AIMaxEliteTroopsTier - soldierType.Tier + 1) * 2) : 0;

                // 如果没有剩余空间或者已经达到最大添加数量，跳出循环
                if (havenAddNumber >= maxAddNumber || remainingSpace <= 0)
                {
                    break;
                }

                // 限制每次添加的数量，不能超过剩余空间
                calculatedSoldierCount = Math.Min(calculatedSoldierCount, remainingSpace);

                // 更新剩余空间
                remainingSpace -= calculatedSoldierCount;

                // 将计算得到的兵员数量添加到 MobileParty
                mobileParty.MemberRoster.AddToCounts(soldierType, calculatedSoldierCount, false, 0, 0, true, -1);

                // 更新已添加的兵员数量
                havenAddNumber += calculatedSoldierCount;
                currentNumber += calculatedSoldierCount;
            }
        }


        private (List<CharacterObject> basicTroops, List<CharacterObject> eliteTroops) GetAllSoldiers(CultureObject culture)
        {
            // 检查是否已缓存 BasicTroop 和 EliteTroop 列表
            if (basicTroopCache.ContainsKey(culture.BasicTroop) &&
                eliteTroopCache.ContainsKey(culture.EliteBasicTroop))
            {
                // 如果已缓存，直接返回缓存结果
                return (
                    basicTroopCache[culture.BasicTroop],
                    eliteTroopCache[culture.EliteBasicTroop]
                );
            }

            // 使用 HashSet 来避免重复元素
            var allBasicTroops = new HashSet<CharacterObject>();
            var allEliteTroops = new HashSet<CharacterObject>();

            // 定义递归函数，处理兵种的升级目标并防止重复添加
            void AddUpgradeTargets(CharacterObject troop, HashSet<CharacterObject> troopList)
            {
                if (troop == null || troopList.Contains(troop)) return;

                // 添加当前兵种
                troopList.Add(troop);

                // 递归添加所有升级目标
                foreach (var target in troop.UpgradeTargets)
                {
                    AddUpgradeTargets(target, troopList);
                }
            }

            // 递归获取 BasicTroop 和 EliteBasicTroop
            if (culture.BasicTroop != null)
                AddUpgradeTargets(culture.BasicTroop, allBasicTroops);

            if (culture.EliteBasicTroop != null)
                AddUpgradeTargets(culture.EliteBasicTroop, allEliteTroops);

            // 将结果转换为 List 并缓存起来
            var basicTroops = allBasicTroops.ToList();
            var eliteTroops = allEliteTroops.ToList();

            basicTroopCache[culture.BasicTroop] = basicTroops;
            eliteTroopCache[culture.EliteBasicTroop] = eliteTroops;

            // 返回两个列表
            return (basicTroops, eliteTroops);
        }



        public override void SyncData(IDataStore dataStore)
        {
        }

    }
}

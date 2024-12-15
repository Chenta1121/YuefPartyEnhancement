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
            var settings = GlobalSettings<MCMSetting>.Instance;

            if (settings.isPlayerAutoUpgradeEnabled)
                UpgradeAIParty(MobileParty.MainParty.Party);

            // 遍历所有的 Lord 部队
            foreach (MobileParty mobileParty in MobileParty.AllLordParties)
            {
                if (mobileParty.IsMainParty) continue;

                var leaderHero = mobileParty.LeaderHero;
                if (leaderHero != null && leaderHero.Clan != null)
                {
                    bool isNotPlayerClanParty = leaderHero.Clan != Clan.PlayerClan;
                    if (isNotPlayerClanParty)
                    {
                        HandleDailyPartyGrain(mobileParty);
                    }

                    // 如果是 Player Clan 并且开启了自动升级
                    if (leaderHero.Clan == Clan.PlayerClan && settings.isFamilyTroopAutoUpgradeEnabled)
                    {
                        UpgradeAIParty(mobileParty.Party);
                    }
                }
            }

            // 强盗部队升级
            if (settings.isUpgradeForBanditsEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllBanditParties)
                {
                    UpgradeAIParty(mobileParty.Party);
                }
            }

            // 商队部队升级
            if (settings.isUpgradeForCaravansEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllCaravanParties)
                {
                    UpgradeAIParty(mobileParty.Party);
                }
            }

            // 村民部队升级
            if (settings.isUpgradeForVillagersEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllVillagerParties)
                {
                    UpgradeAIParty(mobileParty.Party);
                }
            }

            // 驻军部队升级
            if (settings.isUpgradeForGarrisonsEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllGarrisonParties)
                {
                    // 判断是否是玩家城市的驻军
                    if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan &&
                        !settings.isPlayerCityTroopsUpgradeEnabled)
                        continue;

                    UpgradeAIParty(mobileParty.Party);
                }
            }

            // 民兵部队升级
            if (settings.isUpgradeForMilitiaEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllMilitiaParties)
                {
                    // 判断是否是玩家城市的民兵
                    if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan &&
                        !settings.isPlayerCityTroopsUpgradeEnabled)
                        continue;

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
            // 如果没有开启招募或领主每周金钱为零，则直接返回
            if (GlobalSettings<MCMSetting>.Instance.LordWeeklyGold == 0 || !GlobalSettings<MCMSetting>.Instance.IsRecruitmentEnabled)
                return;

            // 遍历所有活跃的英雄
            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                // 确保英雄符合条件
                if (hero == null || hero.IsLord == false || hero == Hero.MainHero || hero.Clan == null || hero.Clan == Clan.PlayerClan)
                    continue;

                // 如果该领主的金钱已经超过1000000，则跳过
                if (hero.Gold > 1000000)
                    continue;

                // 为符合条件的领主添加每周金钱
                hero.ChangeHeroGold(GlobalSettings<MCMSetting>.Instance.LordWeeklyGold);
            }
        }



        private Random random = new Random(); // 将 Random 对象提升为类成员变量

        private void UpgradeAIParty(PartyBase party)
        {
            TroopRoster memberRoster = party.MemberRoster;

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

                // 如果没有可用的升级目标，跳过该兵员
                if (upgradeTargets == null || upgradeTargets.Length == 0)
                {
                    continue;
                }

                int number = elementCopyAtIndex.Number;
                double baseUpgradeProbability = GlobalSettings<MCMSetting>.Instance.upgradeProbability;

                // 根据角色的阶数调整概率
                int tier = elementCopyAtIndex.Character.Tier;
                double adjustedProbability = baseUpgradeProbability * (1 - tier / 15.0);

                // 确保调整后的概率在 [0, 100] 范围内
                adjustedProbability = Math.Min(100.0, Math.Max(0.0, adjustedProbability));

                // 计算预期升级的兵员数量
                int upgradeCount = (int)Math.Floor(number * adjustedProbability / 100.0);
                
                // 计算期望值的 25% 作为波动范围
                int fluctuation = (int)Math.Floor(upgradeCount * 0.25); // 期望值的 25%，向下取整
                // 生成波动范围内的随机数，并加到原始的升级数量上
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
                if (finalUpgradeCount > 0)
                {
                    party.AddMember(elementCopyAtIndex.Character, -finalUpgradeCount, 0);
                    party.AddMember(selectedUpgradeTarget, finalUpgradeCount, 0);
                }
            }
        }

        public void AddSoliderToParty(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            // 如果没有启用招募功能，直接返回
            if (!GlobalSettings<MCMSetting>.Instance.IsRecruitmentEnabled) return;

            // 检查输入有效性：空值、玩家角色、或是藏匿地点
            if (mobileParty == null || settlement == null || hero == null || hero.CharacterObject.IsPlayerCharacter || settlement.IsHideout)
            {
                return; // 输入无效时直接返回
            }

            // 确保领主的文化和基本兵种存在
            if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.Culture?.BasicTroop != null)
            {
                // 调用添加兵种的方法
                AddSoldiersToParty(mobileParty, mobileParty.LeaderHero, settlement);
            }
        }


        private void AddSoldiersToParty(MobileParty mobileParty, Hero lord, Settlement settlement)
        {
            // 检查输入有效性，首先确认 settlement 是否为 null
            if (settlement == null || mobileParty == null || lord == null || Hero.MainHero.IsPrisoner || lord.Culture != settlement.Culture)
            {
                return; // 如果无效则返回
            }

            // 获取配置项
            var settings = GlobalSettings<MCMSetting>.Instance;
            int maxNumber = mobileParty.Party.PartySizeLimit; // 获取队伍的最大人数
            int currentNumber = mobileParty.Party.NumberOfAllMembers; // 获取当前队伍人数

            // 如果当前人数已经达到上限或者资金不足，直接返回
            if (currentNumber + 20 > maxNumber || lord.Gold < settings.MinRecruitmentCost)
            {
                return; // 如果人数已满或资金不足
            }

            // 获取文化下的所有兵种
            var (basicTroops, eliteTroops) = GetAllSoldiers(lord.Culture);

            // 如果兵种列表为空，返回
            if (basicTroops == null || eliteTroops == null)
            {
                return;
            }

            // 优先处理高阶兵种，按阶级降序排序
            var sortedBasicTroops = basicTroops.OrderByDescending(troop => troop.Tier).ToList();
            var sortedEliteTroops = eliteTroops.OrderByDescending(troop => troop.Tier).ToList();

            // 扣除金钱
            lord.ChangeHeroGold(-settings.SingleRecruitmentCost);

            // 根据 settlement 类型添加兵种
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
                return; // 如果不是城镇或村庄，则不进行任何操作
            }
        }



        private void AddBasicTroopsToParty(MobileParty mobileParty, List<CharacterObject> sortedBasicTroops, ref int currentNumber, int maxNumber)
        {
            // 获取一次性最大招募数量
            int maxAddNumber = GlobalSettings<MCMSetting>.Instance.BasicSoldierOneTimeRecruitmentAmount;
            int addedSoldierCount = 0;  // 已添加兵员数量

            // 计算剩余空间
            int remainingSpace = maxNumber - currentNumber;
            if (remainingSpace <= 0)
            {
                return; // 如果没有剩余空间，直接返回
            }

            // 遍历排序后的基础兵种列表，按阶级降序添加
            foreach (var soldierType in sortedBasicTroops)
            {
                // 计算此兵种应该添加的数量
                int calculatedSoldierCount = CalculateSoldierCount(soldierType);

                // 如果剩余空间已经不足，退出循环
                if (addedSoldierCount >= maxAddNumber || remainingSpace <= 0)
                {
                    break;
                }

                // 限制每次添加的数量，不能超过剩余空间
                calculatedSoldierCount = Math.Min(calculatedSoldierCount, remainingSpace);

                // 将计算得到的兵员数量添加到 MobileParty
                mobileParty.MemberRoster.AddToCounts(soldierType, calculatedSoldierCount, false, 0, 0, true, -1);

                // 更新已添加的兵员数量和剩余空间
                addedSoldierCount += calculatedSoldierCount;
                currentNumber += calculatedSoldierCount;
                remainingSpace -= calculatedSoldierCount;
            }
        }

        // 计算兵种应该添加的数量
        private int CalculateSoldierCount(CharacterObject soldierType)
        {
            if (soldierType.Tier >= 3)
            {
                return Math.Max(0, (MCMSetting.Instance.AIMaxBasicTroopsTier - soldierType.Tier + 1) * 3);
            }
            return 0;
        }


        private void AddEliteTroopsToParty(MobileParty mobileParty, List<CharacterObject> sortedEliteTroops, ref int currentNumber, int maxNumber)
        {
            // 获取最大添加的兵员数量
            int maxAddNumber = GlobalSettings<MCMSetting>.Instance.EliteSoldierOneTimeRecruitmentAmount;
            int addedSoldierCount = 0;  // 已添加兵员数量

            // 计算剩余空间
            int remainingSpace = maxNumber - currentNumber;
            if (remainingSpace <= 0)
            {
                return; // 如果没有剩余空间，直接返回
            }

            // 遍历排序后的精英兵种列表，按阶级降序添加
            foreach (var soldierType in sortedEliteTroops)
            {
                // 计算此兵种应该添加的数量
                int calculatedSoldierCount = CalculateEliteSoldierCount(soldierType);

                // 如果没有剩余空间或者已经达到最大添加数量，跳出循环
                if (addedSoldierCount >= maxAddNumber || remainingSpace <= 0)
                {
                    break;
                }

                // 限制每次添加的数量，不能超过剩余空间
                calculatedSoldierCount = Math.Min(calculatedSoldierCount, remainingSpace);

                // 将计算得到的兵员数量添加到 MobileParty
                mobileParty.MemberRoster.AddToCounts(soldierType, calculatedSoldierCount, false, 0, 0, true, -1);

                // 更新已添加的兵员数量和剩余空间
                addedSoldierCount += calculatedSoldierCount;
                currentNumber += calculatedSoldierCount;
                remainingSpace -= calculatedSoldierCount;
            }
        }

        // 计算精英兵种应该添加的数量
        private int CalculateEliteSoldierCount(CharacterObject soldierType)
        {
            if (soldierType.Tier >= 2)
            {
                return Math.Max(0, (GlobalSettings<MCMSetting>.Instance.AIMaxEliteTroopsTier - soldierType.Tier + 1) * 2);
            }
            return 0;
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
            void AddTroopAndUpgrades(CharacterObject troop, HashSet<CharacterObject> troopList)
            {
                if (troop == null || troopList.Contains(troop)) return;

                // 添加当前兵种
                troopList.Add(troop);

                // 递归添加所有升级目标
                foreach (var target in troop.UpgradeTargets)
                {
                    AddTroopAndUpgrades(target, troopList);
                }
            }

            // 获取 BasicTroop 和 EliteBasicTroop 并进行递归处理
            if (culture.BasicTroop != null)
                AddTroopAndUpgrades(culture.BasicTroop, allBasicTroops);

            if (culture.EliteBasicTroop != null)
                AddTroopAndUpgrades(culture.EliteBasicTroop, allEliteTroops);

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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MCM.Abstractions.Base.Global;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace YuefPartyEnhancement
{
    internal class LordPartyEnhancement : CampaignBehaviorBase
    {
        private Dictionary<CharacterObject, List<CharacterObject>> basicTroopCache = new Dictionary<CharacterObject, List<CharacterObject>>();
        private Dictionary<CharacterObject, List<CharacterObject>> eliteTroopCache = new Dictionary<CharacterObject, List<CharacterObject>>();
        private MCMSetting settings;
        private Random random = new Random();



        public override void RegisterEvents()
        {
            // 确保 settings 已经被初始化
            settings = GlobalSettings<MCMSetting>.Instance;
            if (settings != null)
            {
                CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(this.DailyTick));
                CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, new Action(this.WeeklyTick));
                CampaignEvents.AfterSettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.AddSoliderToParty));
            }
        }
        private void DailyTick()
        {

            // 检查 settings 是否为 null
            if (settings == null)
            {
                return; // 如果 settings 为 null，直接返回
            }

            // 玩家自动升级
            if (settings.isPlayerAutoUpgradeEnabled)
            {
                if (MobileParty.MainParty?.Party != null) // 检查 MainParty 和 Party 是否为 null
                {
                    UpgradeAIParty(MobileParty.MainParty.Party);
                }
            }

            // 遍历所有的 Lord 部队
            foreach (MobileParty mobileParty in MobileParty.AllLordParties)
            {
                if (mobileParty.IsMainParty) continue;

                var leaderHero = mobileParty.LeaderHero;

                // 检查 LeaderHero 和 Clan 是否为 null
                if (leaderHero == null || leaderHero.Clan == null)
                {
                    continue; // 如果 LeaderHero 或 Clan 为 null，跳过当前循环
                }

                bool isNotPlayerClanParty = leaderHero.Clan != Clan.PlayerClan;

                if (isNotPlayerClanParty)
                {
                    HandleDailyPartyGrain(mobileParty);
                    UpgradeAIParty(mobileParty.Party);
                }

                if (leaderHero.Clan == Clan.PlayerClan && settings.isFamilyTroopAutoUpgradeEnabled)
                {
                    if (mobileParty.Party != null) // 检查 Party 是否为 null
                    {
                        UpgradeAIParty(mobileParty.Party);
                    }
                    else
                    {
                        continue; 
                    }
                }
            }

            // 强盗部队升级
            if (settings.isUpgradeForBanditsEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllBanditParties)
                {
                    if (mobileParty.Party != null) // 检查 Party 是否为 null
                    {
                        UpgradeAIParty(mobileParty.Party);
                    }
                    else
                    {
                        continue; // 如果 Party 为 null，跳过当前循环
                    }
                }
            }

            // 商队部队升级
            if (settings.isUpgradeForCaravansEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllCaravanParties)
                {
                    if (mobileParty.Party != null) // 检查 Party 是否为 null
                    {
                        UpgradeAIParty(mobileParty.Party);
                    }
                    else
                    {
                        continue; // 如果 Party 为 null，跳过当前循环
                    }
                }
            }

            // 村民部队升级
            if (settings.isUpgradeForVillagersEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllVillagerParties)
                {
                    if (mobileParty.Party != null) // 检查 Party 是否为 null
                    {
                        UpgradeAIParty(mobileParty.Party);
                    }
                    else
                    {
                        continue; // 如果 Party 为 null，跳过当前循环
                    }
                }
            }

            // 驻军部队升级
            if (settings.isUpgradeForGarrisonsEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllGarrisonParties)
                {
                    if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.OwnerClan != null)
                    {
                        // 判断是否是玩家城市的驻军
                        if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan && !settings.isPlayerCityTroopsUpgradeEnabled)
                            continue;

                        if (mobileParty.Party != null) // 检查 Party 是否为 null
                        {
                            UpgradeAIParty(mobileParty.Party);
                        }
                        else
                        {
                            continue; // 如果 Party 为 null，跳过当前循环
                        }
                    }
                    else
                    {
                        continue; // 如果 CurrentSettlement 或 OwnerClan 为 null，跳过当前循环
                    }
                }
            }

            // 民兵部队升级
            if (settings.isUpgradeForMilitiaEnabled)
            {
                foreach (MobileParty mobileParty in MobileParty.AllMilitiaParties)
                {
                    if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.OwnerClan != null)
                    {
                        // 判断是否是玩家城市的民兵
                        if (mobileParty.CurrentSettlement.OwnerClan == Clan.PlayerClan && !settings.isPlayerCityTroopsUpgradeEnabled)
                            continue;

                        if (mobileParty.Party != null) // 检查 Party 是否为 null
                        {
                            UpgradeAIParty(mobileParty.Party);
                        }
                        else
                        {
                            continue; // 如果 Party 为 null，跳过当前循环
                        }
                    }
                    else
                    {
                        continue; // 如果 CurrentSettlement 或 OwnerClan 为 null，跳过当前循环
                    }
                }
            }
        }

        private void HandleDailyPartyGrain(MobileParty mobileParty)
        {
            if (!settings.IsRecruitmentEnabled) return;
            // 如果粮食分配量不为0，进行处理
            if (settings.AIDailyPartyGrainAmount != 0)
            {
                ItemRoster itemRoster = mobileParty.ItemRoster;
                // 如果粮食数量小于等于300，则增加粮食
                if (itemRoster.GetItemNumber(DefaultItems.Grain) <= 300)
                {
                    itemRoster.AddToCounts(DefaultItems.Grain, settings.AIDailyPartyGrainAmount);
                }
            }
        }

        private void WeeklyTick()
        {
            // 如果没有开启招募或领主每周金钱为零，则直接返回
            if (settings.LordWeeklyGold == 0 || !settings.IsRecruitmentEnabled)
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
                hero.ChangeHeroGold(settings.LordWeeklyGold);
            }
        }
        private Dictionary<FormationClass, double> targetRatios = new Dictionary<FormationClass, double>
        {
            { FormationClass.Infantry, 0.2 },
            { FormationClass.Ranged, 0.3 },
            { FormationClass.Cavalry, 0.1 },
            { FormationClass.HorseArcher, 0.4 }
        };

        private void UpgradeAIParty(PartyBase party)
        {
            // 如果阵营为空，或者阵营没有成员名册，或者成员数量为 0，直接返回
            if (party?.MemberRoster?.Count == 0)
                return;

            TroopRoster memberRoster = party.MemberRoster;
            Random random = new Random();
            ExecuteUpgrades(memberRoster, party, random);
        }

        public void ExecuteUpgrades(TroopRoster memberRoster, PartyBase party, Random random)
        {
            if (memberRoster == null || party == null)
            {
                return;
            }

            var categorizedCache = new ConcurrentDictionary<HashSet<CharacterObject>, Dictionary<FormationClass, List<CharacterObject>>>();

            for (int i = 0; i < memberRoster.Count; i++)
            {
                TroopRosterElement element = memberRoster.GetElementCopyAtIndex(i);
                CharacterObject character = element.Character;

                if (character == null || character.UpgradeTargets == null || character.UpgradeTargets.Length == 0)
                    continue;

                // 筛选符合条件的升级目标
                CharacterObject[] upgradeTargets = FilterUpgradeTargets(character);

                if (upgradeTargets.Length == 0)
                    continue;

                // 获取分类后的升级目标
                Dictionary<FormationClass, List<CharacterObject>> categorizedTargets = GetCategorizedTargets(upgradeTargets, categorizedCache);

                Dictionary<FormationClass, double> finalRatios = GetFinalRatios(categorizedTargets);

                double adjustedProbability = CalculateAdjustedProbability(character);
                // 计算最终升级数量
                int finalUpgradeCount = CalculateFinalUpgradeCount(element, adjustedProbability, random);

                // 执行兵种升级操作
                PerformUpgrades(element, categorizedTargets, finalRatios, finalUpgradeCount, party, random);
            }
        }

        // 根据配置筛选升级目标
        private CharacterObject[] FilterUpgradeTargets(CharacterObject character)
        {
            CharacterObject[] upgradeTargets = character.UpgradeTargets;

            // 根据设置限制不允许升级到常规兵种
            if (!settings.isUpgradeToRegularAllowed && character.Occupation != Occupation.Soldier)
            {
                upgradeTargets = upgradeTargets.Where(target => target.Occupation != Occupation.Soldier).ToArray();
            }

            return upgradeTargets;
        }

        // 获取分类后的升级目标
        private Dictionary<FormationClass, List<CharacterObject>> GetCategorizedTargets(
            CharacterObject[] upgradeTargets,
            ConcurrentDictionary<HashSet<CharacterObject>, Dictionary<FormationClass, List<CharacterObject>>> categorizedCache)
        {
            var upgradeTargetsSet = new HashSet<CharacterObject>(upgradeTargets);
            if (!categorizedCache.TryGetValue(upgradeTargetsSet, out var categorizedTargets))
            {
                categorizedTargets = CategorizeUpgradeTargets(upgradeTargets);
                categorizedCache[upgradeTargetsSet] = categorizedTargets;
            }
            return categorizedTargets;
        }

        // 对升级目标进行分类
        private Dictionary<FormationClass, List<CharacterObject>> CategorizeUpgradeTargets(CharacterObject[] upgradeTargets)
        {
            var categorizedTargets = new Dictionary<FormationClass, List<CharacterObject>>();

            foreach (var target in upgradeTargets)
            {
                if (target == null) continue;

                if (!categorizedTargets.ContainsKey(target.DefaultFormationClass))
                {
                    categorizedTargets[target.DefaultFormationClass] = new List<CharacterObject>();
                }
                categorizedTargets[target.DefaultFormationClass].Add(target);
            }

            return categorizedTargets;
        }

        // 计算调整后的升级概率
        private double CalculateAdjustedProbability(CharacterObject character)
        {
            // 计算调整后的升级概率
            return Math.Max(0.0, Math.Min(settings.upgradeProbability * (1 - character.Tier / 15.0), 100.0));
        }

        // 计算最终的升级数量
        private int CalculateFinalUpgradeCount(TroopRosterElement element, double adjustedProbability, Random random)
        {
            // 计算该阶兵种的升阶总数量
            int upgradeCount = (int)Math.Floor(element.Number * adjustedProbability / 100.0);
            int fluctuation = (int)Math.Floor(upgradeCount * 0.1);  // 加入10%的波动
            return Math.Max(0, Math.Min(upgradeCount + random.Next(-fluctuation, fluctuation + 1), element.Number));
        }

        // 获取实际目标比例，使比例和为1
        private Dictionary<FormationClass, double> GetFinalRatios(Dictionary<FormationClass, List<CharacterObject>> categorizedTargets)
        {
            var finalRatios = new Dictionary<FormationClass, double>(targetRatios);

            var existingCategories = categorizedTargets.Keys.ToList();
            foreach (var category in finalRatios.Keys.ToList())
            {
                if (!existingCategories.Contains(category))
                {
                    finalRatios.Remove(category);
                }
            }

            double totalRatio = finalRatios.Values.Sum();
            if (totalRatio > 0 && totalRatio != 1.0)
            {
                if (totalRatio != 1.0)
                {
                    foreach (var category in finalRatios.Keys.ToList())
                    {
                        finalRatios[category] /= totalRatio;
                    }
                }
            }
            return finalRatios;
        }

        // 执行兵种升级操作
        private void PerformUpgrades(
            TroopRosterElement element,
            Dictionary<FormationClass, List<CharacterObject>> categorizedTargets,
            Dictionary<FormationClass, double> finalRatios,
            int finalUpgradeCount,
            PartyBase party,
            Random random)
        {
            foreach (var category in categorizedTargets)
            {
                FormationClass formationClass = category.Key;
                List<CharacterObject> targetsInCategory = category.Value;

                // 提取比例
                double categoryRatio = finalRatios[formationClass];

                int categoryUpgradeCount = (int)Math.Floor(finalUpgradeCount * categoryRatio);

                if (categoryUpgradeCount > 0)
                {
                    int perTargetUpgradeCount = categoryUpgradeCount / targetsInCategory.Count;

                    if (perTargetUpgradeCount == 0 && categoryUpgradeCount > 0)
                    {
                        var randomTarget = targetsInCategory[random.Next(targetsInCategory.Count)];

                        // 移除旧兵种
                        party.AddMember(element.Character, -1, 0);
                        // 添加新兵种
                        party.AddMember(randomTarget, 1, 0);
                    }
                    else
                    {
                        foreach (var target in targetsInCategory)
                        {
                            // 移除旧兵种并添加新兵种
                            party.AddMember(element.Character, -perTargetUpgradeCount, 0);
                            party.AddMember(target, perTargetUpgradeCount, 0);
                        }
                    }
                }
            }
        }

        public void AddSoliderToParty(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (!settings.IsRecruitmentEnabled) return;

            if (mobileParty == null || settlement == null || hero == null || hero.CharacterObject.IsPlayerCharacter || settlement.IsHideout)
            {
                return;
            }

            if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.Culture?.BasicTroop != null)
            {
                AddSoldiersToParty(mobileParty, mobileParty.LeaderHero, settlement);
            }
        }

        private void AddSoldiersToParty(MobileParty mobileParty, Hero lord, Settlement settlement)
        {
            if (settlement == null || mobileParty == null || lord == null || Hero.MainHero.IsPrisoner || lord.Culture != settlement.Culture)
            {
                return; // 如果无效则返回
            }
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

            // 扣除金钱
            lord.ChangeHeroGold(-settings.SingleRecruitmentCost);

            // 根据 settlement 类型添加兵种
            if (settlement.IsTown)
            {
                AddTroopsToParty(mobileParty, basicTroops, ref currentNumber, maxNumber);
            }
            else if (settlement.IsVillage)
            {
                AddTroopsToParty(mobileParty, eliteTroops, ref currentNumber, maxNumber);
            }
            else
            {
                return;
            }
        }

        private CharacterObject GetTroopWithLeastMembers(MobileParty mobileParty, List<CharacterObject> tierTroops)
        {

            if (tierTroops.Count == 0)
            {
                return null;
            }

            CharacterObject troopWithLeastMembers = null;
            int minTroopCount = int.MaxValue;

            foreach (var soldierType in tierTroops)
            {
                int troopCount = mobileParty.MemberRoster.GetTroopCount(soldierType);
                if (troopCount < minTroopCount)
                {
                    minTroopCount = troopCount;
                    troopWithLeastMembers = soldierType;
                }
            }

            return troopWithLeastMembers;
        }

        private int CalculateSoldierCount(int tier)
        {
            if (tier >= 3)
            {
                return Math.Max(0, (settings.AIMaxBasicTroopsTier - tier + 1) * 2);
            }
            return 0;
        }

        private void AddTroopsToParty(MobileParty mobileParty, Dictionary<int, List<CharacterObject>> TroopsByTier, ref int currentNumber, int maxNumber)
        {
            int maxAddNumber = settings.BasicSoldierOneTimeRecruitmentAmount;

            int totalAdded = 0;
            for (int tier = settings.AIMaxBasicTroopsTier; tier >= 0; tier--)
            {
                List<CharacterObject> tierBasicTroops = TroopsByTier.ContainsKey(tier) ? TroopsByTier[tier] : new List<CharacterObject>();
                int calculatedSoldierCount = CalculateSoldierCount(tier);
                var soldierType = GetTroopWithLeastMembers(mobileParty, tierBasicTroops);

                if (soldierType != null)
                {
                    int currentTroopCount = mobileParty.MemberRoster.GetTroopCount(soldierType);

                    int addCount = Math.Min(calculatedSoldierCount, maxAddNumber - totalAdded);

                    mobileParty.MemberRoster.AddToCounts(soldierType, addCount, false, 0, 0, true, -1);
                    totalAdded += addCount;
                }

                if (totalAdded >= maxAddNumber)
                {
                    break;
                }
            }

            currentNumber = totalAdded;
        }

        private (Dictionary<int, List<CharacterObject>> basicTroopsByTier, Dictionary<int, List<CharacterObject>> eliteTroopsByTier) GetAllSoldiers(CultureObject culture)
        {
            // 检查是否已缓存 BasicTroop 和 EliteTroop 列表
            if (basicTroopCache.TryGetValue(culture.BasicTroop, out var cachedBasicTroops) &&
                eliteTroopCache.TryGetValue(culture.EliteBasicTroop, out var cachedEliteTroops))
            {
                return (
                    GetTroopsByTier(cachedBasicTroops),
                    GetTroopsByTier(cachedEliteTroops)
                );
            }

            var allBasicTroops = new HashSet<CharacterObject>();
            var allEliteTroops = new HashSet<CharacterObject>();

            // 增加兵种和升级目标
            void AddTroopAndUpgrades(CharacterObject troop, HashSet<CharacterObject> troopList)
            {
                if (troop == null) return;

                var stack = new Stack<CharacterObject>();
                stack.Push(troop);

                while (stack.Count > 0)
                {
                    var currentTroop = stack.Pop();
                    if (troopList.Contains(currentTroop)) continue;

                    troopList.Add(currentTroop);
                    foreach (var target in currentTroop.UpgradeTargets)
                    {
                        stack.Push(target);
                    }
                }
            }

            // 处理基础兵种
            if (culture.BasicTroop != null)
                AddTroopAndUpgrades(culture.BasicTroop, allBasicTroops);

            // 处理精英兵种
            if (culture.EliteBasicTroop != null)
                AddTroopAndUpgrades(culture.EliteBasicTroop, allEliteTroops);

            // 缓存兵种数据
            var basicTroops = allBasicTroops.ToList();
            var eliteTroops = allEliteTroops.ToList();

            basicTroopCache[culture.BasicTroop] = basicTroops;
            eliteTroopCache[culture.EliteBasicTroop] = eliteTroops;

            return (
                GetTroopsByTier(basicTroops),
                GetTroopsByTier(eliteTroops)
            );
        }

        private Dictionary<int, List<CharacterObject>> GetTroopsByTier(List<CharacterObject> troops)
        {
            // 检查输入是否为 null 或空
            if (troops == null || troops.Count == 0)
                return new Dictionary<int, List<CharacterObject>>();

            return troops
                .GroupBy(t => t.Tier)
                .OrderByDescending(g => g.Key)
                .ToDictionary(g => g.Key, g => g.ToList()); // 转换为字典，Tier 为键，兵种列表为值
        }


        public override void SyncData(IDataStore dataStore)
        {
        }

    }
}

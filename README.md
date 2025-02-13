## 模组: YuefPartyEnhancement  
###### 版本: 1.0.2  
###### 作者: YuefChen  
###### 邮箱: YueF_Chen@outlook.com  
###### 网址: [https://github.com/Chenta1121/YuefPartyEnhancement](https://github.com/Chenta1121/YuefPartyEnhancement)

---

### 功能简介:  

1. **商店加钱**: 增加了商店的资金，当资金大于300000时则不会改变，每日结算；  
2. **领主部队强化**:  
   - 1. 对领主进行了资金补充（每周300000），当资金大于500000时则不会补充，每周结算；  
   - 2. 对领主AI的部队招兵进行了补充，当领主AI进入与自身文化相同的定居点（村庄或城镇）且队伍至少有超过20个空位时会获得士兵奖励，每次奖励需要消耗10000资金，资金低于50000时，不会获得士兵奖励（也不会扣除资金）；  
   - 3. 对领主部队提供了粮食补给，每日结算刷新；  
   - 4. 对领主部队添加了兵种升阶功能，根据基础概率每日结算，结算时升阶综合概率受到阶数影响，阶数越高综合概率越低，  
     \[\text{综合概率} = \text{基础概率} \times \left(1 - \frac{\text{兵种阶数}}{15}\right)\]  
   - 5. 兵种升阶系统的详细设置，可自行配置。  
3. **支持参数自定义配置**

---

**注**:  
1. AI募兵强化后也不会超过部队上限；  
2. 即使综合概率为100%也无法保证必定全部升级，会有一个浮动差值；  
3. 默认兵种升阶概率为25%；  
4. 募兵系统只会奖励平民兵种树和贵族兵种树包含的兵种，特殊兵种例如雇佣兵，中古战锤ROR士兵等不在列表内；  
5. 升阶系统未覆盖一些模组自定义的部队，例如中古战锤的野兽人掠夺者部队。  

---

#### 更新日志  
**2024/12/25**:  
1. 增加了商店的相关配置设置；  
2. 增加了英文与繁中；  
3. 重写了部队在士兵招募和士兵升阶时的方法，避免在部分情况下部队的兵种过于极端（例如中古战锤帝国的长戟兵占比极高）。  

**2024/12/15**: 修复了在仅勾选升阶对家族成员生效时也会对玩家生效的BUG；

---

## Module: YuefPartyEnhancement  
###### Version: 1.0.2  
###### Author: YuefChen  
###### Email: YueF_Chen@outlook.com  
###### URL: [https://github.com/Chenta1121/YuefPartyEnhancement](https://github.com/Chenta1121/YuefPartyEnhancement)

---

### Feature Summary:  

1. **Store Money Increase**: Adds money to the store, which will not change if the balance exceeds 300,000. The balance is settled daily.  
2. **Lord's Army Enhancement**:  
   - 1. Adds funding to the Lord (300,000 per week). When the funding exceeds 500,000, no further funding is added. This is settled weekly.  
   - 2. Enhances the Lord AI's army recruitment. When the Lord AI enters a settlement (village or town) of the same culture and the army has more than 20 available slots, soldier rewards are given. Each reward costs 10,000 funds. If the funds are below 50,000, no soldiers are recruited (and no funds are deducted).  
   - 3. Provides food supply for the Lord's army, refreshed daily.  
   - 4. Adds a unit promotion system for the Lord's army. The promotion chance is calculated based on the base probability and affected by the unit's tier. The higher the tier, the lower the promotion probability.  
     \[\text{Composite Probability} = \text{Base Probability} \times \left(1 - \frac{\text{Unit Tier}}{15}\right)\]  
   - 5. Detailed settings for the unit promotion system are configurable.  
3. **Customizable Parameter Configuration**

---

**Note**:  
1. The AI recruitment enhancement will not exceed the army size limit;  
2. Even if the composite probability is 100%, it does not guarantee all units will be promoted, there will be a fluctuating margin;  
3. The default promotion probability is 25%;  
4. The recruitment system only rewards units from the civilian and noble unit trees. Special units, such as mercenaries and units from the TOR: ROR mod, are not included;  
5. The promotion system does not cover some custom units from mods, such as the Beastman Marauder units in TOR: Fantasy.

---

#### Changelog  
**2024/12/25**:  
1. Added configuration settings for the store;  
2. Added English and Traditional Chinese language support;  
3. Rewrote the unit recruitment and promotion methods to avoid extreme unit compositions (e.g., excessive Long Spearmen in TOR: Empire).  

**2024/12/15**: Fixed a bug where the unit promotion effect for family members would also apply to the player when only selected for family members.

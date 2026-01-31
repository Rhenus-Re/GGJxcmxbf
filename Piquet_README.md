# 皮克牌 (Piquet) 游戏系统

## 📖 游戏简介

皮克牌是一款源自16世纪法国的经典双人策略纸牌游戏，以其深度策略性和独特计分方式著称。

### 基本规则
- **牌组**：32张牌（7、8、9、10、J、Q、K、A，四种花色）
- **玩家**：2人
- **目标**：通过组合牌型和赢取牌墩获得分数，先达到目标分数者获胜

## 🎮 游戏流程

### 1️⃣ 发牌阶段
- 每位玩家获得12张牌
- 剩余8张作为底牌

### 2️⃣ 换牌阶段
- 发牌员可换0-5张牌
- 非发牌员可换剩余的底牌
- **白牌奖励**：无人像牌（J、Q、K）可获得10分

### 3️⃣ 声明组合阶段
比较三种组合，只有最优者得分：

| 组合类型 | 说明 | 计分 |
|---------|------|------|
| **牌点** | 同花色牌张数量 | 每张1分 |
| **顺子** | 同花色连续3+张 | 3张=3分，4张=4分，5+张=15+分 |
| **长套** | 相同点数3-4张 | 3张=3分，4张=14分 |

### 4️⃣ 出牌阶段
- 非发牌员先出牌
- 必须跟出相同花色（无则可出其他牌）
- 同花色大者赢墩，异花色首出者赢
- 每墩1分，7+墩额外得10分，12墩（全赢）额外得40分

## 🗂️ 项目结构

```
Scripts/Piquet/
├── Card.cs                    # 卡牌数据结构（花色、点数）
├── Deck.cs                    # 牌堆管理（洗牌、发牌）
├── PlayerHand.cs              # 玩家手牌管理
├── CombinationAnalyzer.cs     # 组合判断（牌点、顺子、长套）
├── PiquetGameManager.cs       # 游戏状态管理器
├── TrickPlayLogic.cs          # 出牌和赢墩逻辑
├── ScoreManager.cs            # 计分系统
└── PiquetTestScene.cs         # 测试场景

Scenes/
└── PiquetTest.tscn            # 测试场景文件
```

## 🚀 快速开始

### 在Godot中运行测试

1. 打开Godot项目
2. 运行 `Scenes/PiquetTest.tscn` 场景
3. 查看控制台输出观察游戏流程

### 快捷键
- **空格键**：自动执行当前阶段（换牌/声明/出牌）
- **R键**：重新开始游戏
- **P键**：打印当前游戏状态

## 💻 核心API使用示例

### 创建游戏
```csharp
var gameManager = new PiquetGameManager();
AddChild(gameManager);
gameManager.InitializeGame();
```

### 玩家换牌
```csharp
List<Card> cardsToDiscard = new List<Card> { card1, card2 };
gameManager.ExchangeCards(player1, cardsToDiscard);
gameManager.CompleteExchange();
```

### 声明组合
```csharp
gameManager.DeclareAndCompare();
```

### 出牌
```csharp
gameManager.PlayCard(currentPlayer, selectedCard);
```

### 监听游戏事件
```csharp
gameManager.PhaseChanged += (phase) => {
    GD.Print($"阶段切换: {phase}");
};

gameManager.TrickWon += (playerName) => {
    GD.Print($"{playerName} 赢得牌墩");
};

gameManager.GameOver += (winner) => {
    GD.Print($"游戏结束，获胜者：{winner}");
};
```

## 🎯 组合分析示例

```csharp
// 获取玩家的最佳组合
var combinations = CombinationAnalyzer.GetBestCombinations(playerHand);

// 获取最佳顺子
var bestSequence = CombinationAnalyzer.GetBestSequence(playerHand);
if (bestSequence != null) {
    GD.Print($"最佳顺子: {bestSequence}");
}

// 比较两个组合
int result = CombinationAnalyzer.CompareCombinations(combo1, combo2);
```

## 📊 计分系统

### 基础计分
- **声明分**：牌点、顺子、长套的最优者得分
- **赢墩分**：每墩1分
- **多墩奖励**：7+墩额外10分
- **全赢奖励**：12墩额外40分

### 特殊规则（已实现）
- **白牌 (Carte Blanche)**：无人像牌+10分
- 可扩展实现Pique和Repique规则

## 🔧 自定义配置

在 `PiquetGameManager` 中可修改：

```csharp
gameManager.TotalRounds = 6;    // 总回合数（默认6局）
gameManager.TargetScore = 100;  // 目标分数（默认100分）
```

## 📝 下一步开发建议

1. **UI增强**：
   - 可视化卡牌显示
   - 拖拽式出牌交互
   - 实时分数面板

2. **AI对手**：
   - 实现简单AI策略
   - 组合选择优化
   - 出牌决策算法

3. **网络对战**：
   - 支持在线双人对战
   - 房间匹配系统

4. **音效与动画**：
   - 发牌、出牌动画
   - 背景音乐和音效

## 📚 参考资料

- 游戏规则基于经典皮克牌规则
- 使用32张牌组（7-A）
- 双人回合制策略游戏

---

**开发者提示**：所有核心逻辑已完成，可以直接在此基础上构建UI和AI系统。

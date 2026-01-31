# 皮克牌游戏 - 快速入门指南

## ✅ 已完成的功能

### 核心系统
✓ **卡牌系统**：32张皮克牌（7-A，四花色）  
✓ **牌堆管理**：洗牌、发牌功能  
✓ **手牌管理**：玩家手牌操作  
✓ **组合分析**：牌点、顺子、长套自动识别  
✓ **游戏流程**：发牌→换牌→声明→出牌→计分  
✓ **计分系统**：完整的计分逻辑  
✓ **测试场景**：可运行的演示  

## 🎮 如何测试

### 方法1：运行测试场景（推荐）
1. 在Godot编辑器中打开 `Scenes/PiquetTest.tscn`
2. 点击运行场景（F6）
3. 查看控制台输出观察游戏流程
4. 使用快捷键：
   - **空格** = 自动执行当前阶段
   - **R** = 重新开始游戏
   - **P** = 打印当前状态

### 方法2：代码测试
在任何脚本中添加：
```csharp
var gameManager = new PiquetGameManager();
AddChild(gameManager);
// 游戏会自动开始
```

## 📁 文件说明

| 文件 | 功能 |
|-----|------|
| `Card.cs` | 定义卡牌（花色、点数、资源路径） |
| `Deck.cs` | 牌堆（32张牌管理） |
| `PlayerHand.cs` | 玩家手牌管理 |
| `CombinationAnalyzer.cs` | 组合判断引擎 |
| `PiquetGameManager.cs` | 游戏主控制器 |
| `TrickPlayLogic.cs` | 出牌和赢墩逻辑 |
| `ScoreManager.cs` | 计分系统 |
| `CardVisual.cs` | 卡牌UI显示组件 |
| `PiquetTestScene.cs` | 测试场景控制器 |

## 🎯 游戏流程示例

```
1. [发牌] 每人12张，底牌8张
2. [换牌] 玩家选择换牌（可选）
3. [声明] 自动比较三种组合
   - 牌点：同花色最多的牌
   - 顺子：同花色连续牌
   - 长套：相同点数的牌
4. [出牌] 轮流出牌，必须跟花色
5. [计分] 统计赢墩数量
6. 重复直到6局或达到100分
```

## 💡 下一步开发建议

### 初级：UI美化
```csharp
// 使用CardVisual显示卡牌
var cardVisual = new CardVisual();
cardVisual.SetCard(myCard, true);
AddChild(cardVisual);
```

### 中级：玩家交互
- 实现点击选择卡牌
- 添加换牌界面
- 显示组合提示

### 高级：AI对手
- 评估手牌价值
- 智能换牌决策
- 出牌策略算法

## 🔍 调试技巧

查看手牌：
```csharp
gameManager.PrintGameState();
```

查看组合：
```csharp
var combos = CombinationAnalyzer.GetBestCombinations(player1);
foreach (var combo in combos) {
    GD.Print(combo.Value);
}
```

## ❓ 常见问题

**Q: 如何查看卡牌图片？**  
A: 卡牌资源在 `Src/cards/1x/` 文件夹中，使用 `Card.GetImageFileName()` 获取文件名

**Q: 如何修改游戏规则？**  
A: 在 `PiquetGameManager` 中修改 `TotalRounds` 和 `TargetScore`

**Q: 如何添加自定义组合？**  
A: 在 `CombinationAnalyzer` 中添加新的判断方法

## 📞 支持

查看 `Piquet_README.md` 获取完整文档和API参考。

祝游戏开发愉快！🎉

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 游戏场景控制器 - 负责显示和交互
    /// </summary>
    public partial class GameScene : Node2D
    {
        // 场景节点引用
        private HBoxContainer playerHand;
        private HBoxContainer computerHand;
        private MarginContainer playerOutcardArea;  // 玩家出牌区域
        private MarginContainer comOutcardArea;     // 电脑出牌区域
        
        // 模糊遮罩
        private ColorRect blurOverlay;           // 模糊遮罩
        
        // 声明面板UI
        private PanelContainer declarationPanel;  // 声明面板
        private VBoxContainer declarationContainer; // 声明容器
        private Label declarationLabel;           // 声明标签
        private bool isDeclarationPanelVisible = false; // 声明面板是否可见
        private Dictionary<CombinationType, Combination> playerCombinations; // 玩家的组合
        private HashSet<CombinationType> selectedDeclarations = new HashSet<CombinationType>(); // 选中的声明
        
        // 赢墩结果显示UI
        private PanelContainer trickResultPanel;  // 赢墩结果面板
        private Label trickResultLabel;           // 赢墩结果标签
        private bool isShowingTrickResult = false; // 是否正在显示赢墩结果
        private bool isLastTrick = false;         // 是否是最后一墩
        private const float NORMAL_TRICK_DELAY = 2.0f;  // 普通墩显示时间
        private const float LAST_TRICK_DELAY = 3.5f;    // 最后一墩显示时间
        
        // 回合/游戏结束面板UI
        private PanelContainer roundEndPanel;     // 回合结束面板
        private Label roundEndLabel;              // 回合结束标签
        private PanelContainer gameOverPanel;    // 游戏结束面板
        private Label gameOverLabel;             // 游戏结束标签
        
        // 状态提示UI
        private PanelContainer statusPanel;       // 状态提示面板
        private Label statusLabel;               // 状态提示标签
        private Label hintLabel;                 // 操作提示标签
        
        // 游戏管理器
        private PiquetGameManager gameManager;
        
        // 当前选中的卡牌（用于换牌等）
        private List<CardVisual> selectedCards = new List<CardVisual>();
        
        // 换牌状态
        private bool isPlayerExchangeDone = false;
        private bool isComputerExchangeDone = false;
        private int maxExchangeCount = 5;  // 玩家最多可换的牌数
        
        // 发牌会话ID（用于防止旧的发牌定时器影响新一局）
        private int dealingSessionId = 0;

        public override void _Ready()
        {
            GD.Print("游戏场景初始化...");
            
            // 获取场景中的容器节点（使用完整路径）
            playerHand = GetNode<HBoxContainer>("PlayerHandArea/Playerhand");
            computerHand = GetNode<HBoxContainer>("ComHandArea/Comhand");
            playerOutcardArea = GetNode<MarginContainer>("Playeroutcard");
            comOutcardArea = GetNode<MarginContainer>("Comoutcard");
            
            // 设置手牌容器的负间距，让牌重叠显示
            playerHand.AddThemeConstantOverride("separation", -80);
            computerHand.AddThemeConstantOverride("separation", -80);

            // 创建模糊遮罩
            CreateBlurOverlay();
            
            // 创建状态提示UI
            CreateStatusPanel();
            
            // 创建声明面板UI
            CreateDeclarationPanel();
            
            // 创建赢墩结果面板UI
            CreateTrickResultPanel();
            
            // 创建回合结束面板UI
            CreateRoundEndPanel();
            
            // 创建游戏结束面板UI
            CreateGameOverPanel();
            
            // 创建游戏管理器
            gameManager = new PiquetGameManager();
            AddChild(gameManager);
            
            // 连接游戏事件信号
            ConnectGameSignals();
            
            GD.Print("场景初始化完成，等待游戏开始...");
        }

        /// <summary>
        /// 创建模糊遮罩
        /// </summary>
        private void CreateBlurOverlay()
        {
            // 创建模糊遮罩（全屏半透明黑色）
            blurOverlay = new ColorRect();
            blurOverlay.Name = "BlurOverlay";
            blurOverlay.Color = new Color(0, 0, 0, 0.7f);
            blurOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            blurOverlay.Size = new Vector2(1920, 1080);
            blurOverlay.Visible = false;
            blurOverlay.ZIndex = 90;
            AddChild(blurOverlay);
        }

        /// <summary>
        /// 创建状态提示面板
        /// </summary>
        private void CreateStatusPanel()
        {
            // 创建状态面板
            statusPanel = new PanelContainer();
            statusPanel.Name = "StatusPanel";
            statusPanel.ZIndex = 50;
            AddChild(statusPanel);
            
            // 设置面板样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            styleBox.BorderColor = new Color(0.4f, 0.6f, 0.9f);
            styleBox.SetBorderWidthAll(2);
            styleBox.SetCornerRadiusAll(10);
            styleBox.SetContentMarginAll(15);
            statusPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 设置面板位置（屏幕顶部中央）
            statusPanel.Position = new Vector2(560, 10);
            statusPanel.Size = new Vector2(800, 100);
            
            // 创建垂直布局
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            statusPanel.AddChild(vbox);
            
            // 创建状态标签（当前阶段）
            statusLabel = new Label();
            statusLabel.Text = "🎴 皮克牌游戏";
            statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statusLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.4f));
            statusLabel.AddThemeFontSizeOverride("font_size", 28);
            vbox.AddChild(statusLabel);
            
            // 创建提示标签（操作说明）
            hintLabel = new Label();
            hintLabel.Text = "按 空格键 开始游戏";
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hintLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.8f, 1.0f));
            hintLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(hintLabel);
        }

        /// <summary>
        /// 更新状态提示
        /// </summary>
        private void UpdateStatus(string status, string hint)
        {
            if (statusLabel != null)
                statusLabel.Text = status;
            if (hintLabel != null)
                hintLabel.Text = hint;
        }

        /// <summary>
        /// 创建声明面板（模糊背景 + 声明选择）
        /// </summary>
        private void CreateDeclarationPanel()
        {
            // 创建声明面板
            declarationPanel = new PanelContainer();
            declarationPanel.Name = "DeclarationPanel";
            declarationPanel.Visible = false;
            declarationPanel.ZIndex = 100;
            AddChild(declarationPanel);
            
            // 设置面板样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.15f, 0.25f, 0.95f);
            styleBox.BorderColor = new Color(0.3f, 0.7f, 0.9f);
            styleBox.SetBorderWidthAll(3);
            styleBox.SetCornerRadiusAll(15);
            styleBox.SetContentMarginAll(25);
            declarationPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 设置面板位置（屏幕中央）
            declarationPanel.Position = new Vector2(150, 200);
            declarationPanel.Size = new Vector2(1620, 680);
            
            // 创建垂直布局
            var mainVbox = new VBoxContainer();
            mainVbox.AddThemeConstantOverride("separation", 20);
            declarationPanel.AddChild(mainVbox);
            
            // 创建标题标签
            declarationLabel = new Label();
            declarationLabel.Text = "声明阶段 - 选择要声明的组合";
            declarationLabel.HorizontalAlignment = HorizontalAlignment.Center;
            declarationLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 1.0f));
            declarationLabel.AddThemeFontSizeOverride("font_size", 32);
            mainVbox.AddChild(declarationLabel);
            
            // 创建声明容器
            declarationContainer = new VBoxContainer();
            declarationContainer.Name = "DeclarationContainer";
            declarationContainer.AddThemeConstantOverride("separation", 15);
            mainVbox.AddChild(declarationContainer);
            
            // 创建提示标签
            var hintLabel = new Label();
            hintLabel.Text = "点击选择/取消声明，按 Enter 确认声明";
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            hintLabel.AddThemeFontSizeOverride("font_size", 18);
            mainVbox.AddChild(hintLabel);
        }

        /// <summary>
        /// 创建赢墩结果显示面板
        /// </summary>
        private void CreateTrickResultPanel()
        {
            // 创建赢墩结果面板
            trickResultPanel = new PanelContainer();
            trickResultPanel.Name = "TrickResultPanel";
            trickResultPanel.Visible = false;
            trickResultPanel.ZIndex = 100;
            AddChild(trickResultPanel);
            
            // 设置面板样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.05f, 0.1f, 0.2f, 0.9f);
            styleBox.BorderColor = new Color(1.0f, 0.85f, 0.0f);  // 金色边框
            styleBox.SetBorderWidthAll(4);
            styleBox.SetCornerRadiusAll(20);
            styleBox.SetContentMarginAll(30);
            trickResultPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 设置面板位置（屏幕中央偏上）
            trickResultPanel.Position = new Vector2(660, 380);
            trickResultPanel.Size = new Vector2(600, 200);
            
            // 创建结果标签
            trickResultLabel = new Label();
            trickResultLabel.Name = "TrickResultLabel";
            trickResultLabel.HorizontalAlignment = HorizontalAlignment.Center;
            trickResultLabel.VerticalAlignment = VerticalAlignment.Center;
            trickResultLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.3f));  // 金色文字
            trickResultLabel.AddThemeFontSizeOverride("font_size", 36);
            trickResultLabel.CustomMinimumSize = new Vector2(540, 140);
            trickResultPanel.AddChild(trickResultLabel);
        }

        /// <summary>
        /// 显示赢墩结果
        /// </summary>
        private void ShowTrickResult(string playerName, int trickScore, int totalTricks, bool isLast = false)
        {
            // 设置标志
            isShowingTrickResult = true;
            
            // 更新标签文本
            string winnerText = playerName == gameManager.Player1.PlayerName ? "🎉 你 赢了这墩！" : "💻 电脑 赢了这墩！";
            string lastTrickText = isLast ? "\n\n🏁 本局最后一墩！" : "";
            trickResultLabel.Text = $"{winnerText}\n\n+{trickScore} 分 (累计 {totalTricks} 墩){lastTrickText}";
            
            // 显示面板
            trickResultPanel.Visible = true;
        }

        /// <summary>
        /// 隐藏赢墩结果
        /// </summary>
        private void HideTrickResult()
        {
            trickResultPanel.Visible = false;
            isShowingTrickResult = false;
            
            // 如果是电脑回合，继续出牌
            if (gameManager.CurrentPhase == GamePhase.Playing && 
                gameManager.CurrentPlayer == gameManager.Player2)
            {
                CallDeferred(nameof(ComputerAutoPlay));
            }
        }

        /// <summary>
        /// 创建回合结束面板
        /// </summary>
        private void CreateRoundEndPanel()
        {
            roundEndPanel = new PanelContainer();
            roundEndPanel.Name = "RoundEndPanel";
            roundEndPanel.Visible = false;
            roundEndPanel.ZIndex = 100;
            AddChild(roundEndPanel);
            
            // 设置面板样式 - 蓝色主题
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.2f, 0.4f, 0.95f);
            styleBox.BorderColor = new Color(0.3f, 0.6f, 1.0f);
            styleBox.SetBorderWidthAll(4);
            styleBox.SetCornerRadiusAll(20);
            styleBox.SetContentMarginAll(40);
            roundEndPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 设置面板位置（屏幕中央）
            roundEndPanel.Position = new Vector2(460, 280);
            roundEndPanel.Size = new Vector2(1000, 500);
            
            // 创建结果标签
            roundEndLabel = new Label();
            roundEndLabel.Name = "RoundEndLabel";
            roundEndLabel.HorizontalAlignment = HorizontalAlignment.Center;
            roundEndLabel.VerticalAlignment = VerticalAlignment.Center;
            roundEndLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1.0f));
            roundEndLabel.AddThemeFontSizeOverride("font_size", 32);
            roundEndLabel.CustomMinimumSize = new Vector2(920, 420);
            roundEndPanel.AddChild(roundEndLabel);
        }

        /// <summary>
        /// 创建游戏结束面板
        /// </summary>
        private void CreateGameOverPanel()
        {
            gameOverPanel = new PanelContainer();
            gameOverPanel.Name = "GameOverPanel";
            gameOverPanel.Visible = false;
            gameOverPanel.ZIndex = 110;
            AddChild(gameOverPanel);
            
            // 设置面板样式 - 金色主题（胜利）或红色主题（失败）
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.15f, 0.1f, 0.05f, 0.95f);
            styleBox.BorderColor = new Color(1.0f, 0.85f, 0.0f);
            styleBox.SetBorderWidthAll(5);
            styleBox.SetCornerRadiusAll(25);
            styleBox.SetContentMarginAll(50);
            gameOverPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // 设置面板位置（屏幕中央）
            gameOverPanel.Position = new Vector2(410, 240);
            gameOverPanel.Size = new Vector2(1100, 600);
            
            // 创建结果标签
            gameOverLabel = new Label();
            gameOverLabel.Name = "GameOverLabel";
            gameOverLabel.HorizontalAlignment = HorizontalAlignment.Center;
            gameOverLabel.VerticalAlignment = VerticalAlignment.Center;
            gameOverLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.3f));
            gameOverLabel.AddThemeFontSizeOverride("font_size", 40);
            gameOverLabel.CustomMinimumSize = new Vector2(1000, 500);
            gameOverPanel.AddChild(gameOverLabel);
        }

        /// <summary>
        /// 显示回合结束信息
        /// </summary>
        private void ShowRoundEnd(int p1Tricks, int p2Tricks, int p1Score, int p2Score, int currentRound)
        {
            string trickWinner = p1Tricks > p2Tricks ? "🎉 你赢得了更多牌墩！" : 
                                 p1Tricks < p2Tricks ? "💻 电脑赢得了更多牌墩" : "⚖️ 双方平分牌墩";
            
            // 计算本回合得分
            int p1RoundScore = p1Tricks + (p1Tricks >= 7 ? 10 : 0) + (p1Tricks == 12 ? 40 : 0);
            int p2RoundScore = p2Tricks + (p2Tricks >= 7 ? 10 : 0) + (p2Tricks == 12 ? 40 : 0);
            
            roundEndLabel.Text = $"═══════ 第 {currentRound} 局结束 ═══════\n\n" +
                                 $"{trickWinner}\n\n" +
                                 $"📊 牌墩统计:\n" +
                                 $"   你: {p1Tricks} 墩    电脑: {p2Tricks} 墩\n\n" +
                                 $"📈 本局得分:\n" +
                                 $"   你: +{p1RoundScore} 分    电脑: +{p2RoundScore} 分\n\n" +
                                 $"💰 累计总分:\n" +
                                 $"   你: {p1Score} 分    电脑: {p2Score} 分";
            
            blurOverlay.Visible = true;
            roundEndPanel.Visible = true;
            
            // 4秒后自动关闭
            GetTree().CreateTimer(4.0).Timeout += () =>
            {
                HideRoundEnd();
            };
        }

        /// <summary>
        /// 隐藏回合结束面板
        /// </summary>
        private void HideRoundEnd()
        {
            roundEndPanel.Visible = false;
            blurOverlay.Visible = false;
            
            // 如果游戏还没结束，开始新回合并播放发牌动画
            if (gameManager.CurrentPhase != GamePhase.GameOver)
            {
                GD.Print($"\n--- 开始第 {gameManager.CurrentRound} 局 ---");
                gameManager.StartNewRound();
            }
        }

        /// <summary>
        /// 显示游戏结束信息
        /// </summary>
        private void ShowGameOver(string winner, int p1Score, int p2Score)
        {
            bool playerWins = winner == gameManager.Player1.PlayerName;
            
            // 更新面板边框颜色
            var styleBox = gameOverPanel.GetThemeStylebox("panel") as StyleBoxFlat;
            if (styleBox != null)
            {
                styleBox.BorderColor = playerWins ? new Color(1.0f, 0.85f, 0.0f) : new Color(0.8f, 0.2f, 0.2f);
            }
            
            string resultText = playerWins ? "🏆 恭喜你获胜！🏆" : "💻 电脑获胜";
            string emoji = playerWins ? "🎊🎉✨" : "😔";
            
            gameOverLabel.Text = $"╔══════════════════════════╗\n" +
                                 $"║     游戏结束 Game Over     ║\n" +
                                 $"╚══════════════════════════╝\n\n" +
                                 $"{emoji}\n" +
                                 $"{resultText}\n\n" +
                                 $"═══════ 最终得分 ═══════\n\n" +
                                 $"👤 你: {p1Score} 分\n" +
                                 $"💻 电脑: {p2Score} 分\n\n" +
                                 $"分差: {Math.Abs(p1Score - p2Score)} 分";
            
            // 更新标签颜色
            gameOverLabel.AddThemeColorOverride("font_color", 
                playerWins ? new Color(1.0f, 0.9f, 0.3f) : new Color(0.9f, 0.6f, 0.6f));
            
            blurOverlay.Visible = true;
            gameOverPanel.Visible = true;
        }

        /// <summary>
        /// 显示声明面板
        /// </summary>
        private void ShowDeclarationPanel()
        {
            isDeclarationPanelVisible = true;
            blurOverlay.Visible = true;
            declarationPanel.Visible = true;
            
            // 分析玩家手牌的组合
            playerCombinations = CombinationAnalyzer.GetBestCombinations(gameManager.Player1);
            
            // 刷新声明显示
            RefreshDeclarationDisplay();
            
            GD.Print("显示声明面板");
        }

        /// <summary>
        /// 隐藏声明面板
        /// </summary>
        private void HideDeclarationPanel()
        {
            isDeclarationPanelVisible = false;
            blurOverlay.Visible = false;
            declarationPanel.Visible = false;
            
            GD.Print("隐藏声明面板");
        }

        /// <summary>
        /// 刷新声明显示
        /// </summary>
        private void RefreshDeclarationDisplay()
        {
            // 清空现有内容
            foreach (Node child in declarationContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            selectedDeclarations.Clear();
            
            // 显示三种组合类型
            CreateDeclarationRow(CombinationType.Point, "牌点 (Point)", "同一花色牌张数量最多");
            CreateDeclarationRow(CombinationType.Sequence, "顺子 (Sequence)", "同一花色连续的牌");
            CreateDeclarationRow(CombinationType.Set, "长套 (Set)", "相同点数的牌(3张或4张)");
        }

        /// <summary>
        /// 创建声明行
        /// </summary>
        private void CreateDeclarationRow(CombinationType type, string typeName, string description)
        {
            // 创建行容器
            var rowPanel = new PanelContainer();
            declarationContainer.AddChild(rowPanel);
            
            // 设置行样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            styleBox.SetBorderWidthAll(2);
            styleBox.BorderColor = new Color(0.4f, 0.4f, 0.5f);
            styleBox.SetCornerRadiusAll(8);
            styleBox.SetContentMarginAll(15);
            rowPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 20);
            rowPanel.AddChild(hbox);
            
            // 左侧：类型信息
            var infoVbox = new VBoxContainer();
            infoVbox.CustomMinimumSize = new Vector2(200, 0);
            hbox.AddChild(infoVbox);
            
            var typeLabel = new Label();
            typeLabel.Text = typeName;
            typeLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0.5f));
            typeLabel.AddThemeFontSizeOverride("font_size", 24);
            infoVbox.AddChild(typeLabel);
            
            var descLabel = new Label();
            descLabel.Text = description;
            descLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            infoVbox.AddChild(descLabel);
            
            // 中间：组合详情和卡牌显示
            var comboContainer = new HBoxContainer();
            comboContainer.AddThemeConstantOverride("separation", 5);
            comboContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hbox.AddChild(comboContainer);
            
            if (playerCombinations.ContainsKey(type) && playerCombinations[type] != null)
            {
                var combo = playerCombinations[type];
                
                // 显示组合中的卡牌
                foreach (var card in combo.Cards)
                {
                    var cardVisual = new CardVisual();
                    comboContainer.AddChild(cardVisual);
                    cardVisual.SetCard(card, true);
                    cardVisual.CustomMinimumSize = new Vector2(60, 90);
                    cardVisual.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                    cardVisual.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                }
                
                // 分数显示
                var scoreLabel = new Label();
                scoreLabel.Text = $"  得分: {combo.Score}分";
                scoreLabel.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
                scoreLabel.AddThemeFontSizeOverride("font_size", 20);
                scoreLabel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
                comboContainer.AddChild(scoreLabel);
            }
            else
            {
                var noComboLabel = new Label();
                noComboLabel.Text = "无此类型组合";
                noComboLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                noComboLabel.AddThemeFontSizeOverride("font_size", 18);
                comboContainer.AddChild(noComboLabel);
            }
            
            // 右侧：选择按钮
            var selectButton = new Button();
            selectButton.CustomMinimumSize = new Vector2(100, 50);
            hbox.AddChild(selectButton);
            
            if (playerCombinations.ContainsKey(type) && playerCombinations[type] != null)
            {
                selectButton.Text = "声明";
                selectButton.Disabled = false;
                selectButton.Pressed += () => ToggleDeclaration(type, selectButton, rowPanel);
            }
            else
            {
                selectButton.Text = "无";
                selectButton.Disabled = true;
            }
        }

        /// <summary>
        /// 切换声明选择（只能选择一种）
        /// </summary>
        private void ToggleDeclaration(CombinationType type, Button button, PanelContainer rowPanel)
        {
            var styleBox = rowPanel.GetThemeStylebox("panel") as StyleBoxFlat;
            
            if (selectedDeclarations.Contains(type))
            {
                // 取消选择
                selectedDeclarations.Remove(type);
                button.Text = "声明";
                if (styleBox != null)
                {
                    styleBox.BorderColor = new Color(0.4f, 0.4f, 0.5f);
                }
                GD.Print($"取消声明: {type}");
            }
            else
            {
                // 先清空之前的选择
                selectedDeclarations.Clear();
                
                // 重置所有行的样式
                foreach (Node child in declarationContainer.GetChildren())
                {
                    if (child is PanelContainer panel)
                    {
                        var panelStyle = panel.GetThemeStylebox("panel") as StyleBoxFlat;
                        if (panelStyle != null)
                        {
                            panelStyle.BorderColor = new Color(0.4f, 0.4f, 0.5f);
                        }
                        
                        // 重置按钮文字
                        var hbox = panel.GetChild(0) as HBoxContainer;
                        if (hbox != null)
                        {
                            foreach (Node hboxChild in hbox.GetChildren())
                            {
                                if (hboxChild is Button btn && !btn.Disabled)
                                {
                                    btn.Text = "声明";
                                }
                            }
                        }
                    }
                }
                
                // 选择新的声明
                selectedDeclarations.Add(type);
                button.Text = "已选";
                if (styleBox != null)
                {
                    styleBox.BorderColor = new Color(0.3f, 1f, 0.5f);
                }
                GD.Print($"选择声明: {type}");
            }
        }

        /// <summary>
        /// 确认声明
        /// </summary>
        private void ConfirmDeclaration()
        {
            if (gameManager.CurrentPhase != GamePhase.Declaration)
                return;
            
            GD.Print($"确认声明，选择了 {selectedDeclarations.Count} 种组合");
            
            // 隐藏声明面板
            HideDeclarationPanel();
            
            // 执行声明比较（自动进行）
            gameManager.DeclareAndCompare();
        }

        /// <summary>
        /// 连接游戏管理器的所有信号
        /// </summary>
        private void ConnectGameSignals()
        {
            gameManager.CardsDealt += OnCardsDealt;
            gameManager.PhaseChanged += OnPhaseChanged;
            gameManager.TrickWon += OnTrickWon;
            gameManager.RoundEnded += OnRoundEnded;
            gameManager.GameOver += OnGameOver;
        }

        /// <summary>
        /// 发牌完成后更新显示
        /// </summary>
        private void OnCardsDealt()
        {
            GD.Print("发牌完成，更新UI显示");
            RefreshAllHands();
        }

        /// <summary>
        /// 刷新所有手牌显示（带动画）
        /// </summary>
        private void RefreshAllHands()
        {
            // 增加发牌会话ID，使之前的定时器失效
            dealingSessionId++;
            

            // 显示玩家手牌（正面，可交互，带动画）
            ShowHandWithAnimation(playerHand, gameManager.Player1.Cards, true, true);
            
            // 显示电脑手牌（背面，不可交互，带动画）
            ShowHandWithAnimation(computerHand, gameManager.Player2.Cards, false, false);
        }

        /// <summary>
        /// 带动画的显示手牌
        /// </summary>
        private void ShowHandWithAnimation(HBoxContainer container, List<Card> cards, bool faceUp, bool clickable)
        {
            // 先清空
            ClearContainer(container);
            
            // 定义牌堆起始位置（屏幕中央）
            Vector2 deckPosition = new Vector2(960, 540);
            
            // 记录当前会话ID，用于检查定时器是否过期
            int currentSession = dealingSessionId;
            
            // TODO: 播放发牌音效
            // 如果需要添加音效，可以在这里使用 AudioStreamPlayer 播放声音
            var audioPlayer = new AudioStreamPlayer();
            AddChild(audioPlayer);
            audioPlayer.Stream = GD.Load<AudioStream>("res://Src/audio/washcard.wav");
            audioPlayer.Play();
            
            // 逐张创建卡牌并播放动画
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                int cardIndex = i;
                
                // 延迟发牌（每张间隔0.1秒）
                float delay = cardIndex * 0.1f;
                
                GetTree().CreateTimer(delay).Timeout += () =>
                {
                    // 检查会话ID是否仍然有效（防止旧的定时器影响新一局）
                    if (currentSession != dealingSessionId)
                    {
                        return; // 这是旧的定时器，忽略
                    }
                    CreateCardWithAnimation(container, card, faceUp, clickable, deckPosition);
                };
            }
        }

        /// <summary>
        /// 创建单张卡牌并播放飞行动画
        /// </summary>
        private void CreateCardWithAnimation(HBoxContainer container, Card card, bool faceUp, bool clickable, Vector2 startPos)
        {
            // 调试：显示当前容器中的卡牌数量
            int currentCount = container.GetChildCount();
            if (currentCount >= 12)
            {
                GD.PrintErr($"警告：容器已有 {currentCount} 张牌，不再添加新牌");
                return;
            }
            
            var cardVisual = new CardVisual();
            container.AddChild(cardVisual);
            
            // 设置卡牌数据（先设置为背面）
            cardVisual.SetCard(card, false);
            
            // 设置显示属性（增大尺寸以提高SVG渲染质量）
            cardVisual.CustomMinimumSize = new Vector2(180, 270);
            cardVisual.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            cardVisual.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            
            // 如果可点击，添加点击事件
            if (clickable)
            {
                cardVisual.GuiInput += (inputEvent) => OnCardClicked(inputEvent, cardVisual);
            }
            
            // 等待一帧，确保布局计算完成
            CallDeferred(nameof(AnimateCard), cardVisual, faceUp, startPos);
        }

        /// <summary>
        /// 播放卡牌动画
        /// </summary>
        private void AnimateCard(CardVisual cardVisual, bool faceUp, Vector2 startPos)
        {
            if (!IsInstanceValid(cardVisual))
                return;
                
            // 获取卡牌最终位置
            Vector2 finalPosition = cardVisual.GlobalPosition;
            
            // 设置起始位置
            cardVisual.GlobalPosition = startPos;
            cardVisual.Scale = Vector2.Zero;
            
            // 创建Tween动画
            Tween tween = CreateTween();
            tween.SetParallel(true); // 并行执行多个动画
            
            // 位置动画（从牌堆飞到目标位置）
            tween.TweenProperty(cardVisual, "global_position", finalPosition, 0.5)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            
            // 缩放动画（从0放大到1）
            tween.TweenProperty(cardVisual, "scale", Vector2.One, 0.5)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            
            // 如果需要正面朝上，在动画中途翻牌
            if (faceUp)
            {
                tween.Chain();
                tween.TweenCallback(Callable.From(() => 
                {
                    if (IsInstanceValid(cardVisual))
                        cardVisual.SetFaceUp(true);
                })).SetDelay(0.25);
            }
        }

        /// <summary>
        /// 在容器中显示一手牌
        /// </summary>
        private void ShowHand(HBoxContainer container, List<Card> cards, bool faceUp, bool clickable)
        {
            // 增加发牌会话ID，使之前的发牌动画定时器失效
            dealingSessionId++;
            
            // 清空现有卡牌
            ClearContainer(container);

            // 为每张牌创建 CardVisual
            foreach (var card in cards)
            {
                var cardVisual = new CardVisual();
                container.AddChild(cardVisual);
                
                // 设置卡牌数据
                cardVisual.SetCard(card, faceUp);
                
                // 设置显示属性（增大尺寸以提高SVG渲染质量）
                cardVisual.CustomMinimumSize = new Vector2(180, 270);
                cardVisual.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                cardVisual.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                
                // 如果可点击，添加点击事件
                if (clickable)
                {
                    cardVisual.GuiInput += (inputEvent) => OnCardClicked(inputEvent, cardVisual);
                }
            }
            
            GD.Print($"显示了 {cards.Count} 张牌（正面:{faceUp}, 可点击:{clickable}）");
        }

        /// <summary>
        /// 清空容器中的所有节点（立即删除）
        /// </summary>
        private void ClearContainer(Container container)
        {
            // 获取所有子节点的数组（避免在遍历时修改集合）
            var children = container.GetChildren();
            foreach (Node child in children)
            {
                container.RemoveChild(child);
                child.QueueFree();
            }
        }

        /// <summary>
        /// 显示出的牌
        /// </summary>
        private void ShowPlayedCard(MarginContainer area, Card card)
        {
            // 清空现有的牌
            foreach (Node child in area.GetChildren())
            {
                child.QueueFree();
            }
            
            // 创建新的卡牌显示
            var cardVisual = new CardVisual();
            area.AddChild(cardVisual);
            
            // 设置卡牌数据（正面显示）
            cardVisual.SetCard(card, true);
            
            // 设置显示属性
            cardVisual.CustomMinimumSize = new Vector2(180, 270);
            cardVisual.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            cardVisual.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        }

        /// <summary>
        /// 清空出牌区域
        /// </summary>
        private void ClearPlayedCards()
        {
            foreach (Node child in playerOutcardArea.GetChildren())
            {
                child.QueueFree();
            }
            foreach (Node child in comOutcardArea.GetChildren())
            {
                child.QueueFree();
            }
        }

        /// <summary>
        /// 卡牌点击事件处理
        /// </summary>
        private void OnCardClicked(InputEvent inputEvent, CardVisual cardVisual)
        {
            if (inputEvent is InputEventMouseButton mouseEvent && 
                mouseEvent.Pressed && 
                mouseEvent.ButtonIndex == MouseButton.Left)
            {
                HandleCardClick(cardVisual);
            }
        }

        /// <summary>
        /// 处理卡牌点击逻辑（根据当前阶段）
        /// </summary>
        private void HandleCardClick(CardVisual cardVisual)
        {
            Card clickedCard = cardVisual.GetCard();
            
            switch (gameManager.CurrentPhase)
            {
                case GamePhase.Exchanging:
                    // 换牌阶段：选择要换的牌
                    HandleExchangeClick(cardVisual);
                    break;
                    
                case GamePhase.Playing:
                    // 出牌阶段：出牌
                    HandlePlayClick(cardVisual);
                    break;
                    
                default:
                    GD.Print($"当前阶段 {gameManager.CurrentPhase} 不能点击卡牌");
                    break;
            }
        }

        /// <summary>
        /// 换牌阶段的点击处理
        /// </summary>
        private void HandleExchangeClick(CardVisual cardVisual)
        {
            Card card = cardVisual.GetCard();
            
            // 检查是否已经选中
            if (selectedCards.Contains(cardVisual))
            {
                // 取消选中
                selectedCards.Remove(cardVisual);
                cardVisual.Position = new Vector2(cardVisual.Position.X, 0); // 恢复位置
                GD.Print($"取消选择: {card}，已选{selectedCards.Count}张");
            }
            else
            {
                // 检查是否超过最大换牌数
                if (selectedCards.Count >= maxExchangeCount)
                {
                    GD.Print($"最多只能选择{maxExchangeCount}张牌进行交换！");
                    return;
                }
                
                // 选中卡牌（向上移动表示选中）
                selectedCards.Add(cardVisual);
                cardVisual.Position = new Vector2(cardVisual.Position.X, -30); // 向上移动
                GD.Print($"选择换牌: {card}，已选{selectedCards.Count}张");
            }
            
            // 更新状态提示
            UpdateStatus("🔄 换牌阶段", $"已选 {selectedCards.Count}/{maxExchangeCount} 张，按 Enter 确认换牌");
        }

        /// <summary>
        /// 出牌阶段的点击处理
        /// </summary>
        private void HandlePlayClick(CardVisual cardVisual)
        {
            Card card = cardVisual.GetCard();
            
            // 检查是否是玩家回合
            if (gameManager.CurrentPlayer != gameManager.Player1)
            {
                GD.Print("现在不是你的回合！");
                return;
            }
            
            // 尝试出牌
            bool success = gameManager.PlayCard(gameManager.Player1, card);
            
            if (success)
            {
                GD.Print($"你出了: {card}");
                
                // 显示玩家出的牌
                ShowPlayedCard(playerOutcardArea, card);
                
                // 刷新玩家手牌显示
                ShowHand(playerHand, gameManager.Player1.Cards, true, true);
                // 刷新电脑手牌显示
                ShowHand(computerHand, gameManager.Player2.Cards, false, false);
                
                // 检查游戏阶段，如果还在出牌阶段
                if (gameManager.CurrentPhase == GamePhase.Playing)
                {
                    // 更新状态
                    UpdatePlayingStatus();
                    
                    // 如果是电脑回合，延迟后自动出牌
                    if (gameManager.CurrentPlayer == gameManager.Player2)
                    {
                        CallDeferred(nameof(ComputerAutoPlay));
                    }
                }
            }
        }

        /// <summary>
        /// 电脑自动出牌
        /// </summary>
        private void ComputerAutoPlay()
        {
            // 如果正在显示赢墩结果，等待结果显示完毕
            if (isShowingTrickResult)
                return;
            
            // 显示电脑正在思考
            UpdateStatus("⏳ 电脑出牌中...", "电脑正在考虑出哪张牌...");
            
            // 延迟1.2秒，让玩家看到电脑在思考
            GetTree().CreateTimer(1.2).Timeout += () =>
            {
                if (gameManager.CurrentPhase != GamePhase.Playing)
                    return;
                
                // 再次检查是否正在显示赢墩结果
                if (isShowingTrickResult)
                    return;
                    
                if (gameManager.CurrentPlayer == gameManager.Player2)
                {
                    // 简单AI：随机出一张合法的牌
                    Card cardToPlay = SelectComputerCard();
                    
                    if (cardToPlay != null)
                    {
                        GD.Print($"电脑出牌: {cardToPlay}");
                        gameManager.PlayCard(gameManager.Player2, cardToPlay);
                        
                        // 显示电脑出的牌
                        ShowPlayedCard(comOutcardArea, cardToPlay);
                        
                        // 刷新电脑手牌显示
                        ShowHand(computerHand, gameManager.Player2.Cards, false, false);
                        
                        // 刷新玩家手牌
                        ShowHand(playerHand, gameManager.Player1.Cards, true, true);
                        
                        // 更新状态
                        if (gameManager.CurrentPhase == GamePhase.Playing)
                        {
                            UpdatePlayingStatus();
                        }
                        
                        // 注意：如果电脑赢得牌墩，会在HideTrickResult中触发继续出牌
                    }
                }
            };
        }

        /// <summary>
        /// 简单AI选择出牌（选择第一张合法的牌）
        /// </summary>
        private Card SelectComputerCard()
        {
            var currentTrick = gameManager.GetCurrentTrick();
            var computerCards = gameManager.Player2.Cards;
            
            if (computerCards.Count == 0)
                return null;
            
            // 如果是领牌，随便出一张
            if (currentTrick.Count == 0)
            {
                return computerCards[0];
            }
            
            // 如果是跟牌，优先出相同花色
            Card leadCard = currentTrick[0];
            var sameSuitCards = computerCards.Where(c => c.Suit == leadCard.Suit).ToList();
            
            if (sameSuitCards.Count > 0)
            {
                return sameSuitCards[0];
            }
            
            // 没有相同花色，随便出一张
            return computerCards[0];
        }

        /// <summary>
        /// 阶段变更处理
        /// </summary>
        private void OnPhaseChanged(GamePhase newPhase)
        {
            GD.Print($"阶段切换: {newPhase}");
            
            switch (newPhase)
            {
                case GamePhase.Dealing:
                    UpdateStatus("🎴 发牌阶段", "正在发牌...");
                    break;
                    
                case GamePhase.Exchanging:
                    // 进入换牌阶段
                    StartExchangePhase();
                    break;
                    
                case GamePhase.Declaration:
                    // 声明阶段：显示声明面板，等待玩家选择
                    GD.Print("\n=== 声明阶段 ===");
                    UpdateStatus("📢 声明阶段", "选择要声明的组合，按 Enter 确认");
                    // 自动显示声明面板
                    ShowDeclarationPanel();
                    break;
                    
                case GamePhase.Playing:
                    // 出牌阶段，刷新显示（不需要动画，只是更新卡牌显示）
                    ShowHand(playerHand, gameManager.Player1.Cards, true, true);
                    ShowHand(computerHand, gameManager.Player2.Cards, false, false);
                    UpdatePlayingStatus();
                    
                    // 如果电脑先出牌
                    if (gameManager.CurrentPlayer == gameManager.Player2)
                    {
                        CallDeferred(nameof(ComputerAutoPlay));
                    }
                    break;
                    
                case GamePhase.Scoring:
                    UpdateStatus("📊 计分阶段", "正在计算得分...");
                    break;
                    
                case GamePhase.GameOver:
                    UpdateStatus("🏁 游戏结束", "按 R 键重新开始");
                    break;
            }
        }
        
        /// <summary>
        /// 更新出牌阶段的状态提示
        /// </summary>
        private void UpdatePlayingStatus()
        {
            var (p1Tricks, p2Tricks) = gameManager.GetTrickCounts();
            bool isPlayerTurn = gameManager.CurrentPlayer == gameManager.Player1;
            
            string turnText = isPlayerTurn ? "🎯 轮到你出牌" : "⏳ 电脑出牌中...";
            string hintText = isPlayerTurn ? 
                $"点击一张牌出牌 | 你: {p1Tricks}墩  电脑: {p2Tricks}墩" : 
                $"等待电脑出牌... | 你: {p1Tricks}墩  电脑: {p2Tricks}墩";
            
            UpdateStatus(turnText, hintText);
        }
        
        /// <summary>
        /// 开始换牌阶段
        /// </summary>
        private void StartExchangePhase()
        {
            GD.Print("\n=== 换牌阶段 ===");
            
            // 重置换牌状态
            isPlayerExchangeDone = false;
            isComputerExchangeDone = false;
            selectedCards.Clear();
            
            // 确定谁先换牌（非发牌员先换，最多5张）
            bool playerIsNonDealer = gameManager.Player1 == gameManager.GetNonDealer();
            
            if (playerIsNonDealer)
            {
                // 玩家是非发牌员，玩家先换（最多5张）
                maxExchangeCount = 5;
                UpdateStatus("🔄 换牌阶段 - 你先换牌", $"点击手牌选择要换的牌（最多{maxExchangeCount}张），按 Enter 确认");
                GD.Print("你是非发牌员，请先选择要换的牌（最多5张）");
            }
            else
            {
                // 电脑是非发牌员，电脑先换
                UpdateStatus("🔄 换牌阶段", "电脑正在换牌...");
                GD.Print("电脑是非发牌员，电脑先换牌...");
                ComputerExchange(true); // true = 非发牌员（最多5张）
            }
        }
        
        /// <summary>
        /// 电脑自动换牌
        /// </summary>
        private void ComputerExchange(bool isNonDealer)
        {
            // 显示电脑正在思考
            UpdateStatus("🤖 电脑换牌中...", "电脑正在考虑换哪些牌...");
            
            // 延迟1.5秒，让玩家看到电脑在操作
            GetTree().CreateTimer(1.5).Timeout += () =>
            {
                // 电脑简单策略：换掉最小的几张牌
                var computerCards = gameManager.Player2.Cards.OrderBy(c => c.GetValue()).ToList();
                // 非发牌员最多5张，发牌员取决于剩余底牌
                int maxExchange = isNonDealer ? 5 : gameManager.GetTalonCount();
                int exchangeCount = Math.Min(Math.Min(maxExchange, 3), gameManager.GetTalonCount()); // 电脑换掉最小的3张
                
                if (exchangeCount > 0 && gameManager.GetTalonCount() > 0)
                {
                    var cardsToExchange = computerCards.Take(exchangeCount).ToList();
                    gameManager.ExchangeCards(gameManager.Player2, cardsToExchange);
                    GD.Print($"电脑换了{exchangeCount}张牌");
                }
                else
                {
                    GD.Print("电脑不换牌");
                }
                
                isComputerExchangeDone = true;
                
                // 检查是否轮到玩家
                if (!isPlayerExchangeDone)
                {
                    maxExchangeCount = gameManager.GetTalonCount();
                    if (maxExchangeCount > 0)
                    {
                        UpdateStatus("🔄 换牌阶段 - 轮到你换牌", $"点击手牌选择要换的牌（最多{maxExchangeCount}张），按 Enter 确认");
                        GD.Print($"\n轮到你换牌了！底牌剩余{maxExchangeCount}张");
                        // 刷新显示
                        ShowHand(playerHand, gameManager.Player1.Cards, true, true);
                    }
                    else
                    {
                        GD.Print("底牌已用完，你无法换牌");
                        isPlayerExchangeDone = true;
                        FinishExchangePhase();
                    }
                }
                else
                {
                    FinishExchangePhase();
                }
            };
        }
        
        /// <summary>
        /// 玩家确认换牌
        /// </summary>
        private void ConfirmPlayerExchange()
        {
            if (gameManager.CurrentPhase != GamePhase.Exchanging || isPlayerExchangeDone)
                return;
            
            // 收集选中的手牌
            var cardsToExchange = selectedCards.Select(cv => cv.GetCard()).ToList();
            
            // 检查数量是否合法
            if (cardsToExchange.Count > maxExchangeCount)
            {
                GD.Print($"最多只能换{maxExchangeCount}张牌！");
                return;
            }
            
            if (cardsToExchange.Count > gameManager.GetTalonCount())
            {
                GD.Print($"底牌只剩{gameManager.GetTalonCount()}张，无法换{cardsToExchange.Count}张！");
                return;
            }
            
            if (cardsToExchange.Count > 0)
            {
                gameManager.ExchangeCards(gameManager.Player1, cardsToExchange);
                GD.Print($"你换了{cardsToExchange.Count}张牌");
            }
            else
            {
                GD.Print("你选择不换牌");
            }
            
            // 清空选中状态
            selectedCards.Clear();
            isPlayerExchangeDone = true;
            
            // 刷新玩家手牌显示
            ShowHand(playerHand, gameManager.Player1.Cards, true, true);
            
            // 检查电脑是否需要换牌
            bool playerIsNonDealer = gameManager.Player1 == gameManager.GetNonDealer();
            
            if (playerIsNonDealer && !isComputerExchangeDone)
            {
                // 玩家是非发牌员，玩家先换完，轮到电脑（电脑是发牌员）
                GD.Print("\n轮到电脑换牌...");
                ComputerExchange(false); // false = 发牌员
            }
            else
            {
                FinishExchangePhase();
            }
        }
        
        /// <summary>
        /// 完成换牌阶段
        /// </summary>
        private void FinishExchangePhase()
        {
            if (!isPlayerExchangeDone || !isComputerExchangeDone)
                return;
                
            GD.Print("\n换牌阶段结束");
            
            // 检查白牌（无人像牌）
            gameManager.CompleteExchange();
        }

        /// <summary>
        /// 牌墩赢得处理
        /// </summary>
        private void OnTrickWon(string playerName, int trickScore, int totalTricks)
        {
            // 检查是否是最后一墩（两边手牌都打完了）
            isLastTrick = gameManager.Player1.CardCount() == 0 && gameManager.Player2.CardCount() == 0;
            
            string lastTrickText = isLastTrick ? " [最后一墩]" : "";
            GD.Print($">>> {playerName} 赢得牌墩！+{trickScore}分 (累计{totalTricks}墩){lastTrickText}");
            
            // 显示赢墩结果面板
            ShowTrickResult(playerName, trickScore, totalTricks, isLastTrick);
            
            // 最后一墩显示时间更长
            float delay = isLastTrick ? LAST_TRICK_DELAY : NORMAL_TRICK_DELAY;
            
            // 延迟后隐藏结果面板并清空出牌区域
            GetTree().CreateTimer(delay).Timeout += () =>
            {
                HideTrickResult();
                ClearPlayedCards();
            };
        }

        /// <summary>
        /// 回合结束处理
        /// </summary>
        private void OnRoundEnded()
        {
            GD.Print("=== 回合结束 ===");
            var (p1Tricks, p2Tricks) = gameManager.GetTrickCounts();
            GD.Print($"牌墩统计 - 玩家: {p1Tricks}墩, 电脑: {p2Tricks}墩");
            GD.Print($"当前得分 - 玩家: {gameManager.Player1.Score}分, 电脑: {gameManager.Player2.Score}分");
            
            // 游戏管理器在触发OnRoundEnded时可能已经增加了CurrentRound，所以需要减1来显示刚结束的回合
            int roundToShow = gameManager.CurrentRound - 1;
            int p1ScoreToShow = gameManager.Player1.Score;
            int p2ScoreToShow = gameManager.Player2.Score;
            
            // 等待最后一墩结果显示完毕后再显示回合结束面板
            // 需要额外等待一点时间确保墩结果面板完全显示后再切换
            float waitTime = LAST_TRICK_DELAY + 0.5f;
            GetTree().CreateTimer(waitTime).Timeout += () =>
            {
                // 显示回合结束面板（使用保存的回合数）
                ShowRoundEnd(p1Tricks, p2Tricks, 
                            p1ScoreToShow, p2ScoreToShow, 
                            roundToShow);
            };
        }

        /// <summary>
        /// 游戏结束处理
        /// </summary>
        private void OnGameOver(string winner)
        {
            GD.Print("╔════════════════════════════════╗");
            GD.Print("║       游戏结束！Game Over!      ║");
            GD.Print("╚════════════════════════════════╝");
            GD.Print($"最终得分:");
            GD.Print($"  玩家: {gameManager.Player1.Score}分");
            GD.Print($"  电脑: {gameManager.Player2.Score}分");
            GD.Print($"获胜者: {winner}");
            
            // 显示游戏结束面板
            ShowGameOver(winner, gameManager.Player1.Score, gameManager.Player2.Score);
        }

        /// <summary>
        /// 键盘输入处理（用于测试）
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Space:
                        // 空格键：刷新显示
                        GD.Print("刷新显示");
                        RefreshAllHands();
                        break;
                        
                    case Key.R:
                        // R键：重新开始
                        GD.Print("重新开始游戏");
                        HideDeclarationPanel();
                        selectedCards.Clear();
                        selectedDeclarations.Clear();
                        gameManager.InitializeGame();
                        break;
                        
                    case Key.Enter:
                    case Key.KpEnter:
                        // Enter键：根据当前阶段确认操作
                        if (gameManager.CurrentPhase == GamePhase.Exchanging && !isPlayerExchangeDone)
                        {
                            ConfirmPlayerExchange();
                        }
                        else if (gameManager.CurrentPhase == GamePhase.Declaration)
                        {
                            ConfirmDeclaration();
                        }
                        break;
                        
                    case Key.Escape:
                        // Escape键：关闭面板
                        if (isDeclarationPanelVisible)
                        {
                            HideDeclarationPanel();
                        }
                        break;
                }
            }
        }
    }
}

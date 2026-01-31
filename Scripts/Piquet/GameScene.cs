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
        
        // 游戏管理器
        private PiquetGameManager gameManager;
        
        // 当前选中的卡牌（用于换牌等）
        private List<CardVisual> selectedCards = new List<CardVisual>();

        public override void _Ready()
        {
            GD.Print("游戏场景初始化...");
            
            // 获取场景中的容器节点（使用完整路径）
            playerHand = GetNode<HBoxContainer>("PlayerHandArea/Playerhand");
            computerHand = GetNode<HBoxContainer>("ComHandArea/Comhand");
            
            // 创建游戏管理器
            gameManager = new PiquetGameManager();
            AddChild(gameManager);
            
            // 连接游戏事件信号
            ConnectGameSignals();
            
            GD.Print("场景初始化完成，等待游戏开始...");
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
            
            // 逐张创建卡牌并播放动画
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = cards[i];
                int cardIndex = i;
                
                // 延迟发牌（每张间隔0.1秒）
                float delay = cardIndex * 0.1f;
                
                GetTree().CreateTimer(delay).Timeout += () =>
                {
                    CreateCardWithAnimation(container, card, faceUp, clickable, deckPosition);
                };
            }
        }

        /// <summary>
        /// 创建单张卡牌并播放飞行动画
        /// </summary>
        private void CreateCardWithAnimation(HBoxContainer container, Card card, bool faceUp, bool clickable, Vector2 startPos)
        {
            var cardVisual = new CardVisual();
            container.AddChild(cardVisual);
            
            // 设置卡牌数据（先设置为背面）
            cardVisual.SetCard(card, false);
            
            // 设置显示属性（增大尺寸以提高SVG渲染质量）
            cardVisual.CustomMinimumSize = new Vector2(100, 150);
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
                cardVisual.CustomMinimumSize = new Vector2(100, 150);
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
        /// 清空容器中的所有节点
        /// </summary>
        private void ClearContainer(Container container)
        {
            foreach (Node child in container.GetChildren())
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
            // TODO: 实现换牌逻辑
            GD.Print($"选择换牌: {cardVisual.GetCard()}");
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
                // 刷新玩家手牌显示
                ShowHand(playerHand, gameManager.Player1.Cards, true, true);
                
                // 如果是电脑回合，延迟后自动出牌
                if (gameManager.CurrentPlayer == gameManager.Player2)
                {
                    CallDeferred(nameof(ComputerAutoPlay));
                }
            }
        }

        /// <summary>
        /// 电脑自动出牌
        /// </summary>
        private void ComputerAutoPlay()
        {
            // 等待一小段时间让玩家看到
            GetTree().CreateTimer(1.0).Timeout += () =>
            {
                if (gameManager.CurrentPlayer == gameManager.Player2 && 
                    gameManager.CurrentPhase == GamePhase.Playing)
                {
                    // 简单AI：随机出一张合法的牌
                    Card cardToPlay = SelectComputerCard();
                    
                    if (cardToPlay != null)
                    {
                        gameManager.PlayCard(gameManager.Player2, cardToPlay);
                        
                        // 刷新电脑手牌显示
                        ShowHand(computerHand, gameManager.Player2.Cards, false, false);
                        
                        // 刷新玩家手牌（可能轮到玩家了）
                        ShowHand(playerHand, gameManager.Player1.Cards, true, true);
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
                case GamePhase.Exchanging:
                    // 进入换牌阶段，暂时自动跳过
                    GD.Print("换牌阶段（暂时跳过）");
                    gameManager.CompleteExchange();
                    break;
                    
                case GamePhase.Declaration:
                    // 自动进行声明
                    gameManager.DeclareAndCompare();
                    break;
                    
                case GamePhase.Playing:
                    // 出牌阶段，刷新显示
                    RefreshAllHands();
                    
                    // 如果电脑先出牌
                    if (gameManager.CurrentPlayer == gameManager.Player2)
                    {
                        CallDeferred(nameof(ComputerAutoPlay));
                    }
                    break;
            }
        }

        /// <summary>
        /// 牌墩赢得处理
        /// </summary>
        private void OnTrickWon(string playerName)
        {
            GD.Print($"{playerName} 赢得牌墩");
        }

        /// <summary>
        /// 回合结束处理
        /// </summary>
        private void OnRoundEnded()
        {
            GD.Print("回合结束");
        }

        /// <summary>
        /// 游戏结束处理
        /// </summary>
        private void OnGameOver(string winner)
        {
            GD.Print($"游戏结束！获胜者：{winner}");
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
                        gameManager.InitializeGame();
                        break;
                }
            }
        }
    }
}

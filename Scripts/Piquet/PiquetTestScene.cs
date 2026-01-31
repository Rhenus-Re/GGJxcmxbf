using Godot;
using System;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 皮克牌测试场景 - 用于测试游戏逻辑
    /// </summary>
    public partial class PiquetTestScene : Node
    {
        private PiquetGameManager gameManager;

        public override void _Ready()
        {
            GD.Print("皮克牌测试场景启动");
            
            // 创建游戏管理器
            gameManager = new PiquetGameManager();
            AddChild(gameManager);
            
            // 连接信号
            gameManager.PhaseChanged += OnPhaseChanged;
            gameManager.CardsDealt += OnCardsDealt;
            gameManager.ExchangeComplete += OnExchangeComplete;
            gameManager.DeclarationComplete += OnDeclarationComplete;
            gameManager.TrickWon += OnTrickWon;
            gameManager.RoundEnded += OnRoundEnded;
            gameManager.GameOver += OnGameOver;
        }

        public override void _Input(InputEvent @event)
        {
            // 按空格键进行自动测试
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
            {
                RunAutomatedTest();
            }
            
            // 按R键重新开始
            if (@event is InputEventKey keyEvent2 && keyEvent2.Pressed && keyEvent2.Keycode == Key.R)
            {
                gameManager.InitializeGame();
            }
            
            // 按P键打印游戏状态
            if (@event is InputEventKey keyEvent3 && keyEvent3.Pressed && keyEvent3.Keycode == Key.P)
            {
                gameManager.PrintGameState();
            }
        }

        /// <summary>
        /// 运行自动化测试
        /// </summary>
        private void RunAutomatedTest()
        {
            GD.Print("\n开始自动化测试...");
            
            switch (gameManager.CurrentPhase)
            {
                case GamePhase.Exchanging:
                    AutoExchange();
                    break;
                    
                case GamePhase.Declaration:
                    gameManager.DeclareAndCompare();
                    break;
                    
                case GamePhase.Playing:
                    AutoPlay();
                    break;
            }
        }

        /// <summary>
        /// 自动换牌（示例：不换牌）
        /// </summary>
        private void AutoExchange()
        {
            GD.Print("自动换牌（跳过）");
            // 玩家可以选择不换牌
            gameManager.CompleteExchange();
        }

        /// <summary>
        /// 自动出牌（智能选择合法的牌）
        /// </summary>
        private void AutoPlay()
        {
            var currentPlayer = gameManager.CurrentPlayer;
            if (currentPlayer.CardCount() == 0)
                return;

            Card cardToPlay = null;
            var currentTrick = gameManager.GetCurrentTrick();

            // 如果是领牌（第一张牌），随便出一张
            if (currentTrick.Count == 0)
            {
                cardToPlay = currentPlayer.Cards[0];
            }
            // 如果是跟牌（第二张牌），需要遵循跟牌规则
            else if (currentTrick.Count == 1)
            {
                Card leadCard = currentTrick[0];
                
                // 优先选择相同花色的牌
                var sameSuitCards = currentPlayer.Cards.Where(c => c.Suit == leadCard.Suit).ToList();
                
                if (sameSuitCards.Count > 0)
                {
                    // 有相同花色，随机选一张
                    cardToPlay = sameSuitCards[0];
                }
                else
                {
                    // 没有相同花色，随便出一张（反正必输）
                    cardToPlay = currentPlayer.Cards[0];
                }
            }

            if (cardToPlay != null)
            {
                gameManager.PlayCard(currentPlayer, cardToPlay);
            }
        }

        // 信号处理
        private void OnPhaseChanged(GamePhase phase)
        {
            GD.Print($">> 阶段变更: {phase}");
        }

        private void OnCardsDealt()
        {
            GD.Print(">> 发牌完成");
            gameManager.PrintGameState();
        }

        private void OnExchangeComplete()
        {
            GD.Print(">> 换牌完成");
        }

        private void OnDeclarationComplete()
        {
            GD.Print(">> 声明完成");
        }

        private void OnTrickWon(string playerName)
        {
            GD.Print($">> {playerName} 赢得牌墩");
        }

        private void OnRoundEnded()
        {
            GD.Print(">> 回合结束");
        }

        private void OnGameOver(string winner)
        {
            GD.Print($">> 游戏结束！获胜者：{winner}");
        }
    }
}

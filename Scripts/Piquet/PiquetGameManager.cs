using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 游戏阶段
    /// </summary>
    public enum GamePhase
    {
        Setup,          // 准备阶段
        Dealing,        // 发牌阶段
        Exchanging,     // 换牌阶段
        Declaration,    // 声明组合阶段
        Playing,        // 出牌阶段
        Scoring,        // 计分阶段
        GameOver        // 游戏结束
    }

    /// <summary>
    /// 玩家角色
    /// </summary>
    public enum PlayerRole
    {
        Dealer,         // 发牌员
        NonDealer       // 非发牌员
    }

    /// <summary>
    /// 皮克牌游戏管理器
    /// </summary>
    public partial class PiquetGameManager : Node
    {
        // 游戏状态
        public GamePhase CurrentPhase { get; private set; }
        public int CurrentRound { get; private set; }
        public int TotalRounds { get; set; } = 6;
        public int TargetScore { get; set; } = 100;

        // 玩家
        public PlayerHand Player1 { get; private set; }
        public PlayerHand Player2 { get; private set; }
        public PlayerRole Player1Role { get; private set; }
        
        // 牌堆
        private Deck deck;
        private List<Card> talon;  // 底牌（8张）

        // 当前回合信息
        public PlayerHand CurrentPlayer { get; private set; }
        private List<Card> currentTrick;  // 当前牌墩
        private PlayerHand trickWinner;   // 牌墩赢家
        private int player1Tricks;
        private int player2Tricks;

        // 声明结果
        private Dictionary<CombinationType, PlayerHand> declarationWinners;

        // 信号（用于UI更新）
        [Signal]
        public delegate void PhaseChangedEventHandler(GamePhase newPhase);
        
        [Signal]
        public delegate void CardsDealtEventHandler();
        
        [Signal]
        public delegate void ExchangeCompleteEventHandler();
        
        [Signal]
        public delegate void DeclarationCompleteEventHandler();
        
        [Signal]
        public delegate void TrickWonEventHandler(string playerName, int trickScore, int totalTricks);
        
        [Signal]
        public delegate void RoundEndedEventHandler();
        
        [Signal]
        public delegate void GameOverEventHandler(string winner);

        public override void _Ready()
        {
            InitializeGame();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitializeGame()
        {
            GD.Print("=== 皮克牌游戏开始 ===");
            
            Player1 = new PlayerHand("玩家1");
            Player2 = new PlayerHand("玩家2");
            deck = new Deck();
            talon = new List<Card>();
            currentTrick = new List<Card>();
            declarationWinners = new Dictionary<CombinationType, PlayerHand>();
            
            CurrentRound = 1;
            Player1Role = PlayerRole.NonDealer;  // 第一局玩家1为非发牌方
            
            CurrentPhase = GamePhase.Setup;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            
            StartNewRound();
        }

        /// <summary>
        /// 开始新回合
        /// </summary>
        public void StartNewRound()
        {
            GD.Print($"\n--- 第 {CurrentRound} 局开始 ---");
            
            // 清理上一局
            Player1.Clear();
            Player2.Clear();
            talon.Clear();
            currentTrick.Clear();
            declarationWinners.Clear();
            player1Tricks = 0;
            player2Tricks = 0;
            
            // 重置并洗牌
            deck.Reset();
            deck.Shuffle();
            
            // 进入发牌阶段
            CurrentPhase = GamePhase.Dealing;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
            
            DealCards();
        }

        /// <summary>
        /// 发牌（每人12张，底牌8张）
        /// </summary>
        private void DealCards()
        {
            GD.Print("开始发牌...");
            
            // 确定发牌顺序（非发牌员先拿牌）
            PlayerHand firstPlayer = Player1Role == PlayerRole.NonDealer ? Player1 : Player2;
            PlayerHand secondPlayer = Player1Role == PlayerRole.Dealer ? Player1 : Player2;
            
            // 发牌：交替发给两位玩家，每人12张
            // 传统皮克牌发牌方式：每次发2或3张，共发4轮，每人12张
            // 第1轮: 2张, 第2轮: 3张, 第3轮: 3张, 第4轮: 4张 (2+3+3+4=12)
            // 或者简化为：每人直接发12张
            firstPlayer.AddCards(deck.DrawCards(12));
            secondPlayer.AddCards(deck.DrawCards(12));
            
            // 剩余8张作为底牌
            for (int i = 0; i < 8; i++)
            {
                talon.Add(deck.DrawCard());
            }
            
            Player1.SortCards();
            Player2.SortCards();
            
            GD.Print($"发牌完成 - {Player1.PlayerName}: {Player1.CardCount()}张, {Player2.PlayerName}: {Player2.CardCount()}张, 底牌: {talon.Count}张");
            EmitSignal(SignalName.CardsDealt);
            
            // 进入换牌阶段
            CurrentPhase = GamePhase.Exchanging;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }

        /// <summary>
        /// 玩家换牌
        /// </summary>
        public bool ExchangeCards(PlayerHand player, List<Card> cardsToDiscard)
        {
            if (CurrentPhase != GamePhase.Exchanging)
            {
                GD.PrintErr("当前不是换牌阶段");
                return false;
            }

            int maxExchange = player == GetDealer() ? 5 : talon.Count;
            
            if (cardsToDiscard.Count > maxExchange || cardsToDiscard.Count > talon.Count)
            {
                GD.PrintErr($"换牌数量超过限制（最多{maxExchange}张）");
                return false;
            }

            // 从底牌中抽牌
            List<Card> newCards = talon.Take(cardsToDiscard.Count).ToList();
            talon.RemoveRange(0, cardsToDiscard.Count);
            
            // 替换手牌
            player.ExchangeCards(cardsToDiscard, newCards);
            
            GD.Print($"{player.PlayerName} 换了 {cardsToDiscard.Count} 张牌");
            
            return true;
        }

        /// <summary>
        /// 完成换牌阶段
        /// </summary>
        public void CompleteExchange()
        {
            // 检查白牌（无人像牌）
            if (!GetNonDealer().HasFaceCards())
            {
                GetNonDealer().Score += 10;
                GD.Print($"{GetNonDealer().PlayerName} 声明白牌，获得10分！");
            }
            
            EmitSignal(SignalName.ExchangeComplete);
            
            // 进入声明阶段
            CurrentPhase = GamePhase.Declaration;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }

        /// <summary>
        /// 声明组合并比较
        /// </summary>
        public void DeclareAndCompare()
        {
            GD.Print("\n=== 声明组合阶段 ===");
            
            var player1Combos = CombinationAnalyzer.GetBestCombinations(Player1);
            var player2Combos = CombinationAnalyzer.GetBestCombinations(Player2);
            
            // 比较三种组合
            CompareCombination(CombinationType.Point, player1Combos, player2Combos);
            CompareCombination(CombinationType.Sequence, player1Combos, player2Combos);
            CompareCombination(CombinationType.Set, player1Combos, player2Combos);
            
            GD.Print($"\n声明后分数 - {Player1}: {Player1.Score}分, {Player2}: {Player2.Score}分");
            
            EmitSignal(SignalName.DeclarationComplete);
            
            // 进入出牌阶段
            CurrentPhase = GamePhase.Playing;
            
            // 非发牌员先出牌 - 必须在发射信号前设置，这样UI才能正确显示
            CurrentPlayer = GetNonDealer();
            GD.Print($"出牌阶段开始，先手玩家: {CurrentPlayer.PlayerName}");
            
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);
        }

        /// <summary>
        /// 比较某一类型的组合
        /// </summary>
        private void CompareCombination(CombinationType type, 
            Dictionary<CombinationType, Combination> player1Combos,
            Dictionary<CombinationType, Combination> player2Combos)
        {
            Combination combo1 = player1Combos.ContainsKey(type) ? player1Combos[type] : null;
            Combination combo2 = player2Combos.ContainsKey(type) ? player2Combos[type] : null;
            
            int comparison = CombinationAnalyzer.CompareCombinations(combo1, combo2);
            
            string typeName = type.ToString();
            
            if (comparison > 0)
            {
                Player1.Score += combo1.Score;
                declarationWinners[type] = Player1;
                GD.Print($"{typeName}: {Player1.PlayerName} 获胜 - {combo1} (+{combo1.Score}分)");
            }
            else if (comparison < 0)
            {
                Player2.Score += combo2.Score;
                declarationWinners[type] = Player2;
                GD.Print($"{typeName}: {Player2.PlayerName} 获胜 - {combo2} (+{combo2.Score}分)");
            }
            else if (combo1 != null && combo2 != null)
            {
                GD.Print($"{typeName}: 平局 - 双方相等，都不得分");
            }
        }

        /// <summary>
        /// 获取发牌员
        /// </summary>
        public PlayerHand GetDealer()
        {
            return Player1Role == PlayerRole.Dealer ? Player1 : Player2;
        }

        /// <summary>
        /// 获取非发牌员
        /// </summary>
        public PlayerHand GetNonDealer()
        {
            return Player1Role == PlayerRole.NonDealer ? Player1 : Player2;
        }

        /// <summary>
        /// 获取底牌剩余数量
        /// </summary>
        public int GetTalonCount()
        {
            return talon.Count;
        }

        /// <summary>
        /// 获取底牌列表（用于显示）
        /// </summary>
        public List<Card> GetTalon()
        {
            return talon;
        }
        /// <summary>
        /// 打印游戏状态
        /// </summary>
        public void PrintGameState()
        {
            GD.Print($"\n=== 游戏状态 ===");
            GD.Print($"回合: {CurrentRound}/{TotalRounds}");
            GD.Print($"阶段: {CurrentPhase}");
            GD.Print($"{Player1}");
            GD.Print($"{Player2}");
            GD.Print($"底牌剩余: {talon.Count}张");
        }
    }
}

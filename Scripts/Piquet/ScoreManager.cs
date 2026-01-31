using Godot;
using System;
using System.Collections.Generic;

namespace PiquetGame
{
    /// <summary>
    /// 计分系统 - 跟踪和管理游戏分数
    /// </summary>
    public class ScoreManager
    {
        public class RoundScore
        {
            public int RoundNumber { get; set; }
            public int Player1PointScore { get; set; }
            public int Player1SequenceScore { get; set; }
            public int Player1SetScore { get; set; }
            public int Player1TrickScore { get; set; }
            public int Player1Total { get; set; }

            public int Player2PointScore { get; set; }
            public int Player2SequenceScore { get; set; }
            public int Player2SetScore { get; set; }
            public int Player2TrickScore { get; set; }
            public int Player2Total { get; set; }

            public override string ToString()
            {
                return $"第{RoundNumber}局:\n" +
                       $"  玩家1: 牌点{Player1PointScore} + 顺子{Player1SequenceScore} + 长套{Player1SetScore} + 赢墩{Player1TrickScore} = {Player1Total}分\n" +
                       $"  玩家2: 牌点{Player2PointScore} + 顺子{Player2SequenceScore} + 长套{Player2SetScore} + 赢墩{Player2TrickScore} = {Player2Total}分";
            }
        }

        private List<RoundScore> roundHistory;
        private int player1TotalScore;
        private int player2TotalScore;

        public ScoreManager()
        {
            roundHistory = new List<RoundScore>();
            player1TotalScore = 0;
            player2TotalScore = 0;
        }

        /// <summary>
        /// 记录一局的分数
        /// </summary>
        public void RecordRound(int roundNumber, 
            int p1Point, int p1Seq, int p1Set, int p1Trick,
            int p2Point, int p2Seq, int p2Set, int p2Trick)
        {
            var round = new RoundScore
            {
                RoundNumber = roundNumber,
                Player1PointScore = p1Point,
                Player1SequenceScore = p1Seq,
                Player1SetScore = p1Set,
                Player1TrickScore = p1Trick,
                Player1Total = p1Point + p1Seq + p1Set + p1Trick,
                
                Player2PointScore = p2Point,
                Player2SequenceScore = p2Seq,
                Player2SetScore = p2Set,
                Player2TrickScore = p2Trick,
                Player2Total = p2Point + p2Seq + p2Set + p2Trick
            };

            roundHistory.Add(round);
            player1TotalScore += round.Player1Total;
            player2TotalScore += round.Player2Total;
        }

        /// <summary>
        /// 获取总分
        /// </summary>
        public (int player1, int player2) GetTotalScores()
        {
            return (player1TotalScore, player2TotalScore);
        }

        /// <summary>
        /// 打印完整得分历史
        /// </summary>
        public void PrintScoreHistory()
        {
            GD.Print("\n╔════════════════════════════════════════╗");
            GD.Print("║           得分历史记录                  ║");
            GD.Print("╚════════════════════════════════════════╝");
            
            foreach (var round in roundHistory)
            {
                GD.Print(round.ToString());
            }
            
            GD.Print($"\n累计总分:");
            GD.Print($"  玩家1: {player1TotalScore}分");
            GD.Print($"  玩家2: {player2TotalScore}分");
        }

        /// <summary>
        /// 获取历史记录
        /// </summary>
        public List<RoundScore> GetHistory()
        {
            return new List<RoundScore>(roundHistory);
        }

        /// <summary>
        /// 重置分数
        /// </summary>
        public void Reset()
        {
            roundHistory.Clear();
            player1TotalScore = 0;
            player2TotalScore = 0;
        }
    }

    /// <summary>
    /// 特殊分数规则（可选的高级计分）
    /// </summary>
    public class SpecialScoring
    {
        /// <summary>
        /// Carte Blanche（白牌）：手中无人像牌（J、Q、K）得10分
        /// </summary>
        public static int CarteBlanche(PlayerHand hand)
        {
            return hand.HasFaceCards() ? 0 : 10;
        }

        /// <summary>
        /// Pique（皮克）：对手在声明和出牌前得分为0，额外得30分
        /// </summary>
        public static int Pique(int opponentScore)
        {
            return opponentScore == 0 ? 30 : 0;
        }

        /// <summary>
        /// Repique（重皮克）：在出牌前，自己得分达到30而对手为0，得60分
        /// </summary>
        public static int Repique(int myDeclarationScore, int opponentDeclarationScore)
        {
            if (myDeclarationScore >= 30 && opponentDeclarationScore == 0)
                return 60;
            return 0;
        }
    }
}

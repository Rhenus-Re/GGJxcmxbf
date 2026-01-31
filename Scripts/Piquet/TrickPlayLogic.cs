using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 出牌和赢墩逻辑扩展
    /// </summary>
    public partial class PiquetGameManager
    {
        /// <summary>
        /// 玩家出牌
        /// </summary>
        public bool PlayCard(PlayerHand player, Card card)
        {
            if (CurrentPhase != GamePhase.Playing)
            {
                GD.PrintErr("当前不是出牌阶段");
                return false;
            }

            if (player != CurrentPlayer)
            {
                GD.PrintErr($"现在不是{player.PlayerName}的回合");
                return false;
            }

            if (!player.Cards.Contains(card))
            {
                GD.PrintErr("手中没有这张牌");
                return false;
            }

            // 如果是第二张牌，需要验证是否符合跟牌规则
            if (currentTrick.Count == 1)
            {
                Card leadCard = currentTrick[0];
                if (!IsValidFollow(player, card, leadCard))
                {
                    GD.PrintErr("必须跟出相同花色的牌");
                    return false;
                }
            }

            // 出牌
            player.RemoveCard(card);
            currentTrick.Add(card);
            
            GD.Print($"{player.PlayerName} 出牌: {card}");

            // 如果是第一张牌，切换到对手
            if (currentTrick.Count == 1)
            {
                CurrentPlayer = GetOpponent(player);
            }
            // 如果是第二张牌，判断谁赢得牌墩
            else if (currentTrick.Count == 2)
            {
                EvaluateTrick();
            }

            return true;
        }

        /// <summary>
        /// 验证跟牌是否合法
        /// </summary>
        private bool IsValidFollow(PlayerHand player, Card followCard, Card leadCard)
        {
            // 允许出任何牌 - 玩家可以选择不跟同花色来故意输掉这一墩
            // 如果出的是不同花色，会自动输掉这一墩
            return true;
        }

        /// <summary>
        /// 判断牌墩赢家
        /// </summary>
        private void EvaluateTrick()
        {
            Card card1 = currentTrick[0];
            Card card2 = currentTrick[1];
            
            // 第一张牌的出牌者
            PlayerHand firstPlayer = GetOpponent(CurrentPlayer);
            
            // 判断谁赢：同花色比大小，不同花色首出者必赢（跟牌者出其他花色必输）
            bool firstWins;
            if (card1.Suit == card2.Suit)
            {
                // 相同花色，比较点数大小
                firstWins = card1.GetValue() > card2.GetValue();
            }
            else
            {
                // 不同花色，跟牌者无法赢墩（领牌者必赢）
                firstWins = true;
            }

            trickWinner = firstWins ? firstPlayer : CurrentPlayer;
            
            if (trickWinner == Player1)
                player1Tricks++;
            else
                player2Tricks++;

            // 计算这墩得分（每墩1分）
            int trickScore = 1;
            int totalTricks = trickWinner == Player1 ? player1Tricks : player2Tricks;

            GD.Print($">>> {trickWinner.PlayerName} 赢得此墩！(当前已赢{totalTricks}墩)");
            
            EmitSignal(SignalName.TrickWon, trickWinner.PlayerName, trickScore, totalTricks);

            // 清空当前牌墩
            currentTrick.Clear();

            // 赢家先出下一张牌
            CurrentPlayer = trickWinner;

            // 检查是否所有牌都出完了
            if (Player1.CardCount() == 0 && Player2.CardCount() == 0)
            {
                EndRound();
            }
        }

        /// <summary>
        /// 结束当前回合并计分
        /// </summary>
        private void EndRound()
        {
            GD.Print("\n=== 回合结束，计算赢墩分数 ===");
            
            CurrentPhase = GamePhase.Scoring;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            // 计算赢墩分数
            int player1TrickScore = CalculateTrickScore(player1Tricks, player2Tricks);
            int player2TrickScore = CalculateTrickScore(player2Tricks, player1Tricks);
            
            Player1.Score += player1TrickScore;
            Player2.Score += player2TrickScore;

            GD.Print($"{Player1.PlayerName}: 赢得{player1Tricks}墩，获得{player1TrickScore}分");
            GD.Print($"{Player2.PlayerName}: 赢得{player2Tricks}墩，获得{player2TrickScore}分");
            GD.Print($"\n本局总分 - {Player1.PlayerName}: {Player1.Score}分, {Player2.PlayerName}: {Player2.Score}分");

            EmitSignal(SignalName.RoundEnded);

            // 检查游戏是否结束
            CurrentRound++;
            if (CurrentRound > TotalRounds || Player1.Score >= TargetScore || Player2.Score >= TargetScore)
            {
                EndGame();
            }
            else
            {
                // 交换发牌权
                Player1Role = Player1Role == PlayerRole.Dealer ? PlayerRole.NonDealer : PlayerRole.Dealer;
                StartNewRound();
            }
        }

        /// <summary>
        /// 计算赢墩分数
        /// </summary>
        private int CalculateTrickScore(int myTricks, int opponentTricks)
        {
            int score = 0;

            // 每赢一墩得1分
            score += myTricks;

            // 如果赢得7墩以上（超过半数），额外得10分
            if (myTricks >= 7)
            {
                score += 10;
            }

            // 如果赢得全部12墩（Capot），额外得40分
            if (myTricks == 12)
            {
                score += 40;
            }

            return score;
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        private void EndGame()
        {
            CurrentPhase = GamePhase.GameOver;
            EmitSignal(SignalName.PhaseChanged, (int)CurrentPhase);

            string winner;
            if (Player1.Score > Player2.Score)
                winner = Player1.PlayerName;
            else if (Player2.Score > Player1.Score)
                winner = Player2.PlayerName;
            else
                winner = "平局";

            GD.Print("\n╔════════════════════════════════╗");
            GD.Print("║       游戏结束！Game Over!      ║");
            GD.Print("╚════════════════════════════════╝");
            GD.Print($"最终得分:");
            GD.Print($"  {Player1.PlayerName}: {Player1.Score}分");
            GD.Print($"  {Player2.PlayerName}: {Player2.Score}分");
            GD.Print($"获胜者: {winner}");

            EmitSignal(SignalName.GameOver, winner);
        }

        /// <summary>
        /// 获取对手
        /// </summary>
        private PlayerHand GetOpponent(PlayerHand player)
        {
            return player == Player1 ? Player2 : Player1;
        }

        /// <summary>
        /// 获取当前牌墩信息
        /// </summary>
        public List<Card> GetCurrentTrick()
        {
            return new List<Card>(currentTrick);
        }

        /// <summary>
        /// 获取赢墩统计
        /// </summary>
        public (int player1Tricks, int player2Tricks) GetTrickCounts()
        {
            return (player1Tricks, player2Tricks);
        }
    }
}

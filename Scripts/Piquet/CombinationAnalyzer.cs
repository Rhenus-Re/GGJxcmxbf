using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 组合类型
    /// </summary>
    public enum CombinationType
    {
        Point,      // 牌点（同花色总数）
        Sequence,   // 顺子（连牌）
        Set         // 长套（相同点数）
    }

    /// <summary>
    /// 组合信息
    /// </summary>
    public class Combination
    {
        public CombinationType Type { get; set; }
        public List<Card> Cards { get; set; }
        public int Value { get; set; }
        public int Score { get; set; }

        public Combination(CombinationType type, List<Card> cards)
        {
            Type = type;
            Cards = cards;
            CalculateValue();
            CalculateScore();
        }

        private void CalculateValue()
        {
            switch (Type)
            {
                case CombinationType.Point:
                    // 牌点：同花色牌张数量
                    Value = Cards.Count;
                    break;
                case CombinationType.Sequence:
                    // 顺子：最高牌的值
                    Value = Cards.Max(c => c.GetValue());
                    break;
                case CombinationType.Set:
                    // 长套：牌面值
                    Value = Cards[0].GetValue();
                    break;
            }
        }

        private void CalculateScore()
        {
            switch (Type)
            {
                case CombinationType.Point:
                    // 牌点：每张牌1分
                    Score = Cards.Count;
                    break;
                case CombinationType.Sequence:
                    // 顺子：3张=3分，4张=4分，5张=15分，6张=16分，7张=17分，8张=18分
                    Score = Cards.Count;
                    if (Cards.Count >= 5)
                        Score += 10;
                    break;
                case CombinationType.Set:
                    // 长套：3张=3分，4张=14分
                    Score = Cards.Count == 4 ? 14 : 3;
                    break;
            }
        }

        public override string ToString()
        {
            string typeName = Type switch
            {
                CombinationType.Point => "牌点",
                CombinationType.Sequence => "顺子",
                CombinationType.Set => "长套",
                _ => ""
            };
            return $"{typeName}: [{string.Join(", ", Cards)}] (价值:{Value}, 分数:{Score})";
        }
    }

    /// <summary>
    /// 组合判断器 - 分析手牌中的各种组合
    /// </summary>
    public class CombinationAnalyzer
    {
        /// <summary>
        /// 获取最佳牌点组合（同一花色最多的牌）
        /// </summary>
        public static Combination GetBestPoint(PlayerHand hand)
        {
            var suitGroups = hand.GetCardsBySuit();
            var bestSuit = suitGroups.OrderByDescending(g => g.Value.Count).First();
            
            if (bestSuit.Value.Count == 0)
                return null;

            return new Combination(CombinationType.Point, bestSuit.Value);
        }

        /// <summary>
        /// 获取所有顺子组合（同花色连续牌，至少3张）
        /// </summary>
        public static List<Combination> GetAllSequences(PlayerHand hand)
        {
            List<Combination> sequences = new List<Combination>();
            var suitGroups = hand.GetCardsBySuit();

            foreach (var suitGroup in suitGroups.Values)
            {
                if (suitGroup.Count < 3)
                    continue;

                // 按点数排序
                var sortedCards = suitGroup.OrderBy(c => c.Rank).ToList();
                
                // 查找连续序列
                List<Card> currentSequence = new List<Card> { sortedCards[0] };
                
                for (int i = 1; i < sortedCards.Count; i++)
                {
                    if ((int)sortedCards[i].Rank == (int)sortedCards[i-1].Rank + 1)
                    {
                        currentSequence.Add(sortedCards[i]);
                    }
                    else
                    {
                        if (currentSequence.Count >= 3)
                        {
                            sequences.Add(new Combination(CombinationType.Sequence, new List<Card>(currentSequence)));
                        }
                        currentSequence.Clear();
                        currentSequence.Add(sortedCards[i]);
                    }
                }
                
                // 检查最后一个序列
                if (currentSequence.Count >= 3)
                {
                    sequences.Add(new Combination(CombinationType.Sequence, currentSequence));
                }
            }

            return sequences;
        }

        /// <summary>
        /// 获取最佳顺子
        /// </summary>
        public static Combination GetBestSequence(PlayerHand hand)
        {
            var sequences = GetAllSequences(hand);
            if (sequences.Count == 0)
                return null;

            // 先按长度排序，再按最高牌排序
            return sequences.OrderByDescending(s => s.Cards.Count)
                           .ThenByDescending(s => s.Value)
                           .First();
        }

        /// <summary>
        /// 获取所有长套（3张或4张相同点数）
        /// </summary>
        public static List<Combination> GetAllSets(PlayerHand hand)
        {
            List<Combination> sets = new List<Combination>();
            var rankGroups = hand.GetCardsByRank();

            foreach (var group in rankGroups.Values)
            {
                if (group.Count >= 3)
                {
                    sets.Add(new Combination(CombinationType.Set, group));
                }
            }

            return sets;
        }

        /// <summary>
        /// 获取最佳长套
        /// </summary>
        public static Combination GetBestSet(PlayerHand hand)
        {
            var sets = GetAllSets(hand);
            if (sets.Count == 0)
                return null;

            // 先按数量排序（4张>3张），再按牌面值排序
            return sets.OrderByDescending(s => s.Cards.Count)
                      .ThenByDescending(s => s.Value)
                      .First();
        }

        /// <summary>
        /// 比较两个组合（返回1表示combo1更好，-1表示combo2更好，0表示相等）
        /// </summary>
        public static int CompareCombinations(Combination combo1, Combination combo2)
        {
            if (combo1 == null && combo2 == null) return 0;
            if (combo1 == null) return -1;
            if (combo2 == null) return 1;

            // 先比较数量/长度
            if (combo1.Cards.Count != combo2.Cards.Count)
                return combo1.Cards.Count.CompareTo(combo2.Cards.Count);

            // 数量相同，比较价值
            return combo1.Value.CompareTo(combo2.Value);
        }

        /// <summary>
        /// 获取玩家的所有最佳组合
        /// </summary>
        public static Dictionary<CombinationType, Combination> GetBestCombinations(PlayerHand hand)
        {
            var combinations = new Dictionary<CombinationType, Combination>();
            
            var bestPoint = GetBestPoint(hand);
            if (bestPoint != null)
                combinations[CombinationType.Point] = bestPoint;

            var bestSequence = GetBestSequence(hand);
            if (bestSequence != null)
                combinations[CombinationType.Sequence] = bestSequence;

            var bestSet = GetBestSet(hand);
            if (bestSet != null)
                combinations[CombinationType.Set] = bestSet;

            return combinations;
        }
    }
}

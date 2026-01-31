using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 玩家手牌管理类
    /// </summary>
    public class PlayerHand
    {
        public string PlayerName { get; private set; }
        public List<Card> Cards { get; private set; }
        public int Score { get; set; }

        public PlayerHand(string name)
        {
            PlayerName = name;
            Cards = new List<Card>();
            Score = 0;
        }

        /// <summary>
        /// 添加卡牌到手牌
        /// </summary>
        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        /// <summary>
        /// 添加多张卡牌
        /// </summary>
        public void AddCards(List<Card> cards)
        {
            Cards.AddRange(cards);
        }

        /// <summary>
        /// 移除卡牌
        /// </summary>
        public bool RemoveCard(Card card)
        {
            return Cards.Remove(card);
        }

        /// <summary>
        /// 清空手牌
        /// </summary>
        public void Clear()
        {
            Cards.Clear();
        }

        /// <summary>
        /// 排序手牌（按花色和点数）
        /// </summary>
        public void SortCards()
        {
            Cards.Sort();
        }

        /// <summary>
        /// 检查是否有人像牌（J、Q、K）
        /// </summary>
        public bool HasFaceCards()
        {
            return Cards.Any(c => c.Rank == Rank.Jack || c.Rank == Rank.Queen || c.Rank == Rank.King);
        }

        /// <summary>
        /// 获取手牌数量
        /// </summary>
        public int CardCount()
        {
            return Cards.Count;
        }

        /// <summary>
        /// 换牌：移除旧牌，添加新牌
        /// </summary>
        public void ExchangeCards(List<Card> cardsToRemove, List<Card> newCards)
        {
            foreach (var card in cardsToRemove)
            {
                Cards.Remove(card);
            }
            AddCards(newCards);
            SortCards();
        }

        /// <summary>
        /// 获取所有花色的牌
        /// </summary>
        public Dictionary<Suit, List<Card>> GetCardsBySuit()
        {
            var suitGroups = new Dictionary<Suit, List<Card>>();
            
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                suitGroups[suit] = Cards.Where(c => c.Suit == suit).OrderByDescending(c => c.Rank).ToList();
            }
            
            return suitGroups;
        }

        /// <summary>
        /// 获取相同点数的牌组
        /// </summary>
        public Dictionary<Rank, List<Card>> GetCardsByRank()
        {
            return Cards.GroupBy(c => c.Rank)
                       .ToDictionary(g => g.Key, g => g.ToList());
        }

        public override string ToString()
        {
            SortCards();
            return $"{PlayerName}: [{string.Join(", ", Cards)}] (分数: {Score})";
        }
    }
}

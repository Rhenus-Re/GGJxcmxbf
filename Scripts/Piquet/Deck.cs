using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiquetGame
{
    /// <summary>
    /// 牌堆类 - 管理32张皮克牌
    /// </summary>
    public class Deck
    {
        private List<Card> cards;
        private Random random;

        public Deck()
        {
            random = new Random();
            InitializeDeck();
        }

        /// <summary>
        /// 初始化牌堆（32张牌：7-A，四种花色）
        /// </summary>
        private void InitializeDeck()
        {
            cards = new List<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    cards.Add(new Card(suit, rank));
                }
            }
        }

        /// <summary>
        /// 洗牌（Fisher-Yates算法）
        /// </summary>
        public void Shuffle()
        {
            int n = cards.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Card temp = cards[i];
                cards[i] = cards[j];
                cards[j] = temp;
            }
        }

        /// <summary>
        /// 发牌
        /// </summary>
        public Card DrawCard()
        {
            if (cards.Count == 0)
            {
                GD.PrintErr("牌堆已空，无法发牌");
                return null;
            }

            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        /// <summary>
        /// 发多张牌
        /// </summary>
        public List<Card> DrawCards(int count)
        {
            List<Card> drawnCards = new List<Card>();
            for (int i = 0; i < count && cards.Count > 0; i++)
            {
                drawnCards.Add(DrawCard());
            }
            return drawnCards;
        }

        /// <summary>
        /// 获取剩余牌数
        /// </summary>
        public int RemainingCards()
        {
            return cards.Count;
        }

        /// <summary>
        /// 重置牌堆
        /// </summary>
        public void Reset()
        {
            InitializeDeck();
        }

        /// <summary>
        /// 将牌放回牌堆底部
        /// </summary>
        public void ReturnCard(Card card)
        {
            cards.Add(card);
        }
    }
}

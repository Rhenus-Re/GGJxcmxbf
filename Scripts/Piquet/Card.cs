using Godot;
using System;

namespace PiquetGame
{
    /// <summary>
    /// 花色枚举（皮克牌使用标准四色）
    /// </summary>
    public enum Suit
    {
        Hearts,   // 红桃 (f)
        Diamonds, // 方块 (x)
        Clubs,    // 梅花 (m)
        Spades    // 黑桃 (t)
    }

    /// <summary>
    /// 牌面值枚举（7-A，共8种）
    /// </summary>
    public enum Rank
    {
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14  // A最大
    }

    /// <summary>
    /// 卡牌类 - 表示一张扑克牌
    /// </summary>
    public class Card : IComparable<Card>
    {
        public Suit Suit { get; private set; }
        public Rank Rank { get; private set; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        /// <summary>
        /// 获取牌面点数值
        /// </summary>
        public int GetValue()
        {
            return (int)Rank;
        }

        /// <summary>
        /// 获取卡牌的资源文件名
        /// </summary>
        public string GetImageFileName()
        {
            string suitPrefix = Suit switch
            {
                Suit.Hearts => "x",
                Suit.Diamonds => "f",
                Suit.Clubs => "m",
                Suit.Spades => "t",
                _ => "x"
            };

            int rankNumber = Rank switch
            {
                Rank.Seven => 7,
                Rank.Eight => 8,
                Rank.Nine => 9,
                Rank.Ten => 10,
                Rank.Jack => 11,
                Rank.Queen => 12,
                Rank.King => 13,
                Rank.Ace => 1,  // A在资源文件中通常是1
                _ => 1
            };

            return $"{suitPrefix}{rankNumber}@3x.png";
        }

        /// <summary>
        /// 比较两张牌的大小（用于排序）
        /// </summary>
        public int CompareTo(Card other)
        {
            if (other == null) return 1;
            
            // 先按花色排序，再按点数排序
            int suitComparison = Suit.CompareTo(other.Suit);
            if (suitComparison != 0)
                return suitComparison;
            
            return Rank.CompareTo(other.Rank);
        }

        public override string ToString()
        {
            string suitSymbol = Suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => ""
            };

            string rankString = Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)Rank).ToString()
            };

            return $"{rankString}{suitSymbol}";
        }
    }
}

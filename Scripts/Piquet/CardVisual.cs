using Godot;
using System;

namespace PiquetGame
{
    /// <summary>
    /// 卡牌视觉节点 - 在Godot场景中显示卡牌
    /// </summary>
    public partial class CardVisual : TextureRect
    {
        private Card card;
        private bool isFaceUp = true;
        
        [Export]
        public string CardResourcePath { get; set; } = "res://Src/cards/SVG/";
        
        [Export]
        public string BackCardPath { get; set; } = "res://Src/cards/SVG/back (2).svg";

        /// <summary>
        /// 设置要显示的卡牌
        /// </summary>
        public void SetCard(Card newCard, bool faceUp = true)
        {
            card = newCard;
            isFaceUp = faceUp;
            UpdateTexture();
        }

        /// <summary>
        /// 翻转卡牌
        /// </summary>
        public void Flip()
        {
            isFaceUp = !isFaceUp;
            UpdateTexture();
        }

        /// <summary>
        /// 设置是否正面朝上
        /// </summary>
        public void SetFaceUp(bool faceUp)
        {
            isFaceUp = faceUp;
            UpdateTexture();
        }

        /// <summary>
        /// 更新纹理
        /// </summary>
        private void UpdateTexture()
        {
            if (card == null)
            {
                Texture = null;
                return;
            }

            string texturePath;
            
            if (isFaceUp)
            {
                texturePath = CardResourcePath + card.GetImageFileName();
            }
            else
            {
                texturePath = BackCardPath;
            }

            // 加载纹理
            var texture = GD.Load<Texture2D>(texturePath);
            if (texture != null)
            {
                Texture = texture;
            }
            else
            {
                GD.PrintErr($"无法加载卡牌纹理: {texturePath}");
            }
        }

        /// <summary>
        /// 获取当前卡牌
        /// </summary>
        public Card GetCard()
        {
            return card;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Stop;
            FocusMode = FocusModeEnum.All;
            ConnectMouseSignals();
        }

        /// <summary>
        /// 鼠标进入效果（Godot 4使用信号方式）
        /// </summary>
        public void OnMouseEntered()
        {
            Scale = new Vector2(1.1f, 1.1f);
        }

        /// <summary>
        /// 鼠标离开效果（Godot 4使用信号方式）
        /// </summary>
        public void OnMouseExited()
        {
            Scale = Vector2.One;
        }
        
        /// <summary>
        /// 连接鼠标信号
        /// </summary>
        private void ConnectMouseSignals()
        {
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }
    }

    /// <summary>
    /// 手牌容器 - 显示一组卡牌
    /// </summary>
    public partial class HandContainer : HBoxContainer
    {
        private PackedScene cardVisualScene;
        
        public override void _Ready()
        {
            // 可以从场景加载CardVisual，或者动态创建
        }

        /// <summary>
        /// 显示手牌
        /// </summary>
        public void DisplayHand(PlayerHand hand, bool faceUp = true)
        {
            // 清空现有卡牌
            foreach (var child in GetChildren())
            {
                child.QueueFree();
            }

            // 为每张牌创建视觉节点
            foreach (var card in hand.Cards)
            {
                var cardVisual = new CardVisual();
                AddChild(cardVisual);
                cardVisual.SetCard(card, faceUp);
                
                // 设置卡牌大小
                cardVisual.CustomMinimumSize = new Vector2(80, 120);
                cardVisual.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                cardVisual.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            }
        }

        /// <summary>
        /// 更新手牌显示
        /// </summary>
        public void UpdateHand(PlayerHand hand)
        {
            DisplayHand(hand, true);
        }
    }
}

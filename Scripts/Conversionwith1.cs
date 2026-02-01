using Godot;
using System;
using System.Collections.Generic;

public partial class Conversionwith1 : Node2D
{
	private NinePatchRect _playerWords;
	private NinePatchRect _comWords;
	private Label _playerLabel;
	private Label _comLabel;
	
	// 对话队列
	private Queue<DialogueLine> _dialogueQueue = new Queue<DialogueLine>();
	private bool _isShowingDialogue = false;
	private float _dialogueTimer = 0f;
	private float _dialogueDisplayTime = 3.0f; // 每条对话显示时间
	
	// 对话数据结构
	public class DialogueLine
	{
		public bool IsPlayer { get; set; }
		public string Text { get; set; }
		
		public DialogueLine(bool isPlayer, string text)
		{
			IsPlayer = isPlayer;
			Text = text;
		}
	}

	public override void _Ready()
	{
		// 获取节点引用
		_playerWords = GetNode<NinePatchRect>("Playerwords");
		_comWords = GetNode<NinePatchRect>("Comwords");
		_playerLabel = _playerWords.GetNode<Label>("Label");
		_comLabel = _comWords.GetNode<Label>("Label");
		
		// 初始隐藏气泡
		_playerWords.Visible = false;
		_comWords.Visible = false;
		
		// 设置 Label 自动换行和大小
		SetupLabel(_playerLabel);
		SetupLabel(_comLabel);
		
		// 加载示例对话
		LoadSampleDialogue();
	}
	
	private void SetupLabel(Label label)
	{
		label.AutowrapMode = TextServer.AutowrapMode.Word;
		label.HorizontalAlignment = HorizontalAlignment.Left;
		label.VerticalAlignment = VerticalAlignment.Center;
		// 设置 Label 的锚点和位置
		label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		label.OffsetLeft = 30;   // 左侧边距（增大此值可增加与左边的距离）
		label.OffsetTop = 0;    // 上边距
		label.OffsetRight = -20; // 右侧边距
		label.OffsetBottom = -20;// 下边距
	}
	
	private void LoadSampleDialogue()
	{
		// 添加示例对话
		_dialogueQueue.Enqueue(new DialogueLine(true, 
			"韦德先生，我是詹姆斯·柯林斯，《雾港纪事报》。首先请允许我表达哀悼。我知道您和伦诺克斯先生是二十年的朋友，如果现在不合适..."));
		
		_dialogueQueue.Enqueue(new DialogueLine(false, 
			"坐吧，柯林斯。我读过你关于码头工人罢工的报道，还算公正。你想知道什么？"));
		
		// 开始显示对话
		ShowNextDialogue();
	}
	
	private void ShowNextDialogue()
	{
		if (_dialogueQueue.Count == 0)
		{
			_isShowingDialogue = false;
			_playerWords.Visible = false;
			_comWords.Visible = false;
			return;
		}
		
		var dialogue = _dialogueQueue.Dequeue();
		_isShowingDialogue = true;
		_dialogueTimer = 0f;
		
		if (dialogue.IsPlayer)
		{
			ShowPlayerDialogue(dialogue.Text);
		}
		else
		{
			ShowComDialogue(dialogue.Text);
		}
	}
	
	private void ShowPlayerDialogue(string text)
	{
		_comWords.Visible = false;
		_playerWords.Visible = true;
		_playerLabel.Text = text;
		
		// 等待一帧让 Label 计算好大小后调整气泡
		CallDeferred(nameof(AdjustBubbleSize), _playerWords, _playerLabel);
	}
	
	private void ShowComDialogue(string text)
	{
		_playerWords.Visible = false;
		_comWords.Visible = true;
		_comLabel.Text = text;
		
		// 等待一帧让 Label 计算好大小后调整气泡
		CallDeferred(nameof(AdjustBubbleSize), _comWords, _comLabel);
	}
	
	private void AdjustBubbleSize(NinePatchRect bubble, Label label)
	{
		// 设置最大宽度和内边距
		float maxWidth = 500f;  // 增大最大宽度
		float paddingX = 80f;   // 水平内边距
		float paddingY = 50f;   // 垂直内边距
		
		// 获取文本所需的大小
		var font = label.GetThemeFont("font");
		var fontSize = label.GetThemeFontSize("font_size");
		var textSize = font.GetMultilineStringSize(label.Text, HorizontalAlignment.Left, maxWidth - paddingX, fontSize);
		
		// 计算气泡大小（加上内边距）
		float bubbleWidth = Mathf.Max(textSize.X + paddingX, 150f);
		float bubbleHeight = Mathf.Max(textSize.Y + paddingY, 80f);
		
		// 判断是否是电脑气泡（从右下角扩展）
		if (bubble == _comWords)
		{
			// 保存右下角的位置
			float rightX = bubble.Position.X + bubble.Size.X;
			float bottomY = bubble.Position.Y + bubble.Size.Y;
			
			// 设置新大小
			bubble.Size = new Vector2(bubbleWidth, bubbleHeight);
			
			// 重新定位，使右下角保持不变
			bubble.Position = new Vector2(rightX - bubbleWidth, bottomY - bubbleHeight);
		}
		else
		{
			// 玩家气泡：从左上角扩展（默认行为）
			bubble.Size = new Vector2(bubbleWidth, bubbleHeight);
		}
	}

	public override void _Process(double delta)
	{
		if (_isShowingDialogue)
		{
			_dialogueTimer += (float)delta;
			
			// 按空格或点击可以跳过当前对话
			if (Input.IsActionJustPressed("ui_accept") || Input.IsMouseButtonPressed(MouseButton.Left))
			{
				if (_dialogueTimer > 0.5f) // 防止误触
				{
					ShowNextDialogue();
				}
			}
			
			// 自动切换到下一条对话
			if (_dialogueTimer >= _dialogueDisplayTime)
			{
				ShowNextDialogue();
			}
		}
	}
	
	// 公共方法：添加新对话
	public void AddDialogue(bool isPlayer, string text)
	{
		_dialogueQueue.Enqueue(new DialogueLine(isPlayer, text));
		
		if (!_isShowingDialogue)
		{
			ShowNextDialogue();
		}
	}
}

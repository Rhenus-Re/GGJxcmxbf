using Godot;
using System;

public partial class Gamestart : Node2D
{
	private Button startButton;
	private Sprite2D middleSprite;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 获取start按钮和middle图片
		startButton = GetNode<Button>("start");
		middleSprite = GetNode<Sprite2D>("Middle");
		
		// 确保middle开始时不可见且完全透明
		middleSprite.Visible = false;
		middleSprite.Modulate = new Color(1, 1, 1, 0);
		
		// 连接按钮点击事件
		startButton.Pressed += OnStartPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnStartPressed()
	{
		// 显示middle图片
		middleSprite.Visible = true;
		
		// 创建淡入动画
		Tween tween = CreateTween();
		
		// 在1.5秒内从透明渐变到不透明
		tween.TweenProperty(middleSprite, "modulate:a", 1.0f, 1.5f);
		
		// 等待2秒
		tween.TweenInterval(2.0f);
		
		// 动画完成后切换场景
		tween.TweenCallback(Callable.From(() => {
			GetTree().ChangeSceneToFile("res://Scenes/seconpage.tscn");
		}));
	}
}

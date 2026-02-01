using Godot;
using System;

public partial class Seconpage : Node2D
{
	private Texture2D cursorTexture;
	private TextureButton fButton;
	private TextureButton wButton;
	private TextureButton lButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 加载椭圆1图片作为光标
		cursorTexture = GD.Load<Texture2D>("res://Src/第二页/椭圆 1.png");
		
		// 获取三个按钮
		fButton = GetNode<TextureButton>("F");
		wButton = GetNode<TextureButton>("W");
		lButton = GetNode<TextureButton>("L");
		
		// 连接鼠标进入和离开事件
		fButton.MouseEntered += OnButtonMouseEntered;
		fButton.MouseExited += OnButtonMouseExited;
		
		wButton.MouseEntered += OnButtonMouseEntered;
		wButton.MouseExited += OnButtonMouseExited;
		
		lButton.MouseEntered += OnButtonMouseEntered;
		lButton.MouseExited += OnButtonMouseExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnButtonMouseEntered()
	{
		// 鼠标进入按钮时，设置自定义光标
		if (cursorTexture != null)
		{
			Vector2 hotspot = cursorTexture.GetSize() / 2;
			Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Arrow, hotspot);
		}
	}
	
	private void OnButtonMouseExited()
	{
		// 鼠标离开按钮时，恢复默认光标
		Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);
	}
	
	// 离开场景时恢复默认光标
	public override void _ExitTree()
	{
		Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);
	}
}

using Godot;
using System;

public partial class Details : Button
{
	private PackedScene detailsScene;
	private Node2D detailsInstance;
	private ColorRect overlay;
	private CanvasLayer canvasLayer;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 加载Details场景
		detailsScene = GD.Load<PackedScene>("res://Scenes/Details.tscn");
		
		// 连接按钮点击信号
		Pressed += OnDetailsPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// 检测ESC键按下
		if (detailsInstance != null && Input.IsActionJustPressed("ui_cancel"))
		{
			HideDetails();
		}
	}
	
	private void OnDetailsPressed()
	{
		if (detailsInstance == null && detailsScene != null)
		{
			// 创建CanvasLayer确保在最上层
			canvasLayer = new CanvasLayer();
			canvasLayer.Layer = 100; // 设置高层级确保在最上面
			GetTree().Root.AddChild(canvasLayer);
			
			// 创建半透明遮罩层阻止背景点击
			overlay = new ColorRect();
			overlay.Color = new Color(0, 0, 0, 0.5f); // 半透明黑色
			overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			overlay.MouseFilter = Control.MouseFilterEnum.Stop; // 阻止鼠标事件穿透
			canvasLayer.AddChild(overlay);
			
			// 实例化Details场景
			detailsInstance = detailsScene.Instantiate<Node2D>();
			
			// 设置到屏幕中心
			Vector2 screenSize = GetViewport().GetVisibleRect().Size;
			detailsInstance.Position = screenSize / 2;
			
			canvasLayer.AddChild(detailsInstance);
		}
	}
	
	private void HideDetails()
	{
		if (detailsInstance != null)
		{
			detailsInstance.QueueFree();
			detailsInstance = null;
		}
		
		if (overlay != null)
		{
			overlay.QueueFree();
			overlay = null;
		}
		
		if (canvasLayer != null)
		{
			canvasLayer.QueueFree();
			canvasLayer = null;
		}
	}
}

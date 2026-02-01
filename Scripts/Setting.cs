using Godot;
using System;

public partial class Setting : Button
{
	private Node2D settingPanel;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// 获取setting场景节点
		settingPanel = GetNode<Node2D>("Node2D");
		
		// 连接按钮点击信号
		Pressed += OnSettingPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// 检测ESC键按下
		if (settingPanel != null && settingPanel.Visible && Input.IsActionJustPressed("ui_cancel"))
		{
			settingPanel.Visible = false;
		}
	}
	
	private void OnSettingPressed()
	{
		// 点击按钮时显示setting场景
		if (settingPanel != null)
		{
			settingPanel.Visible = true;
		}
	}
}

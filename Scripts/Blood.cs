using Godot;
using System;

public partial class Blood : Node2D
{
	[Export] public float MaxHealth = 100f;
	[Export] public float CurrentHealth = 100f;
	
	private Sprite2D _healthBar;
	private Sprite2D _background;
	private Vector2 _originalTextureSize;
	private Vector2 _originalPosition;
	
	public override void _Ready()
	{
		// 获取子节点
		_background = GetNode<Sprite2D>("底色");
		_healthBar = GetNode<Sprite2D>("主角血条");
		
		// 获取原始纹理大小
		if (_healthBar.Texture != null)
		{
			_originalTextureSize = _healthBar.Texture.GetSize();
			_originalPosition = _healthBar.Position;
			GD.Print($"Texture size: {_originalTextureSize}, Original position: {_originalPosition}");
		}
		
		// 禁用居中，这样sprite从左上角开始绘制，便于左侧对齐
		_healthBar.Centered = false;
		_background.Centered = false;
		
		// 启用region
		_healthBar.RegionEnabled = true;
		_background.RegionEnabled = true;
		
		// 设置底色为白色
		_background.Modulate = Colors.White;
		
		// 调整位置（因为禁用了centered，需要补偿）
		_background.Position = new Vector2(_originalPosition.X - _originalTextureSize.X / 2, _originalPosition.Y - _originalTextureSize.Y / 2);
		_healthBar.Position = _background.Position;
		_originalPosition = _healthBar.Position;
		
		// 初始化region为完整纹理
		UpdateHealthDisplay();
	}
	
	// 更新血量显示
	public void UpdateHealthDisplay()
	{
		if (_healthBar == null || _healthBar.Texture == null) return;
		
		// 计算血量百分比
		float healthPercent = Mathf.Clamp(CurrentHealth / MaxHealth, 0f, 1f);
		
		// 计算region宽度
		float regionWidth = _originalTextureSize.X * healthPercent;
		
		GD.Print($"Health: {CurrentHealth}/{MaxHealth} ({healthPercent * 100}%), Region width: {regionWidth}");
		
		// 设置背景的region（完整显示）
		_background.RegionRect = new Rect2(0, 0, _originalTextureSize.X, _originalTextureSize.Y);
		
		// 设置血条的region（从左到右裁剪）
		_healthBar.RegionRect = new Rect2(0, 0, regionWidth, _originalTextureSize.Y);
		
		// 因为禁用了centered，sprite从左上角开始绘制，左侧自动对齐，无需调整位置
		_healthBar.Position = _originalPosition;
		
		GD.Print($"Health bar region: {_healthBar.RegionRect}, Position: {_healthBar.Position}");
	}
	
	// 设置血量
	public void SetHealth(float health)
	{
		CurrentHealth = Mathf.Clamp(health, 0f, MaxHealth);
		UpdateHealthDisplay();
	}
	
	// 减少血量
	public void TakeDamage(float damage)
	{
		SetHealth(CurrentHealth - damage);
	}
	
	// 恢复血量
	public void Heal(float amount)
	{
		SetHealth(CurrentHealth + amount);
	}
	
	public override void _Process(double delta)
	{
		// 测试：按Z键减少血量
		if (Input.IsKeyPressed(Key.Z))
		{
			GD.Print("Damage taken: 10");
			TakeDamage(10f);
		}
		
		// 测试：按R键恢复满血
		if (Input.IsKeyPressed(Key.R))
		{
			SetHealth(MaxHealth);
		}
	}
}

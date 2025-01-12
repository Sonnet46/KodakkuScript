using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Game.ClientState.Objects.Types;
using Newtonsoft.Json;
using Dalamud.Utility.Numerics;
using ECommons;
using ECommons.DalamudServices;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.Draw;
namespace KodakkuScript.Script._07_DawnTrail;

[ScriptType(guid: "9F1D4DC0-1891-7AC0-9571-D5266198751D", name: "魂魄工厂创生设施", territorys: [1208], version: "0.0.0.1",
    author: "Poetry")]
public class Origenics
{
    private int _aoeCount = 0;
    private readonly object _lockObject = new object(); 
    private uint _thunderId = 0;
    
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        InitData();
    }

    private void InitData()
    {
        _aoeCount = 0;
        _thunderId = 0;
    }
    
    private static bool ParseObjectId(string? idStr, out uint id)
    {
        id = 0;
        if (string.IsNullOrEmpty(idStr)) return false;
        try
        {
            var idStr2 = idStr.Replace("0x", "");
            id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private Vector3 DeserializeVector3(string propertyName)
    {
        return JsonConvert.DeserializeObject<Vector3>(propertyName);
    }
    
    private int DeserializeInt(string propertyName)
    {
        return JsonConvert.DeserializeObject<int>(propertyName);
    }
    
    // Boss 1
    
    // Boss1-刺耳尖叫
    // Boss2-雷转质波动
    // Boss3-念动波
    
    [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(36519|36371|36436)$"])]
    public void Aoe(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("AOE",  duration);
    }
    
    // 毒液飞散
    [ScriptMethod(name: "Boss1-毒液飞散:圆形AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^3851[89]$"])]
    public void PoisonCircle(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"毒液飞散:圆形AOE提示";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(6);
        dp.Position = DeserializeVector3(@event["EffectPosition"]);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }
    // 嚣张跋扈
    // 36463
    // 36464
    // 36465 右
    // 36466 左
    // 36467 后
    [ScriptMethod(name: "Boss1-嚣张跋扈:随机顺序扇形攻击", eventType: EventTypeEnum.StartCasting, 
        eventCondition: ["ActionId:regex:^3646[5-7]$"])]
    public void Arrogant(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"嚣张跋扈:随机顺序扇形攻击";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(25);
        if(DeserializeInt(@event["ActionId"]) == 36465)
        {
            dp.Radian = 7*float.Pi/6;
            dp.Rotation = -float.Pi/2;
        }
        else if (DeserializeInt(@event["ActionId"]) == 36466)
        {
            dp.Radian = 7*float.Pi/6;
            dp.Rotation = float.Pi/2;
        }
        else
        {
            dp.Radian = float.Pi/2;
            dp.Rotation = float.Pi;
        }
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"])+7000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }
    
    // Boss 2
    // 回旋臂
    [ScriptMethod(name: "Boss2-回旋臂:左前右后/右前左后扇形AOE", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:36370"])]
    public void Boss2Fan(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        
        dp.Name = $"Boss2-回旋臂";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(30);
        dp.Radian = float.Pi/2;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }
    
    // 齐射
    // 36372 危险
    // 36373 安全
    [ScriptMethod(name: "Boss2-齐射:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36372"])]
    public void Boss2Straight(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss2-齐射:直线";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(4,40);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Rect,dp);
    }
    
    // 激光炮
    // 36366 危险
    // 38807 安全
    [ScriptMethod(name: "Boss2-激光炮:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36366"])]
    public void Boss2Laser(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss2-激光炮:直线";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(10,40);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Rect,dp);
    }
    
    // 急进电涌
    [ScriptMethod(name:"Boss2-急进电涌:击退提示",eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36367"])]
    public void Boss2Charge(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("向实体盒子击退（防击退有效）！",  duration);
    }
    
    // Boss 3
    // 压制强攻
    [ScriptMethod(name: "Boss3-压制强攻:半场扇形", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:39233"])]
    public void Boss3NorthSouth(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-压制强攻";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(26);
        dp.Radian = float.Pi;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }
    
    // 念动反应
    [ScriptMethod(name: "Boss3-念动反应:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36428"])]
    public void Boss3Straight(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-念动反应:直线";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(13,70);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    // 念动排斥
    // 36433 纵向击退
    // 36434 横向击退
    // 横向击退格绘制
    [ScriptMethod(name: "Boss3-念动排斥:击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36434"])]
    public void Boss3Horizontal(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-念动排斥:横向击退";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Scale = new Vector2(20,17);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Straight,dp);
    }
    
    // 念动排斥
    // 39055
    [ScriptMethod(name:"Boss3-念动排斥:击退提示",eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39055"])]
    public void Boss3Charge(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("击退至安全区",  duration);
    }
    
    // 念动压制 39055
    [ScriptMethod(name: "Boss3-念动压制:顺劈",eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39055"])]
    public void Boss3Charge2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-念动压制:顺劈";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(26);
        dp.Radian= float.Pi;
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }
    
    // 雷枪投掷-念动反应 38953
    // 雷枪收回 36431
    [ScriptMethod(name: "Boss3-雷枪投掷:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38953"])]
    public void Boss3ThunderStraight(Event @event, ScriptAccessory accessory)
    {
        lock (_lockObject)
        {
            _aoeCount++;
        }
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-雷枪投掷:念动反应";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10,45);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"])+5000;
        if (_aoeCount == 7)
        {
            dp.TargetObject = _thunderId;
            lock (_lockObject)
            {
                InitData(); 
            }
        }
        else if (_aoeCount == 6)
        {
            _thunderId = sid;
            dp.Rotation = float.Pi;
        }
        else
        {
            dp.Rotation = float.Pi;
        }
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Rect,dp);
    }
    
    [ScriptMethod(name: "Boss3-雷枪收回", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36431"])]
    public void Boss3ThunderStraight2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"Boss3-雷枪收回";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new Vector2(10,33);
        dp.DestoryAt = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Rect,dp);
    }
    
    

}
using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
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

[ScriptType(guid: "123e4567-e89b-12d3-a456-426614174000", name: "丛林竞流生息河岸", territorys: [1167], version: "0.0.0.1",
    author: "Poetry")]
public class Ihuykatumu
{
    private Vector3 SW;
    private Vector3 NE;
    private uint bossId = 0;
    public void Init(ScriptAccessory accessory)
    {
        accessory.Method.RemoveDraw(".*");
        SW = DeserializeVector3("{\"X\":-120.76,\"Y\":-118.00,\"Z\":279.09}");
        NE = DeserializeVector3("{\"X\":-93.17,\"Y\":-118.00,\"Z\":250.93}");
        bossId = 0;
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
    
    // // 死刑
    // [ScriptMethod(name: "死刑文字提示", eventType: EventTypeEnum.StartCasting, 
    //     eventCondition: ["ActionId:regex:^(39132|36347)$"])]
    // public void Death(Event @event, ScriptAccessory accessory)
    // {
    //     var duration = DeserializeInt(@event, "DurationMilliseconds");
    //     accessory.Method.TextInfo("死刑", duration);
    // }
    //
    
    // AOE
    [ScriptMethod(name: "AOE文字提示", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(36492|36507|36341)$"])]
    public void Boss1Aoe(Event @event, ScriptAccessory accessory)
    {
        var duration = DeserializeInt(@event["DurationMilliseconds"]);
        accessory.Method.TextInfo("AOE",  duration);
    }
    
    
    
    
    //Boss1_首领海牛
    
    // 重吐于世
    // 塌方（圆圈）
    [ScriptMethod(name:"Boos1-塌方:圆圈",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36502|36498|36497)$"])]
    public void CollapseCircle(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var idStr = @event["ActionId"];
        var drawScale = idStr switch
        {
            "36502" => new Vector2(6),
            "36497" => new Vector2(12),
            "36498" => new Vector2(8),
            _ => new Vector2(0)
        };
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"塌方:圆圈提示";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.Position= DeserializeVector3(@event["EffectPosition"]);
        dp.Scale = drawScale;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,dp);
    }
    
    // 塌方（直线）
    [ScriptMethod(name: "Boos1-塌方:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36503|36504|36499|36500)$"])]
    public void CollapseLine(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var idStr = @event["ActionId"];
        var drawScale = idStr switch
        {
            "36503" => new Vector2(6, 25),
            "36499" => new Vector2(6, 25),
            "36504" => new Vector2(10, 35),
            "36500" => new Vector2(10, 35),
            _ => new Vector2(0)
        };
            
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"塌方:直线提示";
        dp.Color = new Vector4(1f,0.886f,0f,1f);
        dp.Owner = sid;
        dp.Scale = drawScale;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Rect,dp);
    }
    
    // 腐烂（月环）
    [ScriptMethod(name: "Boos1-腐烂:月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36505"])]
    public void Rotting(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"腐烂:月环";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.InnerScale = new Vector2(6);
        dp.Scale = new Vector2(40);
        dp.DestoryAt = 3000;
        dp.Radian = float.Pi * 2;
        dp.Delay = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Donut,dp);
    }
    
    
    // Boss2_瞌睡怪
    // 瞌睡舞-敲击(直线）
    [ScriptMethod(name: "Boos2-瞌睡舞-敲击:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36479|36482)$"])]
    public void Knock(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var idStr = @event["ActionId"];
        var drawScale = idStr switch
        {
            "36479" => new Vector2(10, 40),
            "36482" => new Vector2(16, 40),
            _ => new Vector2(0)
        };
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"瞌睡舞-敲击:直线提示";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.Scale = drawScale;
        dp.DestoryAt = 7000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    
    // Boos3_亚波伦
    
    // 飞蝗之刃（直线）
    [ScriptMethod(name: "Boos3-飞蝗之刃:直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36346"])]
    public void Blade(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"飞蝗之刃:直线提示";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.Owner = sid;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(12,50);
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }
    
    //闪电镰（扇状放电）
    [ScriptMethod(name: "Boos3-闪电镰:扇状放电", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36351"])]
    public void Lightning(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"闪电镰:扇状放电提示";
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.DestoryAt = 8000;
        dp.Owner = sid;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Scale = new(40);
        dp.Radian = float.Pi/4;
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }
    
    // 唤风-风刃
    /* SW {"X":-120.76,"Y":-118.00,"Z":279.09}
     
     {"X":-102.93,"Y":-118.00,"Z":274.36}
     {"X":-108.92,"Y":-118.00,"Z":276.53}
     {"X":-105.73,"Y":-118.00,"Z":252.34}
     */
    
    /* NW {"X":-93.17,"Y":-118.00,"Z":250.93}
     
     {"X":-111.688,"Y":-118.000,"Z":253.942}
     {"X":-102.276,"Y":-118.000,"Z":264.313}
     {"X":-108.922,"Y":-118.000,"Z":276.528}
     */
    [ScriptMethod(name:"获取BossId", eventType: EventTypeEnum.StartCasting, eventCondition:["ActionId:36359"],userControl:false)]
    public void FindBossId(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        bossId = sid;
    }
    
    [ScriptMethod(name: "Boos3-唤风-风刃", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16748"])]
    public void CuttingWind(Event @event, ScriptAccessory accessory)
    {
        var spo = DeserializeVector3(@event["SourcePosition"]);
        
        float epsilon = 1e-5f;
        bool isSW = Math.Abs(spo.X - SW.X) < epsilon;
        
        var delays = new long[] { 8600, 16700, 24700 };
        Vector3[] pointsSw =
        [
            new Vector3(-102.935f, -118.000f, 274.357f),
            new Vector3(-108.935f, -118.000f, 262.224f),
            new Vector3(-105.733f, -118.000f, 252.340f)
        ];
        Vector3[] pointsNw =
        [
            new Vector3(-111.688f, -118.000f, 253.942f),
            new Vector3(-102.276f, -118.000f, 264.313f),
            new Vector3(-108.922f, -118.000f, 276.528f)
        ];
        
        if (isSW)
        {
            accessory.Log.Debug("风刃SW");
            WindAoe(@event, accessory, pointsSw[0], 0, delays[0]);
            WindAoe(@event, accessory, pointsSw[1], delays[0], delays[1]-delays[0]);
            WindAoe(@event, accessory, pointsSw[2], delays[1], delays[2]-delays[1]);
        }
        else
        {
            accessory.Log.Debug("风刃NW");
            WindAoe(@event, accessory, pointsNw[0], 0, delays[0]);
            WindAoe(@event, accessory, pointsNw[1], delays[0], delays[1]-delays[0]);
            WindAoe(@event, accessory, pointsNw[2], delays[1], delays[2]-delays[1]);
        }


    }
    
    //aoe绘制组
    public void WindAoe(Event @event, ScriptAccessory accessory,Vector3 point, long delay,long destoryAt)
    {
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        var dp3 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"风刃:1";
        dp1.Color = accessory.Data.DefaultDangerColor;
        dp1.Delay = delay;
        dp1.DestoryAt = destoryAt;
        dp1.Owner = bossId;
        dp1.Scale = new (8,72);
        dp1.Position = point;
        
        dp2.Name = $"风刃:2";
        dp2.Color = accessory.Data.DefaultDangerColor;
        dp2.Delay = delay;
        dp2.DestoryAt = destoryAt;
        dp2.Owner = bossId;
        dp2.Scale = new (8,72);
        dp2.Rotation = float.Pi/4;
        dp2.Position = point;
        
        dp3.Name = $"风刃:3";
        dp3.Color = accessory.Data.DefaultDangerColor;
        dp3.Delay = delay;
        dp3.DestoryAt = destoryAt;
        dp3.Owner = bossId;
        dp3.Scale = new (8,72);
        dp3.Rotation = float.Pi/2;
        dp3.Position = point;
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp1);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp2);
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp3);
    }
    
}
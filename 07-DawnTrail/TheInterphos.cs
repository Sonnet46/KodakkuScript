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

#pragma warning disable CA1416

namespace KodakkuScript.Script._07_DawnTrail;

[ScriptType(guid: "1B2EE1F8-D74A-9B08-7E4A-1B02B8D4D5DE", name: "永恒女王歼灭战", territorys: [1202], version: "0.0.0.1",
        author: "Poetry")]
public class TheInterphos
{
    private bool stopBuff = false;
    public void Init(ScriptAccessory accessory)
    {
        stopBuff = false ;  
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
    // 左右刀，该技能存在两次都打同一边的情况。
    [ScriptMethod(name: "合法武力", eventType: EventTypeEnum.StartCasting,
        eventCondition: ["ActionId:regex:^(366(38|39|41|42))$"])]
    public void LegitimateForce(Event @event, ScriptAccessory accessory)
    {
        // 36638 
        // 36640
        
        // 36639 先左后右
        // 36643
        
        // 36641 先右后左
        // 36642
        
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var idStr = @event["ActionId"];
        var firstLeft = idStr switch
        {
            "36638" => true,
            "36639" => true,
            "36640" => false,
            "36641" => false,
            _ => false
        };
        var secondLeft = idStr switch
        {
            "36638" => true,
            "36639" => false,
            "36640" => false,
            "36641" => true,
            _ => false
        };
        var dp1 = accessory.Data.GetDefaultDrawProperties();
        var dp2 = accessory.Data.GetDefaultDrawProperties();
        dp1.Name = $"左右刀-先{(firstLeft ? "左" : "右")}";
        dp1.Color = accessory.Data.DefaultDangerColor.WithW(4);
        dp1.Owner = sid;
        dp1.Scale = new(100);
        dp1.Rotation = firstLeft ? float.Pi/-2 : float.Pi/ 2;
        dp1.DestoryAt = 7700;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp1);
        
        dp2.Name = $"左右刀-后{(secondLeft ? "左" : "右")}";
        dp2.Color = accessory.Data.DefaultDangerColor.WithW(4);
        dp2.Owner = sid;
        dp2.Scale = new(100);
        dp2.Delay = 8000;
        dp2.DestoryAt = 3500;
        dp2.Rotation = secondLeft ? float.Pi/-2 : float.Pi/ 2;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);
    }

    // // DotAoE+左中右扇形范围AoE
    // [ScriptMethod(name: "以太税", eventType: EventTypeEnum.StartCasting,
    //     eventCondition: ["ActionId:regex:^(36604)$"])]
    // public void Aethertithe(Event @event, ScriptAccessory accessory)
    // {
    //   
    // }
    
    // 诉诸武力
    [ScriptMethod(name: "诉诸武力:死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36602"])]
    public void 诉诸武力(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("死刑", 5000);
    }
    
    // // 王土、虚景切换、
    // [ScriptMethod(name: "AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:3660[678]"])]
    // public void VirtualShift(Event @event, ScriptAccessory accessory)
    // {
    //     accessory.Method.TextInfo("AOE", 5000);
    // }
    //
    
    
    // [ScriptMethod(name: "护城墙", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36613"])]
    // public void Castellation(Event @event, ScriptAccessory accessory)
    // {
    //     
    // }
    
    
    //以场地上的一点为中心造成全屏击退效果
    [ScriptMethod(name: "下行突风 击退连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36610"])]
    public void DownwardGale(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("斜角击退", 6000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"斜角击退";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(2, 10);
        dp.Rotation = float.Pi;
        dp.Owner = accessory.Data.Me;
        dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
        dp.TargetPosition = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        dp.DestoryAt = 6000;
        
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
    }
    
    // AoE，并从场地的一侧造成接近场地宽度的横向击退效果
    [ScriptMethod(name: "强风 击退位置提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36612"])]
    public void PowerfulGust(Event @event, ScriptAccessory accessory)
    {
        accessory.Method.TextInfo("横向击退", 6000);
        
        var targetPos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
        var isRight = targetPos.X > 110;
        var rPos = new Vector3(110, 0, 82);
        var lPos = new Vector3(90, 0, 82);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"横向击退";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.ScaleMode |= ScaleMode.YByDistance;
        dp.Scale = new(2);
        dp.Owner = accessory.Data.Me;
        dp.Color = dp.Color = accessory.Data.DefaultSafeColor;
        dp.TargetPosition = isRight ? rPos : lPos;
        dp.DestoryAt = 6000;
        
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }
    
    //召唤出数个终端机组成圆，随后对范围之外造成环形伤害。
    [ScriptMethod(name: "王权残暴:月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36633"])]
    public void BrutalCrown(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        accessory.Method.TextInfo("月环", 8000);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"月环";
        dp.Scale = new(60);
        dp.InnerScale = new(5);
        dp.Radian = float.Pi * 2;
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = 8000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
    }
    
    // 绝对君权
    [ScriptMethod(name: "绝对君权 圆圈高亮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36622"])]
    public void 圆圈提示(Event @event, ScriptAccessory accessory)
    {   
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"绝对君权";
        dp.Scale = new(8);
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(5);
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    
    // 4130 空间掌控：制动
    // 3815 空间掌控：石化光 
    [ScriptMethod(name: "绝对君权 buff提示-1", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4130"])]
    public void Buff收集1(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        if (tid == accessory.Data.Me)
        {
            stopBuff = true;
            accessory.Method.TextInfo("稍后停止移动", 10000);
            Thread.Sleep(15000);
            accessory.Method.TextInfo("停止移动", 3000);
            accessory.Method.TTS("停止移动");

        }
    }
    [ScriptMethod(name: "绝对君权 buff提示-2", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3815"])]
    public void Buff收集2(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        if (tid == accessory.Data.Me)
        {
            stopBuff = true;
            accessory.Method.TextInfo("稍后背对Boss", 10000);
            Thread.Sleep(15000);
            accessory.Method.TextInfo("背对Boss", 3000);
            accessory.Method.TTS("背对Boss");

        }
    }
    

}

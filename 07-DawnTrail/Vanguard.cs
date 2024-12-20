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

#pragma warning disable CA1416

[ScriptType(guid: "e3b0c442-98fc-1c14-9ddf-4b9b8a8f1a1f", name: "前哨基地先锋营", territorys: [1198], version: "0.0.0.1",
    author: "Poetry")]
public class Vanguard
{
    // Boss1
    [ScriptMethod(name: "高速机动:钢铁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36559|39141)$"])]
    public void EnhancedMobility1(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"高速机动:钢铁";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.Scale = new(17);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(5);
        dp.DestoryAt = 9000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    [ScriptMethod(name:"高速机动:月环",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36560|39140)$"])]
    public void EnhancedMobility2(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"高速机动:月环";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = sid;
        dp.InnerScale = new(14);
        dp.Scale = new(60);
        dp.Radian = float.Pi;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Donut,dp);
    }

    // [ScriptMethod(name:"高速机动:右刀直线",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36559|36560)$"])]
    // public void EnhancedMobility3(Event @event,ScriptAccessory accessory)
    // {
    //     var dp = accessory.Data.GetDefaultDrawProperties();
    //     dp.Color = accessory.Data.DefaultDangerColor;
    // }

    [ScriptMethod(name: "召集小队:突进", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36569"])]
    public void DispatchRush(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召集小队:突进";
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Scale = new(5,40);
        dp.DestoryAt = 6000;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Rect,dp);
    }

   [ScriptMethod(name: "召集小队:空袭", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36570"])]
    public void DispatchAirstrike(Event @event, ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"召集小队:空袭";
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Position = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
        dp.Scale = new(14);
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Owner = sid;
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = dms+100;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
    }

    // Boss2
    // [ScriptMethod(name: "自控式冲击炮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId :regex:^(36570|37151)$"])]
    // public void SelfControlledImpactGun(Event @event,ScriptAccessory accessory)
    // {
    //     
    // }

    [ScriptMethod(name:"加速度炸弹:TTS",eventType:EventTypeEnum.StatusAdd,eventCondition:["StatusID:3802"])]
    public void DynamicInductionBomb(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        if (tid == accessory.Data.Me)
        {
            accessory.Method.TTS("稍后停止行动");
            Thread.Sleep(dms-3000);
            accessory.Method.TextInfo("停止行动", 3000);
            accessory.Method.TTS("停止移动");
        }
    }
    
    //Boss3
    // [ScriptMethod(name:"祸魂剑",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^(36574|36589|36590)$"])]
    // public void SoulbaneSaber(Event @event,ScriptAccessory accessory)
    // {
    // }

    [ScriptMethod(name:"祸魂剑-爆炸:半场",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^365(75|91)$"])]
    public void SoulbaneSaberBurst(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"祸魂剑-爆炸";
        dp.Owner = sid;
        dp.Scale = new(19);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
        dp.DestoryAt = dms;
        dp.ScaleMode = ScaleMode.ByTime;
        dp.Radian = float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
    }

    [ScriptMethod(name:"曲蛇融魂斩",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^3658[0-8]$"])]
    public void Syntheslither(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["TargetId"], out var tid)) return;
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"曲蛇融魂斩";
        dp.Owner = tid;
        dp.Scale = new(19);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.DestoryAt = dms;
        dp.Radian = float.Pi/2;
        accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Fan,dp);
        
    }
    
    // Slitherbane Foreguard,Slitherbane Rearguard
    [ScriptMethod(name:"前/后尾祸剑击",eventType:EventTypeEnum.StartCasting,eventCondition:["ActionId:regex:^3659[23]$"])]
    public void Slitherbane(Event @event,ScriptAccessory accessory)
    {
        if (!ParseObjectId(@event["SourceId"], out var sid)) return;
        var aid = JsonConvert.DeserializeObject<uint>(@event["ActionId"]);
        var dms = JsonConvert.DeserializeObject<int>(@event["DurationMilliseconds"]);
        bool isFront = aid % 2 == 0;

        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"{(isFront ? "前" : "后")}尾祸剑击";
        dp.Color = new Vector4(1f,0.886f,0f,1f);
        dp.Owner = sid;
        dp.Scale = new(19);
        dp.DestoryAt = dms;
        dp.Radian =float.Pi;
        dp.Rotation = isFront ? 0 : float.Pi;
        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Fan,dp);
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
}
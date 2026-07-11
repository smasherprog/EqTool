using System.Net;
using System.Text;

namespace EQToolApis.Services
{
    // Renders a character profile (PULSE-style dark layout) as HTML. Shared by the
    // Characters web page (fragment) and the desktop app's embedded browser panel
    // (full document). CSS/JS stay ES5- and IE11-safe (floats/inline-block, no
    // arrow functions) because the desktop panel hosts the legacy WebBrowser control.
    public static class CharacterProfileHtml
    {
        public const string Css = @"
.pp-profile, .pp-message { font-family: 'Segoe UI', Arial, sans-serif; color: #cdd3e6; font-size: 13px; }
.pp-profile *, .pp-profile { box-sizing: border-box; }
.pp-panel { background: #131829; border: 1px solid #242c49; border-radius: 10px; margin: 0 0 14px 0; }
.pp-clear { clear: both; }
.pp-stats { float: left; width: 250px; padding: 16px; }
.pp-main { margin-left: 266px; }
@media (max-width: 700px) {
    .pp-stats { float: none; width: auto; }
    .pp-main { margin-left: 0; }
}
.pp-charname { font-size: 20px; font-weight: bold; color: #ffffff; }
.pp-updated { color: #6b7390; font-size: 11px; margin: 2px 0 12px 0; }
.pp-core { border-top: 1px solid #242c49; border-bottom: 1px solid #242c49; padding: 10px 0; }
.pp-core-cell { display: inline-block; width: 24%; text-align: center; }
.pp-core-label { color: #6b7390; font-size: 11px; }
.pp-core-val { color: #ffffff; font-weight: bold; font-size: 15px; }
.pp-sec { color: #ffffff; font-weight: bold; margin: 14px 0 4px 0; }
.pp-statrow { padding: 2px 0 2px 8px; }
.pp-stat-label { color: #9aa3c0; display: inline-block; width: 55%; }
.pp-stat-val { color: #4ade80; font-weight: bold; }
.pp-stat-val.pp-neg { color: #f87171; }
.pp-stat-val.pp-dim { color: #565e7c; font-weight: normal; }
.pp-note { color: #565e7c; font-size: 10px; margin-top: 16px; }
.pp-h { color: #ffffff; font-weight: bold; font-size: 15px; padding: 13px 16px 11px 16px; border-bottom: 1px solid #242c49; }
.pp-equip-rows { padding: 12px; text-align: center; }
.pp-erow { margin: 2px 0; font-size: 0; }
.pp-slot { display: inline-block; position: relative; width: 46px; height: 46px; background: #0d1120; border: 1px solid #262e4a; border-radius: 6px; margin: 2px; vertical-align: top; font-size: 11px; }
.pp-slot img { width: 44px; height: 44px; border: 0; border-radius: 5px; display: block; }
.pp-slot-item { cursor: pointer; }
.pp-slot-item:hover { border-color: #8b93b8; }
.pp-count { position: absolute; right: 1px; bottom: 1px; background: #1e2745; color: #cdd3e6; font-size: 10px; padding: 0 4px; border-radius: 4px; }
.pp-tabs { padding: 12px 16px 0 16px; border-bottom: 1px solid #242c49; }
.pp-tab { display: inline-block; color: #6b7390; text-decoration: none; font-weight: bold; padding: 4px 2px 8px 2px; margin-right: 22px; border-bottom: 2px solid transparent; cursor: pointer; }
.pp-tab:hover { color: #cdd3e6; text-decoration: none; }
.pp-tab-active, .pp-tab-active:hover { color: #ffffff; border-bottom-color: #e8ecf8; }
.pp-pane { padding: 12px 16px 16px 16px; font-size: 0; }
.pp-bagcol { display: inline-block; vertical-align: top; margin-right: 24px; font-size: 0; }
.pp-bag { display: block; margin: 6px 0 12px 0; font-size: 11px; }
.pp-bag-grid { width: 100px; font-size: 0; }
.pp-shared { border-top: 1px solid #242c49; margin-top: 10px; padding-top: 8px; }
.pp-shared-h { color: #6b7390; font-size: 11px; font-weight: bold; margin-bottom: 2px; }
#pp-tooltip { position: fixed; display: none; z-index: 99999; background: #0c101f; border: 1px solid #3c4670; border-radius: 8px; padding: 10px 12px; color: #dfe4f5; font-family: Consolas, 'Courier New', monospace; font-size: 12px; white-space: pre-wrap; max-width: 340px; line-height: 1.45; box-shadow: 0 6px 24px rgba(0,0,0,0.6); }
.pp-message { color: #9aa3c0; text-align: center; padding: 60px 20px; font-size: 14px; }
";

        public const string Js = @"
function ppOn(el, evt, fn) {
    if (el.addEventListener) { el.addEventListener(evt, fn, false); }
    else if (el.attachEvent) { el.attachEvent('on' + evt, function () { fn(window.event); }); }
}
function ppSlotFrom(el) {
    while (el && el !== document) {
        if (el.getAttribute && el.getAttribute('data-tt')) { return el; }
        el = el.parentNode;
    }
    return null;
}
function ppTip() {
    var tip = document.getElementById('pp-tooltip');
    if (!tip) {
        tip = document.createElement('div');
        tip.id = 'pp-tooltip';
        document.body.appendChild(tip);
    }
    return tip;
}
function ppMoveTip(e) {
    var tip = ppTip();
    var w = tip.offsetWidth || 300, h = tip.offsetHeight || 150;
    var x = e.clientX + 14, y = e.clientY + 12;
    var winW = document.documentElement.clientWidth, winH = document.documentElement.clientHeight;
    if (x + w > winW - 8) { x = e.clientX - w - 14; }
    if (y + h > winH - 8) { y = winH - h - 8; }
    if (x < 0) { x = 0; }
    if (y < 0) { y = 0; }
    tip.style.left = x + 'px';
    tip.style.top = y + 'px';
}
ppOn(document, 'mouseover', function (e) {
    var slot = ppSlotFrom(e.target || e.srcElement);
    if (!slot) { return; }
    var tip = ppTip();
    var text = slot.getAttribute('data-tt');
    if ('textContent' in tip) { tip.textContent = text; } else { tip.innerText = text; }
    tip.style.display = 'block';
    ppMoveTip(e);
});
ppOn(document, 'mouseout', function (e) {
    if (ppSlotFrom(e.target || e.srcElement)) { ppTip().style.display = 'none'; }
});
ppOn(document, 'mousemove', function (e) {
    var tip = document.getElementById('pp-tooltip');
    if (tip && tip.style.display === 'block') { ppMoveTip(e); }
});
function ppShowTab(name) {
    var panes = { inv: document.getElementById('pp-pane-inv'), bank: document.getElementById('pp-pane-bank') };
    var btns = { inv: document.getElementById('pp-btn-inv'), bank: document.getElementById('pp-btn-bank') };
    for (var key in panes) {
        if (!panes[key] || !btns[key]) { continue; }
        panes[key].style.display = key === name ? '' : 'none';
        btns[key].className = 'pp-tab' + (key === name ? ' pp-tab-active' : '');
    }
}
";

        // PULSE-style paperdoll: rings bookend row 2, weapons row centered at the
        // bottom. Held (cursor) is rendered in the Inventory tab when occupied.
        private static readonly string[][] EquipRows =
        [
            ["Charm", "Ear1", "Head", "Face", "Ear2"],
            ["Finger1", "Neck", "Shoulders", "Back", "Hands", "Finger2"],
            ["Wrist1", "Arms", "Chest", "Waist", "Legs", "Feet", "Wrist2"],
            ["Primary", "Secondary", "Range", "Ammo"],
        ];

        private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

        private static string Slot(ProfileItem? item, string assetBase)
        {
            if (item == null)
            {
                return "<span class=\"pp-slot\"></span>";
            }

            var tooltip = string.IsNullOrEmpty(item.Tooltip) ? item.Name : item.Tooltip;
            var tt = Enc(tooltip).Replace("\r", string.Empty).Replace("\n", "&#10;");
            var count = item.Count > 1 ? $"<span class=\"pp-count\">{item.Count}</span>" : string.Empty;
            return $"<span class=\"pp-slot pp-slot-item\" data-tt=\"{tt}\"><img src=\"{Enc(assetBase + item.Image)}\" alt=\"\"/>{count}</span>";
        }

        private static string StatRow(string label, string value, string valueClass = "")
        {
            var cls = string.IsNullOrEmpty(valueClass) ? "pp-stat-val" : $"pp-stat-val {valueClass}";
            return $"<div class=\"pp-statrow\"><span class=\"pp-stat-label\">{label}</span><span class=\"{cls}\">{value}</span></div>";
        }

        private static string BonusRow(string label, int value)
        {
            return value == 0
                ? StatRow(label, "&mdash;", "pp-dim")
                : StatRow(label, (value > 0 ? "+" : string.Empty) + value, value < 0 ? "pp-neg" : string.Empty);
        }

        private static string CoreCell(string label, string value) =>
            $"<div class=\"pp-core-cell\"><div class=\"pp-core-label\">{label}</div><div class=\"pp-core-val\">{value}</div></div>";

        private static string BagGroup(ProfileBag bag, string assetBase)
        {
            var sb = new StringBuilder("<div class=\"pp-bag\">");
            _ = sb.Append(Slot(bag.Container, assetBase));
            if (bag.Contents.Count > 0)
            {
                _ = sb.Append("<div class=\"pp-bag-grid\">");
                foreach (var item in bag.Contents)
                {
                    _ = sb.Append(Slot(item, assetBase));
                }
                _ = sb.Append("</div>");
            }
            _ = sb.Append("</div>");
            return sb.ToString();
        }

        public static string RenderFragment(CharacterProfile p, string assetBase = "")
        {
            var s = p.Stats;
            var sb = new StringBuilder();
            _ = sb.Append("<div class=\"pp-profile\">");

            // Left: stats panel
            _ = sb.Append("<div class=\"pp-panel pp-stats\">");
            _ = sb.Append($"<div class=\"pp-charname\">{Enc(p.CharacterName)}</div>");
            _ = sb.Append($"<div class=\"pp-updated\">{Enc(p.Server.ToString())} &middot; Updated {p.UpdatedAt:yyyy-MM-dd}</div>");
            _ = sb.Append("<div class=\"pp-core\">");
            _ = sb.Append(CoreCell("HP", s.HP == 0 ? "&mdash;" : "+" + s.HP));
            _ = sb.Append(CoreCell("Mana", s.Mana == 0 ? "&mdash;" : "+" + s.Mana));
            _ = sb.Append(CoreCell("AC", s.AC.ToString()));
            _ = sb.Append(CoreCell("ATK", s.Atk == 0 ? "&mdash;" : "+" + s.Atk));
            _ = sb.Append("</div>");
            _ = sb.Append("<div class=\"pp-sec\">Attributes</div>");
            _ = sb.Append(BonusRow("STR", s.Str));
            _ = sb.Append(BonusRow("STA", s.Sta));
            _ = sb.Append(BonusRow("AGI", s.Agi));
            _ = sb.Append(BonusRow("DEX", s.Dex));
            _ = sb.Append(BonusRow("WIS", s.Wis));
            _ = sb.Append(BonusRow("INT", s.Int));
            _ = sb.Append(BonusRow("CHA", s.Cha));
            _ = sb.Append("<div class=\"pp-sec\">Resists</div>");
            _ = sb.Append(BonusRow("Poison", s.SvPoison));
            _ = sb.Append(BonusRow("Magic", s.SvMagic));
            _ = sb.Append(BonusRow("Disease", s.SvDisease));
            _ = sb.Append(BonusRow("Fire", s.SvFire));
            _ = sb.Append(BonusRow("Cold", s.SvCold));
            _ = sb.Append("<div class=\"pp-sec\">Other</div>");
            _ = sb.Append(StatRow("Haste", s.Haste == 0 ? "&mdash;" : s.Haste + " %", s.Haste == 0 ? "pp-dim" : string.Empty));
            _ = sb.Append(StatRow("Weight", s.Weight.ToString("0.#")));
            _ = sb.Append("<div class=\"pp-note\">Totals from equipped item stats</div>");
            _ = sb.Append("</div>");

            // Right: equipped grid + inventory/bank tabs
            _ = sb.Append("<div class=\"pp-main\">");
            _ = sb.Append("<div class=\"pp-panel\"><div class=\"pp-h\">Equipped Items</div><div class=\"pp-equip-rows\">");
            foreach (var row in EquipRows)
            {
                _ = sb.Append("<div class=\"pp-erow\">");
                foreach (var key in row)
                {
                    _ = sb.Append(Slot(p.Equipped.TryGetValue(key, out var item) ? item : null, assetBase));
                }
                _ = sb.Append("</div>");
            }
            _ = sb.Append("</div></div>");

            _ = sb.Append("<div class=\"pp-panel\">");
            _ = sb.Append("<div class=\"pp-tabs\">");
            _ = sb.Append("<a id=\"pp-btn-inv\" class=\"pp-tab pp-tab-active\" onclick=\"ppShowTab('inv')\">Inventory</a>");
            _ = sb.Append("<a id=\"pp-btn-bank\" class=\"pp-tab\" onclick=\"ppShowTab('bank')\">Bank</a>");
            _ = sb.Append("</div>");

            // Bags render in two columns (inventory: 4 + 4, bank: 8 + 8).
            void BagColumns(List<ProfileBag> bags, int perColumn)
            {
                for (var start = 0; start < bags.Count; start += perColumn)
                {
                    _ = sb.Append("<div class=\"pp-bagcol\">");
                    foreach (var bag in bags.Skip(start).Take(perColumn))
                    {
                        _ = sb.Append(BagGroup(bag, assetBase));
                    }
                    _ = sb.Append("</div>");
                }
            }

            _ = sb.Append("<div id=\"pp-pane-inv\" class=\"pp-pane\">");
            BagColumns(p.General, 4);
            var held = p.Equipped.TryGetValue("Held", out var heldItem) ? heldItem : null;
            if (held != null)
            {
                _ = sb.Append("<div class=\"pp-shared\"><div class=\"pp-shared-h\">Held (cursor)</div>");
                _ = sb.Append(Slot(held, assetBase));
                _ = sb.Append("</div>");
            }
            _ = sb.Append("</div>");

            _ = sb.Append("<div id=\"pp-pane-bank\" class=\"pp-pane\" style=\"display:none\">");
            BagColumns(p.Bank, 8);
            _ = sb.Append("<div class=\"pp-shared\"><div class=\"pp-shared-h\">Shared Bank</div>");
            foreach (var item in p.SharedBank)
            {
                _ = sb.Append(Slot(item, assetBase));
            }
            _ = sb.Append("</div></div>");

            _ = sb.Append("</div>"); // pp-panel (tabs)
            _ = sb.Append("</div>"); // pp-main
            _ = sb.Append("<div class=\"pp-clear\"></div>");
            _ = sb.Append("</div>"); // pp-profile
            return sb.ToString();
        }

        // Full standalone page for the desktop app's embedded WebBrowser panel.
        public static string RenderDocument(CharacterProfile? profile, string message, string assetBase)
        {
            var body = profile != null
                ? RenderFragment(profile, assetBase)
                : $"<div class=\"pp-message\">{Enc(message)}</div>";
            return "<!DOCTYPE html><html><head>"
                + "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"/>"
                + "<meta charset=\"utf-8\"/>"
                + $"<title>{Enc(profile?.CharacterName ?? "Character")}</title>"
                + "<style>html,body{background:#0b0e1a;margin:0;padding:14px;}" + Css + "</style>"
                + "</head><body>" + body
                + "<script>" + Js + "</script>"
                + "</body></html>";
        }
    }
}

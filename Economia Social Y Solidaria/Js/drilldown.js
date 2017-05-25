﻿/*
 Highcharts JS v5.0.0 (2016-09-29)
 Highcharts Drilldown module

 Author: Torstein Honsi
 License: www.highcharts.com/license

*/
(function (n) { "object" === typeof module && module.exports ? module.exports = n : n(Highcharts) })(function (n) {
    (function (e) {
        function n(b, a, d) { var c; a.rgba.length && b.rgba.length ? (b = b.rgba, a = a.rgba, c = 1 !== a[3] || 1 !== b[3], b = (c ? "rgba(" : "rgb(") + Math.round(a[0] + (b[0] - a[0]) * (1 - d)) + "," + Math.round(a[1] + (b[1] - a[1]) * (1 - d)) + "," + Math.round(a[2] + (b[2] - a[2]) * (1 - d)) + (c ? "," + (a[3] + (b[3] - a[3]) * (1 - d)) : "") + ")") : b = a.input || "none"; return b } var A = e.noop, B = e.color, u = e.defaultOptions, h = e.each, p = e.extend, H = e.format, C = e.pick, v = e.wrap, q = e.Chart,
        t = e.seriesTypes, D = t.pie, r = t.column, E = e.Tick, w = e.fireEvent, F = e.inArray, G = 1; h(["fill", "stroke"], function (b) { e.Fx.prototype[b + "Setter"] = function () { this.elem.attr(b, n(B(this.start), B(this.end), this.pos)) } }); p(u.lang, { drillUpText: "\u25c1 Volver a {series.name}" }); u.drilldown = {
            activeAxisLabelStyle: { cursor: "pointer", color: "#003399", fontWeight: "bold", textDecoration: "underline" }, activeDataLabelStyle: { cursor: "pointer", color: "#003399", fontWeight: "bold", textDecoration: "underline" }, animation: { duration: 500 }, drillUpButton: {
                position: {
                    align: "right",
                    x: -10, y: 10
                }
            }
        }; e.SVGRenderer.prototype.Element.prototype.fadeIn = function (b) { this.attr({ opacity: .1, visibility: "inherit" }).animate({ opacity: C(this.newOpacity, 1) }, b || { duration: 250 }) }; q.prototype.addSeriesAsDrilldown = function (b, a) { this.addSingleSeriesAsDrilldown(b, a); this.applyDrilldown() }; q.prototype.addSingleSeriesAsDrilldown = function (b, a) {
            var d = b.series, c = d.xAxis, g = d.yAxis, f, k = [], x = [], e, m, y; y = { color: b.color || d.color }; this.drilldownLevels || (this.drilldownLevels = []); e = d.options._levelNumber || 0; (m = this.drilldownLevels[this.drilldownLevels.length -
            1]) && m.levelNumber !== e && (m = void 0); a = p(p({ _ddSeriesId: G++ }, y), a); f = F(b, d.points); h(d.chart.series, function (a) { a.xAxis !== c || a.isDrilling || (a.options._ddSeriesId = a.options._ddSeriesId || G++, a.options._colorIndex = a.userOptions._colorIndex, a.options._levelNumber = a.options._levelNumber || e, m ? (k = m.levelSeries, x = m.levelSeriesOptions) : (k.push(a), x.push(a.options))) }); f = p({
                levelNumber: e, seriesOptions: d.options, levelSeriesOptions: x, levelSeries: k, shapeArgs: b.shapeArgs, bBox: b.graphic ? b.graphic.getBBox() : {}, lowerSeriesOptions: a,
                pointOptions: d.options.data[f], pointIndex: f, oldExtremes: { xMin: c && c.userMin, xMax: c && c.userMax, yMin: g && g.userMin, yMax: g && g.userMax }
            }, y); this.drilldownLevels.push(f); f = f.lowerSeries = this.addSeries(a, !1); f.options._levelNumber = e + 1; c && (c.oldPos = c.pos, c.userMin = c.userMax = null, g.userMin = g.userMax = null); d.type === f.type && (f.animate = f.animateDrilldown || A, f.options.animation = !0)
        }; q.prototype.applyDrilldown = function () {
            var b = this.drilldownLevels, a; b && 0 < b.length && (a = b[b.length - 1].levelNumber, h(this.drilldownLevels,
            function (b) { b.levelNumber === a && h(b.levelSeries, function (c) { c.options && c.options._levelNumber === a && c.remove(!1) }) })); this.redraw(); this.showDrillUpButton()
        }; q.prototype.getDrilldownBackText = function () { var b = this.drilldownLevels; if (b && 0 < b.length) return b = b[b.length - 1], b.series = b.seriesOptions, H(this.options.lang.drillUpText, b) }; q.prototype.showDrillUpButton = function () {
            var b = this, a = this.getDrilldownBackText(), d = b.options.drilldown.drillUpButton, c, g; this.drillUpButton ? this.drillUpButton.attr({ text: a }).align() :
            (g = (c = d.theme) && c.states, this.drillUpButton = this.renderer.button(a, null, null, function () { b.drillUp() }, c, g && g.hover, g && g.select).addClass("highcharts-drillup-button").attr({ align: d.position.align, zIndex: 7 }).add().align(d.position, !1, d.relativeTo || "plotBox"))
        }; q.prototype.drillUp = function () {
            for (var b = this, a = b.drilldownLevels, d = a[a.length - 1].levelNumber, c = a.length, g = b.series, f, k, e, l, m = function (a) {
            var c; h(g, function (b) { b.options._ddSeriesId === a._ddSeriesId && (c = b) }); c = c || b.addSeries(a, !1); c.type === e.type &&
            c.animateDrillupTo && (c.animate = c.animateDrillupTo); a === k.seriesOptions && (l = c)
            }; c--;) if (k = a[c], k.levelNumber === d) {
                a.pop(); e = k.lowerSeries; if (!e.chart) for (f = g.length; f--;) if (g[f].options.id === k.lowerSeriesOptions.id && g[f].options._levelNumber === d + 1) { e = g[f]; break } e.xData = []; h(k.levelSeriesOptions, m); w(b, "drillup", { seriesOptions: k.seriesOptions }); l.type === e.type && (l.drilldownLevel = k, l.options.animation = b.options.drilldown.animation, e.animateDrillupFrom && e.chart && e.animateDrillupFrom(k)); l.options._levelNumber =
                d; e.remove(!1); l.xAxis && (f = k.oldExtremes, l.xAxis.setExtremes(f.xMin, f.xMax, !1), l.yAxis.setExtremes(f.yMin, f.yMax, !1))
            } w(b, "drillupall"); this.redraw(); 0 === this.drilldownLevels.length ? this.drillUpButton = this.drillUpButton.destroy() : this.drillUpButton.attr({ text: this.getDrilldownBackText() }).align(); this.ddDupes.length = []
        }; r.prototype.supportsDrilldown = !0; r.prototype.animateDrillupTo = function (b) {
            if (!b) {
                var a = this, d = a.drilldownLevel; h(this.points, function (a) {
                    a.graphic && a.graphic.hide(); a.dataLabel && a.dataLabel.hide();
                    a.connector && a.connector.hide()
                }); setTimeout(function () { a.points && h(a.points, function (a, b) { var f = b === (d && d.pointIndex) ? "show" : "fadeIn", k = "show" === f ? !0 : void 0; if (a.graphic) a.graphic[f](k); if (a.dataLabel) a.dataLabel[f](k); if (a.connector) a.connector[f](k) }) }, Math.max(this.chart.options.drilldown.animation.duration - 50, 0)); this.animate = A
            }
        }; r.prototype.animateDrilldown = function (b) {
            var a = this, d = this.chart.drilldownLevels, c, g = this.chart.options.drilldown.animation, f = this.xAxis; b || (h(d, function (b) {
                a.options._ddSeriesId ===
                b.lowerSeriesOptions._ddSeriesId && (c = b.shapeArgs, c.fill = b.color)
            }), c.x += C(f.oldPos, f.pos) - f.pos, h(this.points, function (b) { b.shapeArgs.fill = b.color; b.graphic && b.graphic.attr(c).animate(p(b.shapeArgs, { fill: b.color || a.color }), g); b.dataLabel && b.dataLabel.fadeIn(g) }), this.animate = null)
        }; r.prototype.animateDrillupFrom = function (b) {
            var a = this.chart.options.drilldown.animation, d = this.group, c = this; h(c.trackerGroups, function (a) { if (c[a]) c[a].on("mouseover") }); delete this.group; h(this.points, function (c) {
                var f =
                c.graphic, k = b.shapeArgs, h = function () { f.destroy(); d && (d = d.destroy()) }; f && (delete c.graphic, k.fill = b.color, a ? f.animate(k, e.merge(a, { complete: h })) : (f.attr(k), h()))
            })
        }; D && p(D.prototype, {
            supportsDrilldown: !0, animateDrillupTo: r.prototype.animateDrillupTo, animateDrillupFrom: r.prototype.animateDrillupFrom, animateDrilldown: function (b) {
                var a = this.chart.drilldownLevels[this.chart.drilldownLevels.length - 1], d = this.chart.options.drilldown.animation, c = a.shapeArgs, g = c.start, f = (c.end - g) / this.points.length; b || (h(this.points,
                function (b, h) { var l = b.shapeArgs; c.fill = a.color; l.fill = b.color; if (b.graphic) b.graphic.attr(e.merge(c, { start: g + h * f, end: g + (h + 1) * f }))[d ? "animate" : "attr"](l, d) }), this.animate = null)
            }
        }); e.Point.prototype.doDrilldown = function (b, a, d) {
            var c = this.series.chart, g = c.options.drilldown, f = (g.series || []).length, e; c.ddDupes || (c.ddDupes = []); for (; f-- && !e;) g.series[f].id === this.drilldown && -1 === F(this.drilldown, c.ddDupes) && (e = g.series[f], c.ddDupes.push(this.drilldown)); w(c, "drilldown", {
                point: this, seriesOptions: e, category: a,
                originalEvent: d, points: void 0 !== a && this.series.xAxis.getDDPoints(a).slice(0)
            }, function (a) { var c = a.point.series && a.point.series.chart, d = a.seriesOptions; c && d && (b ? c.addSingleSeriesAsDrilldown(a.point, d) : c.addSeriesAsDrilldown(a.point, d)) })
        }; e.Axis.prototype.drilldownCategory = function (b, a) { var d, c, e = this.getDDPoints(b); for (d in e) (c = e[d]) && c.series && c.series.visible && c.doDrilldown && c.doDrilldown(!0, b, a); this.chart.applyDrilldown() }; e.Axis.prototype.getDDPoints = function (b) {
            var a = []; h(this.series, function (d) {
                var c,
                e = d.xData, f = d.points; for (c = 0; c < e.length; c++) if (e[c] === b && d.options.data[c].drilldown) { a.push(f ? f[c] : !0); break }
            }); return a
        }; E.prototype.drillable = function () {
            var b = this.pos, a = this.label, d = this.axis, c = "xAxis" === d.coll && d.getDDPoints, g = c && d.getDDPoints(b); c && (a && g.length ? (a.drillable = !0, a.basicStyles || (a.basicStyles = e.merge(a.styles)), a.addClass("highcharts-drilldown-axis-label").css(d.chart.options.drilldown.activeAxisLabelStyle).on("click", function (a) { d.drilldownCategory(b, a) })) : a && a.drillable && (a.styles =
            {}, a.css(a.basicStyles), a.on("click", null), a.removeClass("highcharts-drilldown-axis-label")))
        }; v(E.prototype, "addLabel", function (b) { b.call(this); this.drillable() }); v(e.Point.prototype, "init", function (b, a, d, c) { var g = b.call(this, a, d, c); b = (b = a.xAxis) && b.ticks[c]; g.drilldown && e.addEvent(g, "click", function (b) { a.xAxis && !1 === a.chart.options.drilldown.allowPointDrilldown ? a.xAxis.drilldownCategory(c, b) : g.doDrilldown(void 0, void 0, b) }); b && b.drillable(); return g }); v(e.Series.prototype, "drawDataLabels", function (b) {
            var a =
            this.chart.options.drilldown.activeDataLabelStyle, d = this.chart.renderer; b.call(this); h(this.points, function (b) { var e = {}; b.drilldown && b.dataLabel && ("contrast" === a.color && (e.color = d.getContrast(b.color || this.color)), b.dataLabel.addClass("highcharts-drilldown-data-label"), b.dataLabel.css(a).css(e)) }, this)
        }); var z, u = function (b) { b.call(this); h(this.points, function (a) { a.drilldown && a.graphic && (a.graphic.addClass("highcharts-drilldown-point"), a.graphic.css({ cursor: "pointer" })) }) }; for (z in t) t[z].prototype.supportsDrilldown &&
        v(t[z].prototype, "drawTracker", u)
    })(n)
});

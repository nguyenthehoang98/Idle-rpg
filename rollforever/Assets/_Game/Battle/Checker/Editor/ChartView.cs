using System;
using Syrus.Plugins.ChartEditor;
using UnityEngine;

namespace _Game.Battle.Checker.Editor
{
    public class ChartView : IDisposable
    {
        private ChartData chartData;
            
        public ChartView(ChartData chartData)
        {
            this.chartData = chartData;
            Rect rect = this.chartData.layoutRect;
            rect.size = new Vector2(rect.size.x, rect.size.y);
            rect.position = new Vector2(rect.position.x, rect.position.y);
            GUIChartEditor.BeginChart(
                chartData.layoutRect, chartData.backgroundColor,
                GUIChartEditorOptions.ChartBounds(-5, 105, -5, 105),
                GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
                GUIChartEditorOptions.ShowAxes(Color.white),
                GUIChartEditorOptions.ShowGrid(10f, 10f, Color.grey, true)
            );
        }

        public void DrawChart()
        {
            foreach (var p in chartData.points)
            {
                var points = p.NormalizePoints(chartData.heightNormalize);
                GUIChartEditor.PushLineChart(points, p.color);

                if (!p.drawNumber) continue;
                for (var i = 0; i < points.Length; i++)
                {
                    float value = p.values[i];
                    Vector2 point = points[i];
                    GUIChartEditor.PushPoint(point, p.color);
                    GUIChartEditor.PushValueLabel(value, point.x, point.y, p.floatFormat);
                }
            }
        }
            
        public void Dispose()
        {
            GUIChartEditor.EndChart(); 
        }
    }
}
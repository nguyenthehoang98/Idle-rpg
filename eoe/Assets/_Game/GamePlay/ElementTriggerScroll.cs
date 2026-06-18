using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace _Game.GamePlay
{
    public class ElementTriggerScroll : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller scroller;
        [SerializeField] private ElementTriggerCellView cellPrefab;

        private List<Color> colors = new List<Color>();

        private void Awake()
        {
            for (int i = 0; i < 4; i++)
            {
                colors.Add(Color.gray);
            }
        }

        private void Start()
        {
            scroller.Delegate = this;
            scroller.ReloadData();
        }

        public void Push(Color color)
        {
            if (colors.Count == 4)
            {
                colors.RemoveAt(0);
            }
            
            colors.Add(color);
            
            scroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return 1;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 24;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            ElementTriggerCellView cellView = scroller.GetCellView(cellPrefab) as
                ElementTriggerCellView;
            cellView.SetData(colors[dataIndex]);
            return cellView;
        }
    }
}
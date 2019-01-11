using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class ScaleableGridContent : MonoBehaviour
{
    [Header("Settings")]
    public int rows;
    public int cols;
    private GridLayoutGroup grid;
    private RectTransform rectTransform;

    private void Start()
    {
        grid = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        if (rows == 0)
        {
            rows = 1;
        }
        if (cols == 0)
        {
            cols = 1;
        }
        grid.cellSize = new Vector2(rectTransform.rect.width / cols, rectTransform.rect.height / rows);
    }
}

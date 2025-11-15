using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SelectBoard 
{
    private int m_selectBoardSize = 5;
    
    private Transform m_root;
    private GameSettings settings;
    List<Cell> cells ;
    public List<Cell> Cells => cells;
    public SelectBoard(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;
        settings = gameSettings;


        cells = new List<Cell>();

        
        CreatSelectBoard();
    }
    internal void CreatSelectBoard( )
    {
        
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELLSELECT_BACKGROUND);
        Vector3 selectOrigin = new Vector3(-m_selectBoardSize * 0.5f + 0.5f , -settings.BoardSizeY *0.5f - 0.5f  ,0);

        for(int x = 0 ; x < m_selectBoardSize ; x++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = selectOrigin + new Vector3(x, 0f, 0f);
            go.transform.SetParent(m_root);

            CellSelect cell = go.GetComponent<CellSelect>();
            cell.Index = x;
            cells.Add(cell);

            // cell.Setup(x, 0);
            
        }
    
    }
    internal bool IsCellsItemCountFull()
    {
        return cells[cells.Count -1].Item != null;
    }
    internal Cell FindSelectCellFree()
    {
        
        foreach (Cell cell in cells)
        {
            if(cell.Item == null) return cell;
        }
        return null;
    }
    
    internal bool CheckMatch3(out Vector2Int pairIndex)
    {
        pairIndex =  Vector2Int.zero;

        for(int i = 0 ; i < cells.Count-2 ; i++)
        {
            if(cells[i].Item == null || cells[i+2].Item ==null) return false;
            NormalItem normalItem = cells[i].Item as NormalItem;
            NormalItem normalItem3 = cells[i+2].Item as NormalItem;

            if(normalItem.IsSameType(normalItem3))
            {
                pairIndex = new Vector2Int(i ,i+2);
                return true;
            }
        } 

        
        return false;
    }

    internal void RemoveItemAtIndex(Vector2Int pairIndex , Action callback)
    {
        for(int x = pairIndex.x ; x<= pairIndex.y ; x++)
        {
            cells[x].ExplodeItem();
        }

        int i = 1;
        while(pairIndex.y + i < cells.Count)
        {
            NormalItem normalItem = cells[pairIndex.y + i].Item as NormalItem;

            if(normalItem == null) break;

            cells[pairIndex.y + i].Free();
            cells[pairIndex.x + i -1].Assign(normalItem);
            
            normalItem.View.DOMove(cells[pairIndex.x + i -1].transform.position, 0.3f);
            i++;
        }
        DOTween.Sequence().AppendInterval(0.3f).OnComplete(() => { callback?.Invoke(); });
        Debug.Log("???????????????????");
        


        
    }
    internal void InsertItemAtIndex(Item item , int index , Action callback)
    {
        for(int i = cells.Count -1 ; i > index ; i--)
        {
            if(cells[i].Item == null && cells[i-1].Item != null)
            {
                Item item1 = cells[i-1].Item;
                cells[i-1].Free();
                cells[i].Assign(item1);
                item1.View.DOMove(cells[i].transform.position, 0.3f);
            }
        }

        cells[index].Assign(item);
        if(CheckMatch3(out Vector2Int pairIndex))
        {
            item.View.DOMove(cells[index].transform.position, 0.3f).OnComplete(() => RemoveItemAtIndex(pairIndex , callback) );
            return;
        }
        
        
        item.View.DOMove(cells[index].transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
        
        Debug.Log("callback");
        // cellSelect.Assign(item);
        // item.View.DOMove(cellSelect.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });

    }

    internal bool FindMatch(NormalItem item , out int index)
    {
        for(int i = cells.Count - 1 ; i >=0 ; i--)
        {
            NormalItem normalItem = cells[i].Item as NormalItem;
            if(normalItem != null && normalItem.ItemType == item.ItemType )
            {
                index = i + 1;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public void AddSelect(Cell cell , Action callback)
    {
        Cell cellSelect  = FindSelectCellFree();

        if(cellSelect == null) return;


        Item item = cell.Item;

        if(item == null) return;
        cell.Free();


        if( FindMatch(item as NormalItem , out int matchIndex))
        {

            InsertItemAtIndex(item , matchIndex , callback);

            return;
        }

        cellSelect.Assign(item);
        item.View.DOMove(cellSelect.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
        
    }

    public Vector2Int GetIndexCellRedo(CellSelect cellSelect )
    {
        int index = cellSelect.Index;
        Item item =cellSelect.Item;

        return item.IndexOrigin;


    }


    

}

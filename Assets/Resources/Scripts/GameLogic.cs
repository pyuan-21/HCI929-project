using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Scripts
{
    public class GameLogic
    {
        #region for Instance
        private static GameLogic mInstance;
        public static GameLogic Instance
        {
            get
            {
                if(mInstance == null)
                {
                    mInstance = new GameLogic();
                }
                return mInstance;
            }
        }
        #endregion
        private int mCellRowNum = 3;
        private int mCellColNum = 3;
        private List<GameObject> mImageTargetList;
        private Dictionary<GameObject, int> mImageTarget2ImageIndexDic;
        private bool hasInit;
        private Action updateActList;

        private GameLogic()
        {
            mImageTargetList = new List<GameObject>();
            mImageTarget2ImageIndexDic = new Dictionary<GameObject, int>();
            hasInit = false;
        }

        public void AddImageTarget(GameObject obj)
        {
            mImageTargetList.Add(obj);
            if(mImageTargetList.Count >= mCellRowNum * mCellColNum)
            {
                if (!hasInit)
                {
                    OnInitGame();
                }
                else
                {
                    updateActList += CheckGameOver;
                }
            }
        }

        public void RemoveImageTarget(GameObject obj)
        {
            mImageTargetList.Remove(obj);
            if (hasInit)
            {
                updateActList -= CheckGameOver;
            }
        }

        private void Randomize(ref List<int> imageIndexList, int blankIndex, int needUnMatchedNum)
        {
            List<int> movableDirList = new List<int>();//left,right,up,down-0, 1, 2, 3 
            int rowIndex = blankIndex / mCellColNum;//blank current rowIndex
            int colIndex = blankIndex % mCellColNum;//blank current colIndex
            int dir;
            System.Random random = new System.Random();
            int unMatchedNum;
            int stepNum = 0;
            while (stepNum < 100)
            {
                movableDirList.Clear();
                if (rowIndex - 1 >= 0)
                {
                    movableDirList.Add(0);//left
                }
                if (rowIndex + 1 <= mCellColNum - 1)
                {
                    movableDirList.Add(1);//right
                }
                if (colIndex - 1 >= 0)
                {
                    movableDirList.Add(2);//up
                }
                if (colIndex + 1 <= mCellRowNum - 1)
                {
                    movableDirList.Add(3);//down
                }
                dir = movableDirList[random.Next(movableDirList.Count)];
                switch (dir)
                {
                    case 0:
                        rowIndex -= 1;
                        break;
                    case 1:
                        rowIndex += 1;
                        break;
                    case 2:
                        colIndex -= 1;
                        break;
                    case 3:
                        colIndex += 1;
                        break;
                }
                int newBlankIndex = rowIndex * mCellRowNum + colIndex;
                int temp = imageIndexList[newBlankIndex];
                imageIndexList[newBlankIndex] = imageIndexList[blankIndex];
                imageIndexList[blankIndex] = temp;
                blankIndex = newBlankIndex;

                unMatchedNum = 0;
                for (int i = 0; i < imageIndexList.Count; i++)
                {
                    if (i != imageIndexList[i])
                    {
                        unMatchedNum++;
                    }
                }
                if(unMatchedNum >= needUnMatchedNum)
                {
                    break;
                }
                stepNum++;
            }
        }

        private void SortImageTargetList()
        {
            //todo: check whether all imageTargets arrange in a rectangle
            //but for now, just assume all imageTarget arrange corectly
            //sort them from top to bottom(y descrease direction), then from left to right(x increase direction)
            //n-puzzle has n cells and each cell is a marker(include blank cell)
            //after init game, remove blank cell to play[todo: find a better way to init game maybe?]
            var tempList = new List<GameObject>();
            var sortedList = new List<GameObject>();
            while (mImageTargetList.Count > 0)
            {
                mImageTargetList.Sort((obj1, obj2) => { return obj1.transform.position.y > obj2.transform.position.y ? -1 : 1; });
                tempList.Clear();
                //int cnt = Mathf.Min(mCellColNum, mImageTargetList.Count);
                tempList.AddRange(mImageTargetList.GetRange(0, mCellColNum));
                mImageTargetList.RemoveRange(0, mCellColNum);
                tempList.Sort((obj1, obj2) => { return obj1.transform.position.x < obj2.transform.position.x ? -1 : 1; });
                sortedList.AddRange(tempList);
            }
            mImageTargetList.AddRange(sortedList);
        }

        public void OnInitGame()
        {
            try
            {
                SortImageTargetList();

                //correct image index arrange like 0,1,2,3,...,mCellRowNum*mCellColNum-1
                List<int> imageIndexList = new List<int>();
                for (int i = 0; i < mCellRowNum * mCellColNum; i++)
                {
                    imageIndexList.Add(i);
                }

                //randomize
                Randomize(ref imageIndexList, imageIndexList.Count - 1, mCellRowNum * mCellColNum / 2 + 1);

                //just for test
                string imageIndexStr = "";
                for(int i = 0; i < imageIndexList.Count; i++)
                {
                    imageIndexStr += (imageIndexList[i] + ", ");
                }
                Debug.Log(String.Format("imageIndexList: {0}", imageIndexStr));

                mImageTarget2ImageIndexDic.Clear();
                for(int i = 0; i < mImageTargetList.Count; i++)
                {
                    int imageIndex = imageIndexList[i];
                    mImageTarget2ImageIndexDic[mImageTargetList[i]] = imageIndex;

                    //set corresponding texture
                    GameObject cellObj = Utility.FindGameObject(mImageTargetList[i], "cell");
                    Renderer rend = cellObj.GetComponent<Renderer>();
                    var texture = UnityEngine.Resources.Load(String.Format("Images/cell_{0}", imageIndex)) as Texture;
                    rend.material.mainTexture = texture;
                }

                hasInit = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private bool CheckCellRectangle()
        {
            //todo: maybe there is a better way to check.
            float angleOffset = 8f;//to visualize angle, use this link: https://www.visnos.com/demos/basic-angles
            float threshold = Mathf.Cos(Mathf.PI * angleOffset / 180);
            //check each vector from cell to its two closed neighbor is vertical
            for(int i = 0; i < mImageTargetList.Count; i++)
            {
                int rowIndex = i / mCellColNum;
                int colIndex = i % mCellColNum;
                bool hasLeft = rowIndex - 1 >= 0;
                bool hasRight = rowIndex + 1 <= mCellColNum - 1;
                bool hasUp = colIndex - 1 >= 0;
                bool hasDown= colIndex + 1 <= mCellRowNum - 1;
                int neighborIndex1, neighborIndex2;
                if (hasLeft && hasUp)
                {
                    neighborIndex1 = (rowIndex - 1) * mCellRowNum + colIndex;
                    neighborIndex2 = rowIndex * mCellRowNum + (colIndex - 1);
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.y);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.y);
                    if(Mathf.Abs(Vector2.Dot(v1, v2)) > threshold)
                    {
                        //considered as not vertical
                        return false;
                    }
                }
                if(hasLeft && hasDown)
                {
                    neighborIndex1 = (rowIndex - 1) * mCellRowNum + colIndex;
                    neighborIndex2 = rowIndex * mCellRowNum + (colIndex + 1);
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.y);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.y);
                    if (Mathf.Abs(Vector2.Dot(v1, v2)) > threshold)
                    {
                        //considered as not vertical
                        return false;
                    }
                }
                if(hasRight && hasUp)
                {
                    neighborIndex1 = (rowIndex + 1) * mCellRowNum + colIndex;
                    neighborIndex2 = rowIndex * mCellRowNum + (colIndex - 1);
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.y);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.y);
                    if (Mathf.Abs(Vector2.Dot(v1, v2)) > threshold)
                    {
                        //considered as not vertical
                        return false;
                    }
                }
                if(hasRight && hasDown)
                {
                    neighborIndex1 = (rowIndex + 1) * mCellRowNum + colIndex;
                    neighborIndex2 = rowIndex * mCellRowNum + (colIndex + 1);
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.y);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.y);
                    if (Mathf.Abs(Vector2.Dot(v1, v2)) > threshold)
                    {
                        //considered as not vertical
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckIsAllMatch()
        {
            //if all cell are matched to the correct sequence, from the first one to last one, the indices should be from 0 to N.
            for(int i = 0; i < mImageTargetList.Count; i++)
            {
                if(i != mImageTarget2ImageIndexDic[mImageTargetList[i]])
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckGameOver()
        {
            SortImageTargetList();
            if (CheckCellRectangle())
            {
                Debug.Log("CheckCellRectangle!!!");
                //check
                if (CheckIsAllMatch())
                {
                    Debug.Log("Congratuations!!!");
                }
            }
        }

        public void Update()
        {
            updateActList?.Invoke();
        }
    }
}
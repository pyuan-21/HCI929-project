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
                if (mInstance == null)
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
        private Dictionary<GameObject, int> mImageTarget2ImageIndexDict;
        private bool mHasInit;
        private Action mUpdateActList;
        private Dictionary<string, List<Texture2D>> mTextureDict;
        private int mCurrentImgIndex = 0;
        private List<String> mImageNameList;
        private bool mGameOver;

        private GameLogic()
        {
            mImageTargetList = new List<GameObject>();
            mImageTarget2ImageIndexDict = new Dictionary<GameObject, int>();
            mHasInit = false;
            mGameOver = false;
        }

        public void AddImageTarget(GameObject obj)
        {
            if (!obj.name.Contains("ImageTarget"))
            {
                return;
            }
            mImageTargetList.Add(obj);
            if (mImageTargetList.Count >= mCellRowNum * mCellColNum)
            {
                if (!mHasInit)
                {
                    OnInitGame();
                }
                else
                {
                    mUpdateActList += CheckGameOver;
                }
            }
        }

        public void RemoveImageTarget(GameObject obj)
        {
            if (!obj.name.Contains("ImageTarget"))
            {
                return;
            }
            mImageTargetList.Remove(obj);
            if (mHasInit)
            {
                mUpdateActList -= CheckGameOver;
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

                //move blank to bottom-right
                while (rowIndex != mCellColNum - 1)
                {
                    rowIndex += 1;
                    newBlankIndex = rowIndex * mCellRowNum + colIndex;
                    temp = imageIndexList[newBlankIndex];
                    imageIndexList[newBlankIndex] = imageIndexList[blankIndex];
                    imageIndexList[blankIndex] = temp;
                    blankIndex = newBlankIndex;
                }
                while (colIndex != mCellRowNum - 1)
                {
                    colIndex += 1;
                    newBlankIndex = rowIndex * mCellRowNum + colIndex;
                    temp = imageIndexList[newBlankIndex];
                    imageIndexList[newBlankIndex] = imageIndexList[blankIndex];
                    imageIndexList[blankIndex] = temp;
                    blankIndex = newBlankIndex;
                }

                //check unmatched num
                unMatchedNum = 0;
                for (int i = 0; i < imageIndexList.Count; i++)
                {
                    if (i != imageIndexList[i])
                    {
                        unMatchedNum++;
                    }
                }
                if (unMatchedNum >= needUnMatchedNum)
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
                for (int i = 0; i < imageIndexList.Count; i++)
                {
                    imageIndexStr += (imageIndexList[i] + ", ");
                }
                Debug.Log(String.Format("imageIndexList: {0}", imageIndexStr));

                mImageTarget2ImageIndexDict.Clear();
                var currentTexList = mTextureDict[GetCurrentImageName()];
                for (int i = 0; i < mImageTargetList.Count; i++)
                {
                    int imageIndex = imageIndexList[i];
                    mImageTarget2ImageIndexDict[mImageTargetList[i]] = imageIndex;

                    //set corresponding texture
                    GameObject cellObj = Utility.FindGameObject(mImageTargetList[i], "cell");
                    Texture2D texture = null;
                    if (imageIndex != 8)
                    {
                        //imageIndex = 8 make it blank
                        texture = currentTexList[imageIndex];
                    }
                    Renderer rend = cellObj.GetComponent<Renderer>();
                    rend.material.mainTexture = texture;
                }

                mHasInit = true;
                mGameOver = false;
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
            for (int i = 0; i < mImageTargetList.Count; i++)
            {
                int rowIndex = i / mCellColNum;
                int colIndex = i % mCellColNum;
                bool hasLeft = rowIndex - 1 >= 0;
                bool hasRight = rowIndex + 1 <= mCellColNum - 1;
                bool hasUp = colIndex - 1 >= 0;
                bool hasDown = colIndex + 1 <= mCellRowNum - 1;
                int neighborIndex1, neighborIndex2;
                if (hasLeft && hasUp)
                {
                    neighborIndex1 = (rowIndex - 1) * mCellRowNum + colIndex;
                    neighborIndex2 = rowIndex * mCellRowNum + (colIndex - 1);
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.y);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.y);
                    if (Mathf.Abs(Vector2.Dot(v1, v2)) > threshold)
                    {
                        //considered as not vertical
                        return false;
                    }
                }
                if (hasLeft && hasDown)
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
                if (hasRight && hasUp)
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
                if (hasRight && hasDown)
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
            for (int i = 0; i < mImageTargetList.Count; i++)
            {
                if (i != mImageTarget2ImageIndexDict[mImageTargetList[i]])
                {
                    return false;
                }
            }
            return true;
        }

        private void CheckGameOver()
        {
            Debug.Log(String.Format("CheckGameOver, {0}", mGameOver));
            if (mGameOver)
            {
                return;
            }
            SortImageTargetList();
            if (CheckCellRectangle())
            {
                Debug.Log("CheckCellRectangle!!!");
                //check
                if (CheckIsAllMatch())
                {
                    OnGameOver();
                }
            }
        }

        public void Update()
        {
            mUpdateActList?.Invoke();
        }

        public void Init(List<String> imageNameList)
        {
            //todo if it has more than one parameters, using dict 'cfg' to pass these parameters.
            mImageNameList = imageNameList;
            CreateAllImages();
            //Test();
            GameObject.Find("TargetMenu").GetComponent<ImgButtonScript>().InitImage();
        }

        private void Test()
        {
            for (int i = 0; i < 9; i++)
            {
                var cellObj = GameObject.Find(String.Format("cell{0}", i + 1));
                if (cellObj != null)
                {
                    Renderer rend = cellObj.GetComponent<Renderer>();
                    var texture = mTextureDict["sunflower"][i];
                    rend.material.mainTexture = texture;
                }
            }
        }
        private void CreateAllImages()
        {
            mTextureDict = new Dictionary<string, List<Texture2D>>();
            for (int i = 0; i < mImageNameList.Count; i++)
            {
                var imageName = mImageNameList[i];
                var originalTexture = UnityEngine.Resources.Load(String.Format("Images/{0}", imageName)) as Texture2D;
                //split the original texture into 9 parts
                var textureList = new List<Texture2D>();
                int maxLen = Mathf.Max(originalTexture.width, originalTexture.height);//make it become a square image
                int minWidth = (maxLen - originalTexture.width) / 2;
                int maxWidth = (maxLen + originalTexture.width) / 2;
                int minHeight = (maxLen - originalTexture.height) / 2;
                int maxHeight = (maxLen + originalTexture.height) / 2;
                for (int num = 0; num < 9; num++)
                {
                    var newTexture = new Texture2D(maxLen / 3, maxLen / 3, TextureFormat.ARGB32, false);
                    for (int row = 0; row < maxLen / 3; row++)
                    {
                        for (int col = 0; col < maxLen / 3; col++)
                        {
                            //row,col starting from left-top
                            int pixelRowIdx = row + (num % 3) * maxLen / 3;
                            int pixelColIdx = col + (num / 3) * maxLen / 3;
                            Color color = Color.white;
                            //if row is inside [maxlen/2-width/2, maxlen/2+width/2), and col is inside [maxlen/2-height/2, maxlen/2+height/2)
                            //take picture's color, otherwise, take white
                            if (pixelRowIdx >= minWidth && pixelRowIdx < maxWidth && pixelColIdx >= minHeight && pixelColIdx < maxHeight)
                            {
                                //if row and col is [0, maxLen], and map the whole image
                                //color = originalTexture.GetPixel(maxWidth - minWidth - (row - minWidth) - 1, maxHeight - minHeight - (col - minHeight) - 1);

                                color = originalTexture.GetPixel(-maxWidth + pixelRowIdx + 1, maxHeight - pixelColIdx - 1);
                                //color = originalTexture.GetPixel(pixelRowIdx - minWidth, pixelColIdx - maxHeight);
                            }
                            newTexture.SetPixel(maxLen / 3 - row + 1, col, color);
                            //newTexture.SetPixel(row, col, color);
                        }
                    }
                    newTexture.Apply();
                    textureList.Add(newTexture);
                }
                mTextureDict.Add(imageName, textureList);
            }
        }

        public void OnChangeImage()
        {
            mCurrentImgIndex = (mCurrentImgIndex + 1) % mImageNameList.Count;//choose next image
            // restart game 
            // consider two situation,
            // 1) 9 markers are detecting
            // 2) not all markers are inside camera(be detected)
            if (mImageTargetList.Count >= mCellRowNum * mCellColNum)
            {
                OnInitGame();
            }
            else
            {
                mHasInit = false;//
                mGameOver = true;//wait for init
            }
        }

        private void OnGameOver()
        {
            mGameOver = true;

            //display victory screen
            GameObject st = GameObject.Find("SolutionTarget");
            GameObject congrats = (st.transform.Find("MountParent").gameObject).transform.GetChild(0).gameObject;
            congrats.SetActive(true);

            GameObject victoryUI = GameObject.Find("Canvas");
            victoryUI.SetActive(true);

            Debug.Log("Congratuations!!!");
        }

        public String GetCurrentImageName()
        {
            return mImageNameList[mCurrentImgIndex];
        }
    }
}
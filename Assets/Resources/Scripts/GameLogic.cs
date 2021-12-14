using System;
using System.Collections;
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
        private int mBlankMarkerIndex;//from 1 to 9, be corresponding to ImageTarget in scene. This index should be the index of background's marker.
        private int delayID;
        private List<ValueTuple<int, float, Action>> delayList;//delayID, delayTime, delayCallBack
        private int delayID_NextGame;//auto start next game's delayID
        private List<String> solution;

        private GameLogic()
        {
            mImageTargetList = new List<GameObject>();
            mImageTarget2ImageIndexDict = new Dictionary<GameObject, int>();
            mHasInit = false;
            mGameOver = false;
            mUpdateActList += CheckGameOver;
            mUpdateActList += UpdateDelayFunc;
            delayID = 0;
            delayList = new List<(int, float, Action)>();
            delayID_NextGame = -1;
            solution = new List<string>();
        }

        public void AddImageTarget(GameObject obj)
        {
            if (!obj.name.Contains("ImageTarget"))
            {
                return;
            }
            mImageTargetList.Add(obj);

            //idk why on the frame when all ImageTarget get detected, the ImageTarget's position is wrong!!! Maybe it's not be initialized?
            //For fixing this issue, delay execute OnInitGame() in next frame.
            AddDelayFunc(0, () => {
                if (mImageTargetList.Count >= mCellRowNum * mCellColNum)
                {
                    if (!mHasInit)
                    {
                        OnInitGame();
                    }
                }
            });
        }

        public void RemoveImageTarget(GameObject obj)
        {
            if (!obj.name.Contains("ImageTarget"))
            {
                return;
            }
            mImageTargetList.Remove(obj);
            //if (mHasInit)
            //{
            //    mUpdateActList -= CheckGameOver;
            //}
        }

        /// <summary>
        /// We have got the sequence from Imagetarget, and we always consider one specific marker(which is used to be the background) as blank
        /// Then we start from 0-8 index. let's consider index=8 as a blank. We move this index=8 randomly for many steps. It will make other index disorder.
        /// Then we set these index to ImageTarget. If we can arrange ImageTarget in a correct way so that all index is from 0-8, then means all match.
        /// </summary>
        /// <param name="imageIndexList"></param>
        /// <param name="blankIndex"></param>
        /// <param name="needUnMatchedNum"></param>
        private void Randomize(ref List<int> imageIndexList, int blankIndex, int needUnMatchedNum)
        {
            solution.Clear();
            List<int> movableDirList = new List<int>();//up,down,left,right  0, 1, 2, 3 
            int currentBlankIndex = imageIndexList.Count - 1;//set the last cell as blank, as default!!
            int rowIndex = currentBlankIndex / mCellColNum;//blank current rowIndex
            int colIndex = currentBlankIndex % mCellColNum;//blank current colIndex

            //blank dest position(here is for connect the blank position with the specific marker!!)
            int destRowIndex = blankIndex / mCellColNum;
            int destColIndex = blankIndex % mCellColNum;
            
            int dir;
            System.Random random = new System.Random();
            int unMatchedNum;
            int stepNum = 0;
            int newBlankIndex = -1;
            int temp = -1;

            Debug.Log(String.Format("Randomize, blankIndex:{0}, rowIndex:{1}, colIndex:{2}", blankIndex, destRowIndex, destColIndex));

            for (int iter = 0; iter < 100; iter++)
            {
                //move randomly
                while (stepNum < 100)
                {
                    movableDirList.Clear();
                    if (rowIndex - 1 >= 0)
                    {
                        movableDirList.Add(0);//blank move up
                    }
                    if (rowIndex + 1 <= mCellRowNum - 1)
                    {
                        movableDirList.Add(1);//blank move down
                    }
                    if (colIndex - 1 >= 0)
                    {
                        movableDirList.Add(2);//blank move left 
                    }
                    if (colIndex + 1 <= mCellColNum - 1)
                    {
                        movableDirList.Add(3);//blank move right
                    }
                    dir = movableDirList[random.Next(movableDirList.Count)];
                    switch (dir)
                    {
                        case 0:
                            rowIndex -= 1;
                            solution.Add("down");
                            break;
                        case 1:
                            rowIndex += 1;
                            solution.Add("up");
                            break;
                        case 2:
                            colIndex -= 1;
                            solution.Add("right");
                            break;
                        case 3:
                            colIndex += 1;
                            solution.Add("left");
                            break;
                    }
                    newBlankIndex = rowIndex * mCellRowNum + colIndex;
                    temp = imageIndexList[newBlankIndex];
                    imageIndexList[newBlankIndex] = imageIndexList[currentBlankIndex];
                    imageIndexList[currentBlankIndex] = temp;
                    currentBlankIndex = newBlankIndex;

                    stepNum++;
                }

                //move blank to dest blankIndex position
                while (rowIndex != destRowIndex)
                {
                    rowIndex = rowIndex < destRowIndex ? rowIndex + 1 : rowIndex - 1;
                    newBlankIndex = rowIndex * mCellRowNum + colIndex;
                    temp = imageIndexList[newBlankIndex];
                    imageIndexList[newBlankIndex] = imageIndexList[currentBlankIndex];
                    imageIndexList[currentBlankIndex] = temp;
                    currentBlankIndex = newBlankIndex;
                    solution.Add(rowIndex < destRowIndex ? "up" : "down");
                }
                while (colIndex != destColIndex)
                {
                    colIndex = colIndex < destColIndex ? colIndex + 1 : colIndex - 1;
                    newBlankIndex = rowIndex * mCellRowNum + colIndex;
                    temp = imageIndexList[newBlankIndex];
                    imageIndexList[newBlankIndex] = imageIndexList[currentBlankIndex];
                    imageIndexList[currentBlankIndex] = temp;
                    currentBlankIndex = newBlankIndex;
                    solution.Add(rowIndex < destRowIndex ? "left" : "right");
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
            int row = 0;
            mImageTargetList.Sort((obj1, obj2) => { return obj1.transform.position.z > obj2.transform.position.z ? -1 : 1; });
            while (row < mCellRowNum)
            {
                tempList.Clear();
                //int cnt = Mathf.Min(mCellColNum, mImageTargetList.Count);
                tempList.AddRange(mImageTargetList.GetRange(0, mCellColNum));
                mImageTargetList.RemoveRange(0, mCellColNum);
                tempList.Sort((obj1, obj2) => { return obj1.transform.position.x < obj2.transform.position.x ? -1 : 1; });
                sortedList.AddRange(tempList);
                row++;
            }
            mImageTargetList.AddRange(sortedList);

            // for debug test
            string imageIndexStr = "";
            for (int i = 0; i < mImageTargetList.Count; i++)
            {
                if (mImageTarget2ImageIndexDict.Count <= 0)
                {
                    imageIndexStr += (mImageTargetList[i].name + ", ");
                }
                else
                {
                    var idx = mImageTarget2ImageIndexDict[mImageTargetList[i]];
                    imageIndexStr += (String.Format("{0} - index{1}", mImageTargetList[i].name, idx) + ", ");
                }
            }
            Debug.Log(String.Format("SortImageTargetList: {0}", imageIndexStr));
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

                //find blankIndex (which is decided by mBlankMarkerIndex, position
                int blankIndex = -1;
                for (int i = 0; i < mImageTargetList.Count; i++)
                {
                    if (mImageTargetList[i].name == String.Format("ImageTarget{0}", mBlankMarkerIndex))
                    {
                        blankIndex = i;
                        break;
                    }
                }
                if (blankIndex == -1)
                {
                    Debug.LogError(String.Format("blankIndex: {0}", blankIndex));
                }

                //randomize
                Randomize(ref imageIndexList, blankIndex, mCellRowNum * mCellColNum / 2 + 1);

                //show solution
                string solutionStr = "";
                for(int i = solution.Count-1; i >= 0; i--)
                {
                    solutionStr += (solution[i] + ", ");
                }
                Debug.Log(String.Format("Solution: {0}", solutionStr));

                //just for test
                string imageIndexStr = "";
                for (int i = 0; i < imageIndexList.Count; i++)
                {
                    imageIndexStr += (imageIndexList[i] + ", ");
                }
                Debug.Log(String.Format("Randomize, imageIndexList: {0}", imageIndexStr));

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
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.z);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.z);
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
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.z);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.z);
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
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.z);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.z);
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
                    var v1 = new Vector2(mImageTargetList[neighborIndex1].transform.position.x, mImageTargetList[neighborIndex1].transform.position.z);
                    var v2 = new Vector2(mImageTargetList[neighborIndex2].transform.position.x, mImageTargetList[neighborIndex2].transform.position.z);
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
            if (!mHasInit)
            {
                return;
            }
            if (mImageTargetList.Count >= mCellRowNum * mCellColNum)
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
        }

        public void Update()
        {
            mUpdateActList?.Invoke();
        }

        public void Init(List<String> imageNameList, int blankMarkerIndex)
        {
            //todo if it has more than one parameters, using dict 'cfg' to pass these parameters.
            mImageNameList = imageNameList;
            mBlankMarkerIndex = blankMarkerIndex;
            CreateAllImages();
            //Test();
            GameObject.Find("TargetMenu").GetComponent<ImgButtonScript>().SetCurrentImage();
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

        private void OnChangeImage()
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
            GameObject.Find("TargetMenu").GetComponent<ImgButtonScript>().SetCurrentImage();
        }

        private void SetVictoryUIVisible(bool visible)
        {
            GameObject vicUI = GameObject.Find("VictoryUI");
            GameObject image = vicUI.transform.GetChild(2).gameObject; //Image2
            image.SetActive(visible);
        }

        private void OnGameOver()
        {
            mGameOver = true;

            SetVictoryUIVisible(true);

            delayID_NextGame = AddDelayFunc(5, () => {
                delayID_NextGame = -1;
                OnNextGame();
            });//5 seconds hide

            Debug.Log("Congratuations!!!");
        }

        public String GetCurrentImageName()
        {
            return mImageNameList[mCurrentImgIndex];
        }

        public void AddUpdateFunction(Action cb)
        {
            if (cb != null)
            {
                mUpdateActList += cb;
            }
        }

        public void RemoveUpdateFunction(Action cb)
        {
            if (cb != null)
            {
                mUpdateActList -= cb;
            }
        }

        /// <summary>
        /// Add delay function, delay 'time' seconds then call 'cb'
        /// </summary>
        /// <param name="time">seconds</param>
        /// <param name="cb">callback</param>
        /// <returns></returns>
        public int AddDelayFunc(float time, Action cb)
        {
            int id = delayID;
            delayList.Add(new ValueTuple<int, float, Action>(id, time, cb));
            delayID++;
            return id;
        }

        public void RemoveDelayFunc(int delayID)
        {
            for (int i = 0; i < delayList.Count; i++)
            {
                if (delayList[i].Item1 == delayID)
                {
                    delayList.RemoveAt(i);
                    break;
                }
            }
        }

        private void UpdateDelayFunc()
        {
            float delta = Time.deltaTime;
            for (int i = delayList.Count - 1; i >= 0; i--)
            {
                delayList[i] = new ValueTuple<int, float, Action>(delayList[i].Item1, delayList[i].Item2 - delta, delayList[i].Item3);
                if (delayList[i].Item2 <= 0)
                {
                    delayList[i].Item3?.Invoke();
                    delayList.RemoveAt(i);
                }
            }
        }

        public void OnNextGame()
        {
            if (delayID_NextGame != -1)
            {
                RemoveDelayFunc(delayID_NextGame);
                delayID_NextGame = -1;
            }

            SetVictoryUIVisible(false);

            OnChangeImage();
        }
    }
}
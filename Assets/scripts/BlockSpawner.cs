using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BlockSpawner : MonoBehaviour
{
    private const int Width = 10;
    private const int Height = 20;
    private int _blockCounter = 0;
    private const int Undefined = -1;
    private const int CreateBomb = 5;
    
    public GameObject[] blocks;
    
    private static readonly Transform[,] Grid = new Transform[Width, Height];

    private IEnumerator _coroutine;

    private int _score;

    private const int NumberOfColoredBricks = 4;
    
    public bool gameOver = false;

    public GameObject gameOverText;

    private bool _crRunning = false;
    
    void Start()
    {
        FillContainer();
        _score = 0;
    }

    private void FillContainer()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var BRICK_WIDTH = 0.5;
                var BRICK_HEIGHT = 0.5;
                
                // it's actually 0.485 but we give a little gap.
                
                Vector3 position = transform.position + new Vector3((float)(i * BRICK_WIDTH), (float)(j * BRICK_HEIGHT), 0);
                var newBlock = Instantiate(blocks[Random.Range(0, NumberOfColoredBricks)], position, Quaternion.identity);
                newBlock.name = _blockCounter++.ToString();
                Grid[i, j] = newBlock.transform;
            }
        }
    }
    
    public bool checkGrid(Dictionary<int,int> dictionary)
    {
        int counter = 0;
        for(int i=0;i< Width; i++)
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i, j],null))
                {
                    counter++;
                    if (dictionary.ContainsKey(i))
                    {
                        dictionary[i] += 1;
                    }
                    else
                    {
                        dictionary.Add(i, 1);
                    }
                }
            }

        //Bomb creating also triggers so...
        return counter < 2;
    }

    private void Update()
    {
        var dictionary = new Dictionary<int, int>();
        bool gridFull = true;
        if (!_crRunning)
        {
            gridFull = checkGrid(dictionary);
        }
        if (!gridFull && !_crRunning )
        {
            _crRunning = true;
            _coroutine = FallElementsDown(dictionary);
            StartCoroutine(_coroutine);
        }

    }

    
    
    private void Logic(Transform clickedObject)
    {
        int clickedBlockX = Undefined, clickedBlockY = Undefined;

        GetClickedGrid(ref clickedBlockX, ref clickedBlockY, clickedObject);

        if (clickedBlockX != Undefined)
        {
            string clickedColor = Grid[clickedBlockX, clickedBlockY].tag;
            List<Point> elementsToBeTraversed = new List<Point>();
            List<Point> elementsToBeDeleted = new List<Point>();
            elementsToBeTraversed.Add(new Point(clickedBlockX, clickedBlockY));
            elementsToBeDeleted.Add(new Point(clickedBlockX, clickedBlockY));

            Dictionary<int, int> dictionary = new Dictionary<int, int>();
            dictionary.Add(clickedBlockX, 1);
            
            TraverseNew(elementsToBeTraversed, clickedColor, elementsToBeDeleted, dictionary);

            if(elementsToBeDeleted.Count < 2) return;
            
            movesController moves = GameObject.Find("moves").GetComponent<movesController>();
            moves.reduceMovesByOne();
            
            addScore(elementsToBeDeleted.Count);
            
            DeleteElements(elementsToBeDeleted, shouldBombBeCreated(elementsToBeDeleted), dictionary, false);
            
        }
    }

    private bool shouldBombBeCreated(List<Point> ElementsToBeDeleted)
    {
        return ElementsToBeDeleted.Count > CreateBomb;
    }

    private void bringNewBricks(Dictionary<int, int> dictionary)
    {
        List<int> mylist = new List<int>();
        for (int i = 0; i < dictionary.Count; i++)
        {
            var item = dictionary.ElementAt(i);
            if (item.Value != 0)
            {
                mylist.Add(item.Key);
                dictionary[item.Key] -= 1;
            }
        }
        createColumns(mylist);
        
    }
    private void createColumns(List<int> mc)
    {
        
        for(int i = 0; i < mc.Count ; i++)
        {
            UnityEngine.Object newBlock = Instantiate(blocks[Random.Range(0, 4)], new Vector3((float) (transform.position.x + mc[i]*0.5),(float)4.75,0), Quaternion.identity);
            GameObject gameObjectBlock = (GameObject)newBlock;
            newBlock.name = "" + _blockCounter++;
            Grid[mc[i], 19] = gameObjectBlock.transform;
        }
    }

    private void DeleteElements(List<Point> elementsToBeDeleted,bool createBomb,Dictionary<int,int> dictionary, bool fromBomb)
    {
        
        foreach (Point point in elementsToBeDeleted)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            if (createBomb)
            {
                pos = Grid[point.GetX(), point.GetY()].gameObject.transform.position;
                Destroy(Grid[point.GetX(), point.GetY()].gameObject);
                UnityEngine.Object newBlock = Instantiate(blocks[4], pos, Quaternion.identity);
                GameObject bombobj = (GameObject) newBlock;
                bombobj.name = "" + _blockCounter++;
                Grid[point.GetX(), point.GetY()] = bombobj.transform;
                createBomb = false;
                dictionary[point.GetX()] -= 1;
            }
            else
            {
                string myBrick = Grid[point.GetX(), point.GetY()].gameObject.name;
                BombAndBrick mmybrick = GameObject.Find(myBrick).GetComponent<BombAndBrick>();
                mmybrick.trigger(point.GetX(), point.GetY(), dictionary, fromBomb);             
            }
        }
        
    }

    public void deleteFromGrid(int x, int y, Dictionary<int,int> dictionary)
    {
        if(x != -1)
            Grid[x, y] = null;
    }
    
    private void GetClickedGrid(ref int x, ref int y, Transform clickedObject)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i, j], null))
                {
                    continue;
                }
                if (Grid[i, j].Equals(clickedObject))
                {
                    x = i;
                    y = j;
                    break;
                }
            }
        }
    }

    
    private void addScore(int count)
    {
        int addscore = (int)Math.Pow(count, 2);
        _score += addscore;
        Text text = GameObject.Find("score").GetComponent<Text>();
        text.text = "" + _score;
    }

    private IEnumerator FallElementsDown(Dictionary<int,int> dictionary)
    { 
        
        bool stillFalling = true;
        bool allfilled = false;
        while (true)
        {
            stillFalling = true;
            allfilled = true;
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary.ElementAt(i).Value != 0)
                {
                    allfilled = false;
                    break;
                }
            }

            if (allfilled)
            {
                break;
            }

            while (stillFalling)
            {
                yield return new WaitForSeconds(0.05f);
                stillFalling = false;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (ReferenceEquals(Grid[x, y], null))
                        {
                            for (int indexY = y + 1; indexY < Height; indexY++)
                            {
                                if (!ReferenceEquals(Grid[x, indexY], null))
                                {
                                    stillFalling = true;
                                    Grid[x, indexY - 1] = Grid[x, indexY];
                                    Vector2 vector = Grid[x, indexY - 1].transform.position;
                                    vector.y -= 0.5f;
                                    Grid[x, indexY - 1].transform.position = vector;
                                    Grid[x, indexY] = null;
                                }
                            }
                        }
                    }
                }
            }

            bringNewBricks(dictionary);
        }
        _crRunning = false;
    }

    public void getClickedBrick(Transform transform)
    {
        if (!_crRunning && !gameOver)
        {
            Logic(transform);
        }
    }
    
    void TraverseNew(List<Point> elementsToBeTraversed, string color, List<Point> elementsToBeDeleted, Dictionary<int,int> dictionary)
    {
        while (elementsToBeTraversed.Count > 0)
        {
            int curX = elementsToBeTraversed[0].GetX();
            int curY = elementsToBeTraversed[0].GetY();
            CheckElement(curX - 1, curY);
            CheckElement(curX + 1, curY);
            CheckElement(curX, curY + 1);
            CheckElement(curX, curY - 1);

            elementsToBeTraversed.Remove(elementsToBeTraversed[0]);
        }
        void CheckElement(int x, int y)
        {
            if (x > -1 && x < Width && y > -1 && y < Height)
            {
                if (!ReferenceEquals(Grid[x, y], null) && Grid[x, y].tag.Equals(color))
                {
                    Point newCur = new Point(x, y);
                    if (!elementsToBeDeleted.Contains(newCur) && !elementsToBeTraversed.Contains(newCur))
                    {
                        if (dictionary.ContainsKey(newCur.GetX()))
                        {
                            dictionary[newCur.GetX()] += 1;
                        }
                        else
                        {
                            dictionary.Add(newCur.GetX(), 1);
                        }
                        elementsToBeDeleted.Add(newCur);
                        elementsToBeTraversed.Add(newCur);
                    }
                }
            }
        }
    }

    public void getBombedBrick(Transform gameObjectTransform)
    {
        if(!_crRunning && !gameOver)
            //Destroy(gameObjectTransform.gameObject);    
            bombIt( gameObjectTransform);
    }

    public void bombIt(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();
        
        var dictionary = new Dictionary<int, int>();

        var ListBomb = new List<Point>();
        
        ListBomb.Add(new Point(x,y));

        findBombedElements(ListBomb, dictionary,elementsToDelete);
        
        DeleteElements(elementsToDelete,false, dictionary, true);
        addScore(elementsToDelete.Count);
        
        
    }

    private void findBombedElements(List<Point> listBomb,Dictionary<int,int> dictionary,List<Point> elementsToDelete)
    {
        var deletedBombs = new List<Point>();
        while (listBomb.Count > 0)
        {
            for(int i = listBomb[0].GetX()-1; i<= listBomb[0].GetX()+1; i++)
            {
                for (int j = listBomb[0].GetY() - 1; j <= listBomb[0].GetY() + 1; j++)
                {
                    if(i < 0 || i >=Width || j<0 || j>=Height)
                        continue;
                    addElement(i, j, elementsToDelete, dictionary);
                    Point point = new Point(i, j);
                    if (Grid[i,j].transform.gameObject.tag.Equals("bomb") && !listBomb.Contains(point) && !deletedBombs.Contains(point))
                    {
                        listBomb.Add(point);
                    }
                } 
            }
            if (listBomb.Count > 0)
            {
                var deletePoint = listBomb[0];
                listBomb.RemoveAt(0);
                deletedBombs.Add(deletePoint);
            }
            
        }
    }

    void addElement(int x, int y, List<Point> deleteList,Dictionary<int, int> dictionary)
    {
        if (x > -1 && x < Width && y > -1 && y < Height)
        {
            Point toAdd = new Point(x,y);
            if (!ReferenceEquals(Grid[x, y], null) && !deleteList.Contains(toAdd))
            {
                deleteList.Add(toAdd);
                if (dictionary.ContainsKey(x))
                {
                    dictionary[x] += 1;
                }
                else
                {
                    dictionary.Add(x, 1);
                }
            }
        }
    }

    public void SetGameOver(bool gameOver)
    {
        this.gameOver = true;
        GameObject myObj = Instantiate(gameOverText,new Vector3(0,0,0) ,Quaternion.identity) as GameObject;
        myObj.transform.SetParent(GameObject.FindWithTag("canvas").transform, false);
    }
    
}

struct Point
{
    int x;
    int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
}
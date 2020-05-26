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
    private const float BrickHeight = 0.5f;
    private const float BrickWidth = 0.5f;
    private const int Undefined = -1;
    private const int CreateBomb = 6;
    private const int CreateMissile = 4;
    private const int NumberOfColoredBricks = 4;
    private int move = 0;

    public GameObject[] blocks;

    private static readonly Transform[,] Grid = new Transform[Width, Height];

    private IEnumerator _coroutine;

    private int _score;
    private int _blockCounter = 0;

    private List<string> colorNames;

    public bool gameOver = false;

    public GameObject gameOverText;

    private bool _crRunning = false;
    void Start()
    {
        colorNames = new List<string>
        {
            "blue",
            "red",
            "green",
            "yellow"
        };
        FillContainer();
        _score = 0;
    }

    private void FillContainer()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var position = transform.position +
                               new Vector3((float) (i * BrickWidth), (float) (j * BrickHeight), 0);
                var newBlock = Instantiate(blocks[Random.Range(0, NumberOfColoredBricks)], position,
                    Quaternion.identity);
                newBlock.name = _blockCounter++.ToString();
                Grid[i, j] = newBlock.transform;
            }
        }
    }
    
    private void Update()
    {
        var dictionary = new Dictionary<int, int>();
        bool gridFull = true;
        if (!_crRunning)
        {
            gridFull = CheckGrid(dictionary);
        }

        if (!gridFull && !_crRunning)
        {
            visitedBomb = new List<Point>();
            _crRunning = true;
            _coroutine = FallElementsDown(dictionary);
            StartCoroutine(_coroutine);
        }
    }

    private bool CheckGrid(Dictionary<int, int> dictionary)
    {
        int counter = 0;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i, j], null))
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
        }
        //Bomb creating also triggers so...
        return counter < 2;
    }
    
    private IEnumerator FallElementsDown(Dictionary<int, int> dictionary)
    {
        while (true)
        {
            bool stillFalling = true;
            bool allfilled = true;
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
                yield return new WaitForSeconds(0.01f);
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
                                    vector.y -= BrickHeight;
                                    Grid[x, indexY - 1].transform.position = vector;
                                    Grid[x, indexY] = null;
                                }
                            }
                        }
                    }
                }
            }

            BringNewBricks(dictionary);
        }

        _crRunning = false;
    }

    
    public void GetClickedBrick(Transform clickedTransform)
    {
        if (!_crRunning && !gameOver)
        {
            FindAndDeleteElements(clickedTransform);
        }
    }

    private void makeRandomBricksCorrupted()
    {
        int corruptedSelected = 0;
        while(corruptedSelected < 5)
        {
            int randomHeight = Random.Range(0, Height);
            int randomWidth = Random.Range(0, Width);
            if (colorNames.Contains(Grid[randomWidth, randomHeight].gameObject.tag))
            {
                corruptedSelected++;
                var vector = Grid[randomWidth, randomHeight].position;
                Destroy(Grid[randomWidth, randomHeight].gameObject);
                var corruptedObject = Instantiate(blocks[7], vector, Quaternion.identity);
                Grid[randomWidth, randomHeight] = corruptedObject.transform;
            }
        }
        
    }

    private void FindAndDeleteElements(Transform clickedObject)
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

            Dictionary<int, int> missingBricksAtColumns = new Dictionary<int, int>();
            missingBricksAtColumns.Add(clickedBlockX, 1);

            TraverseNew(elementsToBeTraversed, clickedColor, elementsToBeDeleted, missingBricksAtColumns);

            if (elementsToBeDeleted.Count < 2) return;

            //move done
            move++;

           

            AddScore(elementsToBeDeleted.Count);

            DeleteElements(elementsToBeDeleted, ShouldMissileBeCreated(elementsToBeDeleted), missingBricksAtColumns, false);

            if (move % 5 == 0)
                makeRandomBricksCorrupted();
        }
    }

    private bool shouldBombBeCreated(List<Point> ElementsToBeDeleted)
    {
        return ElementsToBeDeleted.Count > CreateBomb;
    }

    private bool ShouldMissileBeCreated(List<Point> ElementsToBeDeleted)
    {
        return ElementsToBeDeleted.Count > CreateMissile;
    }

    private void BringNewBricks(Dictionary<int, int> dictionary)
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

        CreateColumns(mylist);

    }

    private void CreateColumns(List<int> mc)
    {

        for (int i = 0; i < mc.Count; i++)
        {
            UnityEngine.Object newBlock = Instantiate(blocks[Random.Range(0, 4)],
                new Vector3((float) (transform.position.x + mc[i] * BrickWidth), (float) (transform.position.y + (Height-1)*BrickHeight), 0), Quaternion.identity);
            GameObject gameObjectBlock = (GameObject) newBlock;
            newBlock.name = "" + _blockCounter++;
            Grid[mc[i], Height-1] = gameObjectBlock.transform;
        }
    }

    private void DeleteElements(List<Point> elementsToBeDeleted, bool createBomb, Dictionary<int, int> dictionary,
        bool fromBomb)
    {

        foreach (Point point in elementsToBeDeleted)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            if (createBomb)
            {
                createBomb = false;


                pos = Grid[point.GetX(), point.GetY()].gameObject.transform.position;
                Destroy(Grid[point.GetX(), point.GetY()].gameObject);
                GameObject bombObject;
                bombObject = elementsToBeDeleted.Count > CreateBomb
                    ? Instantiate(blocks[4], pos, Quaternion.identity)
                    : Instantiate(blocks[Random.Range(5,7)], pos, Quaternion.identity);
                bombObject.name = _blockCounter++.ToString();
                Grid[point.GetX(), point.GetY()] = bombObject.transform;
                dictionary[point.GetX()] -= 1;
            }
            else
            {
                string brickId = Grid[point.GetX(), point.GetY()].gameObject.name;
                var bombOrBrick = GameObject.Find(brickId).GetComponent<BombAndBrick>();
                bombOrBrick.trigger(point.GetX(), point.GetY(), dictionary);
            }
        }

    }

    public void DeleteFromGrid(int x, int y, Dictionary<int, int> dictionary)
    {
        if (x != -1)
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


    private void AddScore(int count)
    {
        int addscore = (int) Math.Pow(count, 2);
        _score += addscore;
        Text text = GameObject.Find("score").GetComponent<Text>();
        text.text = "" + _score;
    }
    
    void TraverseNew(List<Point> elementsToBeTraversed, string color, List<Point> elementsToBeDeleted,
        Dictionary<int, int> dictionary)
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
        if (!_crRunning && !gameOver)
            //Destroy(gameObjectTransform.gameObject);    
            bombIt(gameObjectTransform);
    }

    public void getMissiledBrick(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
        {
            //Destroy(gameObjectTransform.gameObject);    
            missileIt(gameObjectTransform);
        }

    }

    public void getMissiledBrickUpside(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
        {
            //Destroy(gameObjectTransform.gameObject);    
            missileItUpside(gameObjectTransform);
        }

    }

    public void missileIt(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();

        var dictionary = new Dictionary<int, int>();

        var ListBomb = new List<Point>();

        ListBomb.Add(new Point(x, y));

        findMissiledElements(ListBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary, true);
        AddScore(elementsToDelete.Count);
    }

    public void missileItUpside(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();

        var dictionary = new Dictionary<int, int>();

        var ListBomb = new List<Point>();

        ListBomb.Add(new Point(x, y));

        findMissiledElementsUpside(ListBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary, true);
        AddScore(elementsToDelete.Count);
    }

    public void bombIt(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();

        var dictionary = new Dictionary<int, int>();

        var ListBomb = new List<Point>();

        ListBomb.Add(new Point(x, y));

        findBombedElements(ListBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary, true);
        AddScore(elementsToDelete.Count);


    }

    private void findMissiledElements(List<Point> listBomb, Dictionary<int, int> dictionary,
        List<Point> elementsToDelete)
    {
        var y = listBomb[0].GetY();
        visitedBomb.Add(new Point(listBomb[0].GetX(),listBomb[0].GetY()));
        for (int i = 0; i < Width; i++)
        {
            if (!visitedBomb.Contains(new Point(i, y)))
            {
                if (Grid[i, y].transform.gameObject.tag.Equals("bomb"))
                {
                    String bombId = Grid[i, y].transform.gameObject.name;
                    Bomb bomb = GameObject.Find(bombId).GetComponent<Bomb>();
                    bomb.OnMouseDown();
                }

                if (Grid[i, y].transform.gameObject.tag.Equals("missile"))
                {
                    String bombId = Grid[i, y].transform.gameObject.name;
                    Missile bomb = GameObject.Find(bombId).GetComponent<Missile>();
                    bomb.OnMouseDown();
                }

                if (Grid[i, y].transform.gameObject.tag.Equals("upmissile"))
                {
                    String bombId = Grid[i, y].transform.gameObject.name;
                    MissileUpside bomb = GameObject.Find(bombId).GetComponent<MissileUpside>();
                    bomb.OnMouseDown();
                }
            }

            addElement(i, y, elementsToDelete, dictionary);
        }
        // var deletedBombs = new List<Point>();
        // while (listBomb.Count > 0)
        // {
        //     for(int i = listBomb[0].GetX()-1; i<= listBomb[0].GetX()+1; i++)
        //     {
        //         for (int j = listBomb[0].GetY() - 1; j <= listBomb[0].GetY() + 1; j++)
        //         {
        //             if(i < 0 || i >=Width || j<0 || j>=Height)
        //                 continue;
        //             addElement(i, j, elementsToDelete, dictionary);
        //             Point point = new Point(i, j);
        //             if (Grid[i,j].transform.gameObject.tag.Equals("bomb") && !listBomb.Contains(point) && !deletedBombs.Contains(point))
        //             {
        //                 listBomb.Add(point);
        //             }
        //         } 
        //     }
        //     if (listBomb.Count > 0)
        //     {
        //         var deletePoint = listBomb[0];
        //         listBomb.RemoveAt(0);
        //         deletedBombs.Add(deletePoint);
        //     }

        // }
    }
    private void findMissiledElementsUpside(List<Point> listBomb, Dictionary<int, int> dictionary, List<Point> elementsToDelete)
    {
        visitedBomb.Add(new Point(listBomb[0].GetX(),listBomb[0].GetY()));
        var x = listBomb[0].GetX();
        for (int i = 0; i < Height; i++)
        {
            if(!visitedBomb.Contains(new Point(x,i)))
            {
                if ((Grid[x, i].transform.gameObject.tag.Equals("bomb")))
                {
                    String bombId = Grid[x, i].transform.gameObject.name;
                    Bomb bomb = GameObject.Find(bombId).GetComponent<Bomb>();
                    bomb.OnMouseDown();
                }
                else if (Grid[x, i].transform.gameObject.tag.Equals("missile"))
                {
                    String bombId = Grid[x, i].transform.gameObject.name;
                    Missile bomb = GameObject.Find(bombId).GetComponent<Missile>();
                    bomb.OnMouseDown();
                }
                else if (Grid[x, i].transform.gameObject.tag.Equals("upmissile"))
                {
                    String bombId = Grid[x, i].transform.gameObject.name;
                    MissileUpside bomb = GameObject.Find(bombId).GetComponent<MissileUpside>();
                    bomb.OnMouseDown();
                }
                
            }
            addElement(x, i, elementsToDelete, dictionary);
        }
    }

private List<Point> visitedBomb = new List<Point>();    
    private void findBombedElements(List<Point> listBomb,Dictionary<int,int> dictionary,List<Point> elementsToDelete)
    {
        visitedBomb.Add(listBomb[0]);
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
                    if (!visitedBomb.Contains(point))
                    {
                        if (Grid[i, j].transform.gameObject.tag.Equals("bomb") && !listBomb.Contains(point) &&
                            !deletedBombs.Contains(point))
                        {
                            String bombId = Grid[i, j].transform.gameObject.name;
                            Bomb bomb = GameObject.Find(bombId).GetComponent<Bomb>();
                            bomb.OnMouseDown();

                            //listBomb.Add(point);
                        }
                        else if (Grid[i, j].transform.gameObject.tag.Equals("missile"))
                        {
                            String bombId = Grid[i, j].transform.gameObject.name;
                            Missile bomb = GameObject.Find(bombId).GetComponent<Missile>();
                            bomb.OnMouseDown();
                        }
                        else if (Grid[i, j].transform.gameObject.tag.Equals("upmissile"))
                        {
                            String bombId = Grid[i, j].transform.gameObject.name;
                            MissileUpside bomb = GameObject.Find(bombId).GetComponent<MissileUpside>();
                            bomb.OnMouseDown();
                        }
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
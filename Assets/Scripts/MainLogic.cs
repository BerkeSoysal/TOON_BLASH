using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MainLogic : MonoBehaviour
{
    private const int Width = 10; //total number of bricks column-wise
    private const int Height = 20; //total number of bricks row-wise
    private const float BrickHeight = 0.5f;
    private const float BrickWidth = 0.5f;
    private const int Undefined = -1;
    private const int CreateBomb = 6;
    private const int CreateMissile = 4;
    private const int CorruptBrickCreate = 4;
    private int _corruptBrickNum = 6;
    private int _move;

    public GameObject[] blocks;
    public GameObject[] missiles;
    public GameObject bomb;
    public GameObject corruptedBrick;

    private static readonly Transform[][] Grid = new Transform[Width][];
    private IEnumerator _fallElementsDownCoroutine;

    private int _score;
    private int _blockCounter;

    private List<string> _colorNames;

    public bool gameOver;

    public GameObject gameOverText;

    private bool _crRunning;

    void Start()
    {
        for (int i = 0; i < Grid.Length; i++)
        {
            Grid[i] = new Transform[Height];
        }
        
        _colorNames = new List<string>
        {
            "blue",
            "red",
            "green",
            "yellow"
        };
        FillContainer();
        _score = 0;
    }

    /**
     * Fills container with random generated bricks.
     */
    private void FillContainer()
    {
        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var position = transform.position + new Vector3(i * BrickWidth, j * BrickHeight, 0);
                var newBlock = Instantiate(blocks[Random.Range(0, blocks.Length)], position,
                    Quaternion.identity);
                newBlock.name = _blockCounter++.ToString();
                Grid[i][j] = newBlock.transform;
            }
        }
    }

    private bool _elementsAreCreated;
    private bool _changeHappen;

    private void Update()
    {
        var bricksToBeAdded = new Dictionary<int, int>();
        var gridFull = true;
        if (!_crRunning)
        {
            gridFull = CheckGridNotNeedNewBrick(bricksToBeAdded);
        }

        if (!gridFull && !_crRunning)
        {
            _elementsAreCreated = false;
            _visitedBomb = new List<Point>();
            _crRunning = true;
            _fallElementsDownCoroutine = FallElementsDown(bricksToBeAdded);
            StartCoroutine(_fallElementsDownCoroutine);
            _move++;
            _changeHappen = true;
        }

        if (_move % CorruptBrickCreate == 0 && _elementsAreCreated)
        {
            _elementsAreCreated = false;
            MakeRandomBricksCorrupted();
        }

        if (!_crRunning && _changeHappen)
        {
            gameOver = !CheckAvailableMove();
            _changeHappen = false;
        }

        if (gameOver)
        {
            EndGame();
        }
    }

    /**
     * Checks grids are do not need new bricks
     * Adds the missing bricks column information
     */
    private bool CheckGridNotNeedNewBrick(Dictionary<int, int> bricksToBeAdded)
    {
        int counter = 0;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i][j], null))
                {
                    counter++;
                    if (bricksToBeAdded.ContainsKey(i))
                    {
                        bricksToBeAdded[i] += 1;
                    }
                    else
                    {
                        bricksToBeAdded.Add(i, 1);
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
            bool allFilled = true;
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary.ElementAt(i).Value != 0)
                {
                    allFilled = false;
                    break;
                }
            }

            if (allFilled)
            {
                _elementsAreCreated = true;
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
                        if (!ReferenceEquals(Grid[x][y], null)) continue;
                        for (int indexY = y + 1; indexY < Height; indexY++)
                        {
                            if (!ReferenceEquals(Grid[x][indexY], null))
                            {
                                stillFalling = true;
                                Grid[x][indexY - 1] = Grid[x][indexY];
                                Vector2 vector = Grid[x][indexY - 1].transform.position;
                                vector.y -= BrickHeight;
                                Grid[x][indexY - 1].transform.position = vector;
                                Grid[x][indexY] = null;
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

    private bool NoColorFulLeft()
    {
        for (var i = 0; i < Width; i++)
        for (var j = 0; j < Height; j++)
        {
            if (_colorNames.Contains(Grid[i][j].transform.gameObject.tag))
                return false;
        }

        return true;
    }

    private void MakeRandomBricksCorrupted()
    {
        int corruptedSelected = 0;
        while (corruptedSelected < _corruptBrickNum)
        {
            if (NoColorFulLeft())
            {
                gameOver = true;
                break;
            }

            int randomHeight = Random.Range(0, Height);
            int randomWidth = Random.Range(0, Width);


            if (_colorNames.Contains(Grid[randomWidth][randomHeight].gameObject.tag))
            {
                corruptedSelected++;
                var vector = Grid[randomWidth][randomHeight].position;
                Destroy(Grid[randomWidth][randomHeight].gameObject);
                var corruptedObject = Instantiate(corruptedBrick, vector, Quaternion.identity);
                corruptedObject.name = "corrupted" + _blockCounter;
                _blockCounter++;
                Grid[randomWidth][randomHeight] = corruptedObject.transform;
            }
        }

        if (_corruptBrickNum < 20)
            _corruptBrickNum++;
    }

    /**
     * check if user can do a move
     */
    private Boolean CheckAvailableMove()
    {
        for (int i = 0; i < Width; i++)
        for (int j = 0; j < Height; j++)
        {
            string clickedColor = Grid[i][j].tag;
            if (clickedColor.Equals("bomb") || clickedColor.Equals("missile") || clickedColor.Equals("upmissile"))
            {
                return true;
            }

            if (clickedColor.Equals("corrupted"))
                continue;
            List<Point> elementsToBeTraversed = new List<Point>();
            List<Point> elementsToBeDeleted = new List<Point>();
            elementsToBeTraversed.Add(new Point(i, j));
            elementsToBeDeleted.Add(new Point(i, j));

            Dictionary<int, int> missingBricksAtColumns = new Dictionary<int, int>();
            missingBricksAtColumns.Add(i, 1);

            TraverseNew(elementsToBeTraversed, clickedColor, elementsToBeDeleted, missingBricksAtColumns);

            if (elementsToBeDeleted.Count > 1) return true;
        }

        return false;
    }


    private void FindAndDeleteElements(Transform clickedObject)
    {
        int clickedBlockX = Undefined, clickedBlockY = Undefined;

        GetClickedGrid(ref clickedBlockX, ref clickedBlockY, clickedObject);

        if (clickedBlockX != Undefined)
        {
            string clickedColor = Grid[clickedBlockX][clickedBlockY].tag;
            List<Point> elementsToBeTraversed = new List<Point>();
            List<Point> elementsToBeDeleted = new List<Point>();
            elementsToBeTraversed.Add(new Point(clickedBlockX, clickedBlockY));
            elementsToBeDeleted.Add(new Point(clickedBlockX, clickedBlockY));

            Dictionary<int, int> missingBricksAtColumns = new Dictionary<int, int>();
            missingBricksAtColumns.Add(clickedBlockX, 1);

            TraverseNew(elementsToBeTraversed, clickedColor, elementsToBeDeleted, missingBricksAtColumns);

            if (elementsToBeDeleted.Count < 2) return;


            AddScore(elementsToBeDeleted.Count);

            DeleteElements(elementsToBeDeleted, ShouldMissileBeCreated(elementsToBeDeleted), missingBricksAtColumns);
        }
    }

    private bool ShouldMissileBeCreated(List<Point> elementsToBeDeleted)
    {
        return elementsToBeDeleted.Count > CreateMissile;
    }

    private void BringNewBricks(Dictionary<int, int> dictionary)
    {
        var list = new List<int>();
        for (var i = 0; i < dictionary.Count; i++)
        {
            var item = dictionary.ElementAt(i);
            if (item.Value == 0) continue;
            list.Add(item.Key);
            dictionary[item.Key] -= 1;
        }

        CreateColumns(list);
    }

    private void CreateColumns(List<int> mc)
    {
        foreach (var t in mc)
        {
            UnityEngine.Object newBlock = Instantiate(blocks[Random.Range(0, blocks.Length)],
                new Vector3(transform.position.x + t * BrickWidth, transform.position.y + (Height - 1) * BrickHeight,
                    0), Quaternion.identity);
            GameObject gameObjectBlock = (GameObject)newBlock;
            newBlock.name = "" + _blockCounter++;
            Grid[t][Height - 1] = gameObjectBlock.transform;
        }
    }

    private void DeleteElements(List<Point> elementsToBeDeleted, bool createBomb, Dictionary<int, int> dictionary)
    {
        foreach (Point point in elementsToBeDeleted)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            if (createBomb)
            {
                createBomb = false;
                pos = Grid[point.GetX()][point.GetY()].gameObject.transform.position;
                Destroy(Grid[point.GetX()][point.GetY()].gameObject);
                var bombObject = elementsToBeDeleted.Count > CreateBomb
                    ? Instantiate(bomb, pos, Quaternion.identity)
                    : Instantiate(missiles[Random.Range(0, missiles.Length)], pos, Quaternion.identity);
                bombObject.name = _blockCounter++.ToString();
                Grid[point.GetX()][point.GetY()] = bombObject.transform;
                dictionary[point.GetX()] -= 1;
            }
            else
            {
                string brickId = Grid[point.GetX()][point.GetY()].gameObject.name;
                var bombOrBrick = GameObject.Find(brickId).GetComponent<BombAndBrick>();
                bombOrBrick.Trigger(point.GetX(), point.GetY());
            }
        }
    }

    public void DeleteFromGrid(int x, int y)
    {
        if (x != -1)
            Grid[x][y] = null;
    }

    private void GetClickedGrid(ref int x, ref int y, Transform clickedObject)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (ReferenceEquals(Grid[i][j], null))
                {
                    continue;
                }

                if (Grid[i][j].Equals(clickedObject))
                {
                    x = i;
                    y = j;
                    break;
                }
            }
        }
    }

    /**
     * Adds new score and updates text
     */
    private void AddScore(int count)
    {
        var scoreToAdd = (int)Math.Pow(count, 2);
        _score += scoreToAdd;
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
                if (!ReferenceEquals(Grid[x][y], null) && Grid[x][y].tag.Equals(color))
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

    public void GetBombedBrick(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
            BombIt(gameObjectTransform);
    }

    public void GetMissiledBrick(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
        {
            MissileIt(gameObjectTransform);
        }
    }

    public void GetMissiledBrickUpside(Transform gameObjectTransform)
    {
        if (!_crRunning && !gameOver)
        {
            MissileItUpside(gameObjectTransform);
        }
    }

    public void MissileIt(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();
        var dictionary = new Dictionary<int, int>();
        var listBomb = new List<Point>();
        listBomb.Add(new Point(x, y));

        FindMissiledElements(listBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary);
        AddScore(elementsToDelete.Count);
    }

    public void MissileItUpside(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();
        var dictionary = new Dictionary<int, int>();
        var listBomb = new List<Point>();
        listBomb.Add(new Point(x, y));

        FindMissiledElementsUpside(listBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary);
        AddScore(elementsToDelete.Count);
    }

    public void BombIt(Transform gameObjectTransform)
    {
        int x = Undefined, y = Undefined;
        GetClickedGrid(ref x, ref y, gameObjectTransform);
        List<Point> elementsToDelete = new List<Point>();
        var dictionary = new Dictionary<int, int>();
        var listBomb = new List<Point>();
        listBomb.Add(new Point(x, y));

        FindBombedElements(listBomb, dictionary, elementsToDelete);

        DeleteElements(elementsToDelete, false, dictionary);
        AddScore(elementsToDelete.Count);
    }
    
    private void FindMissiledElements(List<Point> listBomb, Dictionary<int, int> dictionary,
        List<Point> elementsToDelete)
    {
        var y = listBomb[0].GetY();
        _visitedBomb.Add(new Point(listBomb[0].GetX(), listBomb[0].GetY()));
        for (int i = 0; i < Width; i++)
        {
            if (!_visitedBomb.Contains(new Point(i, y)) && Grid[i][y].gameObject.GetComponent(typeof(GameElement)) != null)
            {
                String gameObjectId = Grid[i][y].transform.gameObject.name;
                GameElement gameElement = GameObject.Find(gameObjectId).GetComponent<GameElement>();
                gameElement.OnMouseDown();
            }

            addElement(i, y, elementsToDelete, dictionary);
        }
    }

    private void FindMissiledElementsUpside(List<Point> listBomb, Dictionary<int, int> dictionary,
        List<Point> elementsToDelete)
    {
        _visitedBomb.Add(new Point(listBomb[0].GetX(), listBomb[0].GetY()));
        var x = listBomb[0].GetX();
        for (int i = 0; i < Height; i++)
        {
            if (!_visitedBomb.Contains(new Point(x, i)) && Grid[x][i].gameObject.GetComponent(typeof(GameElement)) != null)
            {
                String gameObjectId = Grid[x][i].transform.gameObject.name;
                GameElement gameElement = GameObject.Find(gameObjectId).GetComponent<GameElement>();
                gameElement.OnMouseDown();
            }

            addElement(x, i, elementsToDelete, dictionary);
        }
    }

    private List<Point> _visitedBomb = new List<Point>();

    private void FindBombedElements(List<Point> listBomb, Dictionary<int, int> dictionary, List<Point> elementsToDelete)
    {
        _visitedBomb.Add(listBomb[0]);
        while (listBomb.Count > 0)
        {
            for (int i = listBomb[0].GetX() - 1; i <= listBomb[0].GetX() + 1; i++)
            {
                for (int j = listBomb[0].GetY() - 1; j <= listBomb[0].GetY() + 1; j++)
                {
                    if (i < 0 || i >= Width || j < 0 || j >= Height)
                        continue;
                    addElement(i, j, elementsToDelete, dictionary);
                    Point point = new Point(i, j);
                    if (!_visitedBomb.Contains(point) && Grid[i][j].gameObject.GetComponent(typeof(GameElement)) != null)
                    {
                        String gameObjectId = Grid[i][j].transform.gameObject.name;
                        GameElement gameElement = GameObject.Find(gameObjectId).GetComponent<GameElement>();
                        gameElement.OnMouseDown();
                    }
                }
            }

            if (listBomb.Count > 0)
            {
                listBomb.RemoveAt(0);
            }
        }
    }

    void addElement(int x, int y, List<Point> deleteList, Dictionary<int, int> dictionary)
    {
        if (x > -1 && x < Width && y > -1 && y < Height)
        {
            Point toAdd = new Point(x, y);
            if (!ReferenceEquals(Grid[x][y], null) && !deleteList.Contains(toAdd))
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

    /**
     * prints game over text
     */
    public void EndGame()
    {
        GameObject myObj = Instantiate(gameOverText, new Vector3(0, 0, 0), Quaternion.identity);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnakeGame : MonoBehaviour {

    public SnakeHead snakeHead;

    public GameObject wallPrefab;
    

    public Transform activeFood;
    Vector2 activeFoodGridPosition;

    public int gridHeight;
    public int gridWidth;

    //these allow us to move the entire snake game in unity space
    public int xOffset;
    public int yOffset;

    public int foodEaten = 0;

    float runningTime = 0;
    int stepsTakenSinceLastAte = 0;

    public enum GridType {Nothing, Head,Tail,Wall,Food}

    GridType[,] grid;

    Vector2 currentPositionOfSnakeNormalized = Vector2.zero;

    //neurla network stuff
    NeuralNetwork network;
    Action<NeuralNetwork, int,float> callbackWhenDone;
    List<float> networkInputs;
    List<float> outputs;

    // Use this for initialization
    void Start () {
        
    }
	
    public void setup(NeuralNetwork network,Action<NeuralNetwork, int,float> callback)
    {
        this.network = network;
        callbackWhenDone = callback;

        grid = new GridType[gridWidth, gridHeight];

        initializeGrid(grid);
        setStartingPositions(grid, snakeHead.transform);
        StartCoroutine(mainGameLoop());
    }

    IEnumerator mainGameLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Snake.timeScale);

            networkInputs = generateNetworkInputsBySurroundings();
            outputs = network.feedInputs(networkInputs);

            if(outputs.Count != 4)
            {
                Debug.Log("Mismatched output");
            }

            SnakeHead.MovingDirection chosenDirection = SnakeHead.MovingDirection.Right;
            //Debug.Log("=====================");
            //foreach(float i in outputs)
            //{
            //    Debug.Log(i);
            //}
            int indexOfChosenDirection = ExtensionMethods.highestIndexInList(outputs);
            //Debug.Log("Choice: " + indexOfChosenDirection);
            switch (indexOfChosenDirection)
            {
                case 0:
                    chosenDirection = SnakeHead.MovingDirection.Left;
                    break;
                case 1:
                    chosenDirection = SnakeHead.MovingDirection.Right;
                    break;
                case 2:
                    chosenDirection = SnakeHead.MovingDirection.Up;
                    break;
                case 3:
                    chosenDirection = SnakeHead.MovingDirection.Down;
                    break;
            }



            snakeHead.moveStep(chosenDirection);//also moves tails
            currentPositionOfSnakeNormalized = new Vector2(Mathf.RoundToInt(snakeHead.transform.position.x)-xOffset, Mathf.RoundToInt(snakeHead.transform.position.y)-yOffset);
            checkForDeath(grid);

            bool eaten = checkForEatenFood(grid);

            resetGrid(grid);

            mapGrid(grid, snakeHead);

            if(eaten)
            {
                foodEaten++;
                snakeHead.addToTail();
                addNewFood(grid,activeFood);
                stepsTakenSinceLastAte = -1;//-1 since we increment always
            }
            else
            {
                grid[(int)activeFoodGridPosition.x, (int)activeFoodGridPosition.y] = GridType.Food;
            }

            runningTime += Time.deltaTime;
            stepsTakenSinceLastAte++;

            


            if (stepsTakenSinceLastAte >= ((foodEaten+1) *40))
            {
                snakeDied();
            }
            //if (runningTime >= 10f)
            //{
            //    snakeDied();
            //}
            yield return null;
        }
    }

    /// <summary>
    /// Generates a list of inputs for the neural network based off how what is next to the snake head
    /// </summary>
    /// <returns></returns>
    List<float> generateNetworkInputsBySurroundings()
    {
        List<float> finalList = new List<float>();

        //Debug.Log(currentPositionOfSnakeNormalized);
        finalList.Add(getValueOfPoint((int)currentPositionOfSnakeNormalized.x - 1, (int)currentPositionOfSnakeNormalized.y));//left
        finalList.Add(getValueOfPoint((int)currentPositionOfSnakeNormalized.x + 1, (int)currentPositionOfSnakeNormalized.y));//right
        finalList.Add(getValueOfPoint((int)currentPositionOfSnakeNormalized.x, (int)currentPositionOfSnakeNormalized.y + 1));//up
        finalList.Add(getValueOfPoint((int)currentPositionOfSnakeNormalized.x, (int)currentPositionOfSnakeNormalized.y - 1));//down

        float xPosOfFoodNormalized =  Mathf.InverseLerp(-20f, 20f, activeFoodGridPosition.x);//0 - 1
        float yPosOfFoodNormalized = Mathf.InverseLerp(-20f, 20f, activeFoodGridPosition.y);

        float xPosOfHeadNormalized = Mathf.InverseLerp(-20f, 20f, currentPositionOfSnakeNormalized.x);
        float yPosOfHeadNormalized = Mathf.InverseLerp(-20f, 20f, currentPositionOfSnakeNormalized.y);
        //Debug.Log(xPosOfFoodNormalized + " " + yPosOfFoodNormalized);
        finalList.Add(xPosOfFoodNormalized - xPosOfHeadNormalized);
        finalList.Add(yPosOfFoodNormalized - yPosOfHeadNormalized);

        return finalList;
    }

    /// <summary>
    /// Generates a list of inputs for the neural network based off what is in the grid
    /// </summary>
    /// <returns></returns>
    List<float> generateNetworkInputsByGrid()
    {
        List<float> finalList = new List<float>();

        //init all of them to nothing
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                switch (grid[x, y])
                {
                    case GridType.Wall:
                        finalList.Add(-0.5f);
                        break;
                    case GridType.Food:
                        finalList.Add(0.5f);
                        break;
                    case GridType.Head:
                        finalList.Add(1);
                        break;
                    case GridType.Tail:
                        finalList.Add(-1f);
                        break;
                    case GridType.Nothing:
                        finalList.Add(0);
                        break;
                }
            }
        }

        return finalList;
    }

    bool checkForEatenFood(GridType[,] grid)
    {
        if (grid[(int)currentPositionOfSnakeNormalized.x, (int)currentPositionOfSnakeNormalized.y] == GridType.Food)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks the snake heads position and sees if hes on top of a tail or wall
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="snake"></param>
    void checkForDeath(GridType[,] grid)
    {
        bool dead = false;

        if(grid[(int)currentPositionOfSnakeNormalized.x,(int)currentPositionOfSnakeNormalized.y] == GridType.Wall)
        {
            dead = true;
        }
        else if (grid[(int)currentPositionOfSnakeNormalized.x, (int)currentPositionOfSnakeNormalized.y] == GridType.Tail)
        {
            dead = true;
        }


        if (dead)
        {
            snakeDied();
        }
    }

    void snakeDied()
    {
        snakeHead.die();
        callbackWhenDone(network,foodEaten, runningTime);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Go around the edge of the grid and build walls
    /// </summary>
    /// <param name="grid"></param>
    void initializeGrid(GridType[,] grid)
    {
        //init all of them to nothing
        for(int x = 0;x< grid.GetLength(0);x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = GridType.Nothing;
            }
        }

        //init the top row
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            grid[x, 0] = GridType.Wall;
            createWall(new Vector2(x, 0));
        }
        for (int x = 0; x < grid.GetLength(0); x++)//bottom row
        {
            grid[x, grid.GetLength(1)-1] = GridType.Wall;
            createWall(new Vector2(x, grid.GetLength(1) - 1));
        }

        //init the first col
        //we start at 1 and go to length-1 because we dont want duplicates on the corners of the grid
        for (int y = 1; y < grid.GetLength(1)-1; y++)
        {
            grid[0, y] = GridType.Wall;
            createWall(new Vector2(0,y));
        }
        for (int y = 1; y < grid.GetLength(1)-1; y++)//last col
        {
            grid[grid.GetLength(0)-1, y] = GridType.Wall;
            createWall(new Vector2(grid.GetLength(0) - 1, y));
        }

        //set starting snake position
        int xPos = grid.GetLength(0) / 2;
        int yPos = grid.GetLength(1) / 2;

        grid[xPos, yPos] = GridType.Head;

        snakeHead.transform.position = new Vector2(xPos+xOffset, yPos+yOffset);
    }

    /// <summary>
    /// Resets everything except the walls to empty
    /// </summary>
    /// <param name="grid"></param>
    void resetGrid(GridType[,] grid)
    {
        //init all of them to nothing
        for (int x = 1; x < grid.GetLength(0)-1; x++)
        {
            for (int y = 1; y < grid.GetLength(1)-1; y++)
            {
                grid[x, y] = GridType.Nothing;
            }
        }
    }

    void setStartingPositions(GridType[,] grid,Transform head)
    {
        int xPos = grid.GetLength(0)/2;

        int yPos = grid.GetLength(1) / 2;

        head.position = new Vector2(xPos + xOffset, yPos + yOffset);
        addNewFood(grid,activeFood);
    }

    /// <summary>
    /// Fills in all the correct types for the head and tail
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="snakeHead"></param>
    void mapGrid(GridType[,] grid,SnakeHead snakeHead)
    {
        grid[(int)currentPositionOfSnakeNormalized.x, (int)currentPositionOfSnakeNormalized.y] = GridType.Head;//set head

        foreach(SnakeTail st in snakeHead.tail)
        {
            int xPos = Mathf.RoundToInt(st.transform.position.x) - xOffset;
            int yPos = Mathf.RoundToInt(st.transform.position.y) - yOffset;

            grid[xPos, yPos] = GridType.Tail;
        }
    }

    /// <summary>
    /// Guess a random number in our grid until we find one not occupied
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="foodTransform"></param>
    void addNewFood(GridType[,] grid, Transform foodTransform)
    {
        int xPos = 0;
        int yPos = 0;
        while(grid[xPos,yPos] == GridType.Head || grid[xPos, yPos] == GridType.Wall || grid[xPos, yPos] == GridType.Tail)
        {
            xPos = UnityEngine.Random.Range(1,grid.GetLength(0)-2);
            yPos = UnityEngine.Random.Range(1, grid.GetLength(1) - 2);
        }

        grid[xPos, yPos] = GridType.Food;
        activeFoodGridPosition = new Vector2(xPos, yPos);
        activeFood.position = new Vector2(xPos + xOffset, yPos + yOffset);
    }

    void createWall(Vector2 gridPosition)
    {
        //GameObject wall = Instantiate(wallPrefab, new Vector2(xOffset + gridPosition.x, yOffset + gridPosition.y), Quaternion.identity);
    }


    //takes in a point for the grid, returns the value associated with the type
    float getValueOfPoint(int x,int y)
    {
        if(x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
        {
            return -1;
        }
        else
        {
            switch (grid[x, y])
            {
                case GridType.Wall:
                    //Debug.Log("wall");
                    return -1;
                case GridType.Food:
                    //Debug.Log("food");
                    return 1;
                case GridType.Head:
                    Debug.Log("shouldnt get this");
                    return 0;
                case GridType.Tail:
                    //Debug.Log("tail");
                    return -1;
                case GridType.Nothing:
                    return 0;
            }
        }

        return 0;
    }
}

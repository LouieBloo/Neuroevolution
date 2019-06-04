using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dueler : MonoBehaviour {

    public int teamNumber = 0;
    public float moveSpeed = 10f;

    int killCount = 0;

    public DuelerTurret turret;
    public DuelEdgeDetector edgeDetector;
    public bool alive = false;
    Action<NeuralNetwork,bool,bool,int> callback;

    NeuralNetwork brain;

    Transform target;

    IEnumerator gameLoop;

    // Use this for initialization
    void Start () {
        
	}

    public void start(Vector3 spawnPoint, NeuralNetwork networkToTest, Action<NeuralNetwork,bool,bool,int> callback,int teamNumber, Transform target)
    {
        this.gameObject.SetActive(true);

        this.transform.position = spawnPoint;
        this.teamNumber = teamNumber;
        this.callback = callback;
        this.alive = true;
        this.brain = networkToTest;
        this.target = target;
        this.killCount = 0;

        edgeDetector.setTeam(teamNumber);

        gameLoop = gamePlay();
        StartCoroutine(gameLoop);
    }
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKey(KeyCode.W))
        //{
        //    moveVertical(1);
        //}
        //if (Input.GetKey(KeyCode.S))
        //{
        //    moveVertical(-1);
        //}
        //if (Input.GetKey(KeyCode.D))
        //{
        //    moveHorizontal(1);
        //}
        //if (Input.GetKey(KeyCode.A))
        //{
        //    moveHorizontal(-1);
        //}
        //if (Input.GetMouseButtonDown(0))
        //{
        //    turret.fire(teamNumber, 1);
        //}

        //edgeDetector.getEdges();
    }

    IEnumerator gamePlay()
    {
        List<float> inputs;
        List<float> outputs;
        while (alive)
        {
            //add our x amount of edges
            inputs = edgeDetector.getEdges();
            //add our x distance and y distance normalized
            inputs.Add((this.target.position.x - transform.position.x) / DuelGame.mapSize);
            inputs.Add((this.target.position.y - transform.position.y) / DuelGame.mapSize);

            //string log = "Input: ";
            //foreach(float f in inputs)
            //{
            //    log += f + ",";
            //}
            //Debug.Log(log);

            outputs = brain.feedInputs(inputs);

            //log = "Output: ";
            //foreach (float f in outputs)
            //{
            //    log += f + ",";
            //}
            //Debug.Log(log);

            moveHorizontal(outputs[0]);
            moveVertical(outputs[1]);
            turret.rotate(outputs[2]);
            turret.fire(teamNumber, outputs[3],this);


            yield return null;
        }
    }

    void moveHorizontal(float amount)
    {
        transform.Translate(Vector3.right * Time.deltaTime * moveSpeed * amount, Space.World);
    }
    
    void moveVertical(float amount)
    {
        transform.Translate(Vector3.up * Time.deltaTime * moveSpeed * amount, Space.World);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col && alive)
        {
            if (col.gameObject.CompareTag("Wall"))
            {
                hitByWall();
            }
            else if (col.gameObject.CompareTag("Target"))
            {
                
            }
            else if (col.gameObject.CompareTag("DeathCircle"))
            {
                hitByWall();
            }
            else if (col.gameObject.CompareTag("Bullet"))
            {
                hitByBullet(col);
            }
        }
    }

    void die(bool won,bool wasShot)
    {
        StopCoroutine(this.gameLoop);
        this.gameLoop = null;

        this.transform.position = Vector3.zero;
        this.alive = false;
        this.turret.reset();

        this.gameObject.SetActive(false);
        callback(brain,won,wasShot,killCount);
    }

    public void win()
    {
        die(true,false);
    }

    void hitByBullet(Collider2D col)
    {
        if(alive && col.GetComponent<DuelBullet>().teamNumber != teamNumber)
        {
            col.GetComponent<DuelBullet>().alertOwnerOfHit();
            die(false,true);
        }
    }

    void hitByWall()
    {
        this.transform.position = new Vector3(0, 0, this.transform.position.z);
    }

    void hitByDeathCircle()
    {
        if (alive)
        {
            die(false, false);
        }
    }

    public void killedSomeone()
    {
        killCount++;
    }
}

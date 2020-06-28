using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BalanceObjects : MonoBehaviour {

    public List<GameObject> startingObjects;
    public Transform balancerBall;

    public List<BalancePolygons> allObjects = new List<BalancePolygons>();

    Vector2 startingBallPosition;

    Rigidbody2D balancerBallRigid;

    public float balancerBallMoveSpeed;

    bool on;

    float maxTimeUntilCompleted = 25;//how long until we decide it passes
    float timer = 0;

    NeuralNetwork myNeuralNetwork;

    List<float> networkInput;
    List<float> networkOutput;

    Action<NeuralNetwork,float> callback;

    void Update()
    {
        
    }

    void Awake()
    {
        
        foreach (GameObject g in startingObjects)
        {
            allObjects.Add(new BalancePolygons(g.transform, g.GetComponent<Collider2D>(), g.GetComponent<Rigidbody2D>()));
            g.GetComponent<Balancerod>().setup(rodHitFloor);
        }

        balancerBallRigid = balancerBall.GetComponent<Rigidbody2D>();
        startingBallPosition = balancerBall.position;
    }

    public void setup(NeuralNetwork inputNeuralNetwork,Action<NeuralNetwork,float> callback)
    {
        this.myNeuralNetwork = inputNeuralNetwork;
        this.callback = callback;

        on = true;
        StartCoroutine(mainBalanceRoutine());
        StartCoroutine(keepTimeRoutine());
    }

    IEnumerator mainBalanceRoutine()
    {
        while (true)
        {
            float moveStep = Time.deltaTime * balancerBallMoveSpeed;//how far we will move this frame

            //All raycasts
            //Get the distance to the nearest wall to left of us
            //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left);
            //float leftDistanceToWall = ExtensionMethods.normalizeDistanceFromRaycast(hit);

            //right of us
            //hit = Physics2D.Raycast(transform.position, Vector2.right);
            //float rightDistanceToWall = ExtensionMethods.normalizeDistanceFromRaycast(hit);

            networkInput = new List<float>();

            foreach(BalancePolygons bp in allObjects)
            {
                float normalizedXPos = Mathf.InverseLerp(-15f, 15f, bp.transform.position.x);//get value between 0-1 for xPos of the object
                float normalizedYPos = Mathf.InverseLerp(-15, 35f, bp.transform.position.y);//get value between 0-1 for yPos of the object

                float normalizedRotation = Mathf.InverseLerp(0f, 360f, bp.transform.localEulerAngles.z);//get value between 0-1 for angle of the object

                networkInput.Add(normalizedXPos);
                networkInput.Add(normalizedYPos);
                networkInput.Add(normalizedRotation);
            }


            float normalizedXPosOfBall = Mathf.InverseLerp(-15f, 15f, balancerBall.position.x);//get value between 0-1 for xPos of the ball
            float normalizedYPosOfBall = Mathf.InverseLerp(-15f, 35f, balancerBall.position.y);//get value between 0-1 for yPos of the ball

            networkInput.Add(normalizedXPosOfBall);
            networkInput.Add(normalizedYPosOfBall);

            //Debug.Log( normalizedXPosOfRod + " " + normalizedYPosOfRod + " " + normalizedXPosOfBall + " " + normalizedYPosOfBall + " " + normalizedRotationOfRod);

            //run against network
            networkOutput = myNeuralNetwork.feedInputs(networkInput);

            if (networkOutput == null || networkOutput.Count != 4) { Debug.Log("Mismatched outputs!"); yield break; }

            int selectedIndex = ExtensionMethods.highestIndexInList(networkOutput);

            Vector3 moveDirection = -balancerBall.transform.right;
            switch (selectedIndex)
            {
                case 0:
                    moveDirection = -balancerBall.transform.right;
                    break;
                case 1:
                    moveDirection = balancerBall.transform.up;
                    break;
                case 2:
                    moveDirection = balancerBall.transform.right;
                    break;
                case 3:
                    moveDirection = -balancerBall.transform.up;
                    break;
            }

            //balancerBall.position += moveDirection * moveStep;
            balancerBallRigid.AddForce(moveDirection * balancerBallMoveSpeed * Time.deltaTime);

            yield return null;
        }

    }

    void rodHitFloor()
    {
        if (on)
        {
            on = false;
            StopAllCoroutines();

            foreach (BalancePolygons bp in allObjects)
            {
                bp.stop();
            }


            balancerBall.position = new Vector3(-20, 0, 0);
            balancerBallRigid.velocity = Vector3.zero;
            balancerBallRigid.angularVelocity = 0;

            callback(myNeuralNetwork, timer);
        }
    }

    public void reset()
    {
        foreach (BalancePolygons bp in allObjects)
        {
            bp.start();
        }

        balancerBall.position = startingBallPosition;

        myNeuralNetwork = null;
    }

    public void setNoCollisionWithBalanceObject(BalanceObjects objectToNotCollideWith)
    {
        //all of our colliders with all of theirs
        foreach(BalancePolygons mine in allObjects)
        {
            foreach(BalancePolygons theirs in objectToNotCollideWith.allObjects)
            {
                Physics2D.IgnoreCollision(mine.collider, theirs.collider);
            }

            //also their ball
            Physics2D.IgnoreCollision(mine.collider, objectToNotCollideWith.balancerBall.GetComponent<Collider2D>());
        }

        //my ball with all of theirs
        foreach (BalancePolygons theirs in objectToNotCollideWith.allObjects)
        {
            Physics2D.IgnoreCollision(balancerBall.GetComponent<Collider2D>(), theirs.collider);
        }

        //both balls
        Physics2D.IgnoreCollision(balancerBall.GetComponent<Collider2D>(), objectToNotCollideWith.balancerBall.GetComponent<Collider2D>());
    }

    protected IEnumerator keepTimeRoutine()
    {
        timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if(timer >= maxTimeUntilCompleted)
            {
                rodHitFloor();
            }
            yield return null;
        }
    }

    public class BalancePolygons
    {
        public Transform transform;
        public Vector3 startingPosition;
        public Collider2D collider;
        public Rigidbody2D rigidbody;
        public Vector3 startingRotation;

        public BalancePolygons(Transform transform, Collider2D collider, Rigidbody2D rigidbody)
        {
            this.transform = transform;
            this.collider = collider;
            this.rigidbody = rigidbody;

            startingPosition = transform.position;
            startingRotation = transform.localEulerAngles;
        }

        public void start()
        {
            transform.position = startingPosition + new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(-3f, 3f),0);
            transform.localEulerAngles = startingRotation + new Vector3(0,0, UnityEngine.Random.Range(-45f,45f));

            rigidbody.gravityScale = 1;
            collider.enabled = true;
        }

        public void stop()
        {
            collider.enabled = false;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = 0;
            rigidbody.gravityScale = 0;
            transform.position = new Vector3(-20, 0, 0);
        }
    }
}

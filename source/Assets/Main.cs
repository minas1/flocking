using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Flocking;
using SteeringBehaviors;

public class Main : MonoBehaviour
{
    readonly int NUM_BIRDS_GAME = 200; // number of birds for the mini-game
    readonly string SAVEFILE = "highscores.sav";

    enum State
    {
        NotStarted, // the simulation has not started yet
        FreeWander, // birds wandering freely (no flocking)
        FlockWander, // FlockWander state set as the state for the birds
        InGame,
        FlockFormation,
        Paused,
        GameEnd
    }

    enum Mode
    {
        Flocking,
        Formation,
        Game
    }

    State state = State.NotStarted, prevState = State.NotStarted;

    Mode mode = Mode.Flocking;
    float cameraDistanceFromFlock = 150f;
    float cameraAngle = 90f;

    bool separationMenuShown = false;
    bool cohesionMenuShown = false;
    bool velocityMatchMenuShown = false;
    bool otherOptionsShown = false;

    bool showTrails = false; // show trails of the birds

    string numBirds = "50"; // number of birds
    string modelToLoad = "horse.txt";

    int cameraModeIndex = 0; // 0: focus on flock center, 1: follow a random boid, 2: free camera
    Transform boidTranformToFollow = null;

    GameObject mainCamera;
    GameObject player;

    FpsCalculator fpsCalculator;

    MultipleBirdState flockState;

    int boidsGenerated = 0;
    int boidsInUse = 0;

    Texture2D birdsIcon, clockIcon;

    List<string> highscoreNames;
    List<int> highscores;
    string playerName = "";

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        player.GetComponent<Starling>().maxSpeed = 0f;

        mainCamera = GameObject.Find("Main Camera");
        fpsCalculator = new FpsCalculator(50); // use last 50 frames to calculate fps

        birdsIcon = (Texture2D)Resources.Load("birds-icon");
        clockIcon = (Texture2D)Resources.Load("clock");

        highscoreNames = new List<string>();
        highscores = new List<int>();

        LoadHighScores(SAVEFILE);
    }
    
    void Reset()
    {
        state = State.NotStarted;
        Time.timeScale = 1f;

        player.GetComponent<Starling>().maxSpeed = 0f;
        DestroyBirds();

        var camera = GameObject.Find("Main Camera");
        camera.transform.position = new Vector3(0f, 6f, -10f);
        camera.transform.eulerAngles = new Vector3(0f, 0f, 0f);

        playerName = "";
    }

    // Update is called once per frame
    void Update()
    {
        fpsCalculator.Update(Time.deltaTime);

        if (state == State.NotStarted)
        {
            var camera = GameObject.Find("Main Camera");
            camera.transform.position = new Vector3(0f, 6f, -10f);
            camera.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (state == State.FlockWander)
        {
            // rotate camera
            if (Input.GetKey(KeyCode.LeftArrow))
                cameraAngle += 50f * Time.deltaTime;
            else if (Input.GetKey(KeyCode.RightArrow))
                cameraAngle -= 50f * Time.deltaTime;
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }

            // zoom in and out when the mouse wheel is scrolled
            cameraDistanceFromFlock -= 50f * Input.GetAxis("Mouse ScrollWheel");

            if (cameraModeIndex == 0)
                FocusCameraOnAnchor();
            else if (cameraModeIndex == 1)
                FocusCameraOnRandomBoid();
        }
        else if (state == State.FlockFormation)
        {
            // rotate camera
            if (Input.GetKey(KeyCode.LeftArrow))
                cameraAngle += 50f * Time.deltaTime;
            else if (Input.GetKey(KeyCode.RightArrow))
                cameraAngle -= 50f * Time.deltaTime;
            else if (Input.GetKeyDown(KeyCode.Escape))
                Pause();

            // zoom in and out when the mouse wheel is scrolled
            cameraDistanceFromFlock -= 50f * Input.GetAxis("Mouse ScrollWheel");

            FocusCameraOnAnchor();
        }
        else if (state == State.InGame || state == State.GameEnd)
        {
            // rotate camera
            if (Input.GetKey(KeyCode.LeftArrow))
                cameraAngle += 50f * Time.deltaTime;
            else if (Input.GetKey(KeyCode.RightArrow))
                cameraAngle -= 50f * Time.deltaTime;
            else if (Input.GetKeyDown(KeyCode.Escape))
                Pause();

            var g = flockState as FlockGame;
            if( g.TimePassed <= 0f )
                state = State.GameEnd;

            // zoom in and out when the mouse wheel is scrolled
            cameraDistanceFromFlock -= 50f * Input.GetAxis("Mouse ScrollWheel");
            
            FocusCameraOnAnchor();
        }
        else if (state == State.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Unpause();
            else if (Input.GetKeyDown(KeyCode.Q))
                Reset();
        }
    }

    void Pause()
    {
        prevState = state;
        state = State.Paused;

        Time.timeScale = 0f;
    }

    void Unpause()
    {
        state = prevState;
        Time.timeScale = 1f;
    }

    void OnGUI()
    {
        if (state == State.FlockWander)
        {
            var flockWander = (FlockWander)flockState;

            // show fps
            GUI.Box(new Rect(Screen.width - 80f, 20f, 60f, 25f), "fps: " + (int)fpsCalculator.fps);

            GUI.Box(new Rect(12.5f, 10f, 175f, 45f), "Cohesion to anchor");
            flockWander.cohesionToAnchorWeight = GUI.HorizontalSlider(new Rect(25, 35, 100, 30), flockWander.cohesionToAnchorWeight, 0.0f, 1.0f);
            GUI.Label(new Rect(130, 30, 100, 25), flockWander.cohesionToAnchorWeight.ToString("0.000"));

            GUI.Box(new Rect(12.5f, 60f, 175f, 45f), "Separation from anchor");
            flockWander.separationFromAnchorWeight = GUI.HorizontalSlider(new Rect(25, 85f, 100, 30), flockWander.separationFromAnchorWeight, 0.0f, 2.0f);
            GUI.Label(new Rect(130, 80, 100, 25), flockWander.separationFromAnchorWeight.ToString("0.000"));

            GUI.Box(new Rect(12.5f, 110f, 175f, 45f), "Alignment to anchor");
            flockWander.velMatchToAnchorWeight = GUI.HorizontalSlider(new Rect(25, 135f, 100, 30), flockWander.velMatchToAnchorWeight, 0.0f, 2.0f);
            GUI.Label(new Rect(130, 130, 100, 25), flockWander.velMatchToAnchorWeight.ToString("0.000"));

            float width, height;
            DrawSeparationOptions(12.5f, 170f, flockWander, out width, out height);
            DrawCohesionOptions(12.5f, height + 10f, flockWander, out width, out height);
            DrawVelocityMatchOptions(12.5f, height + 10f, flockWander, out width, out height);

            DrawOtherOptions(12.5f, height + 10f, flockWander, out width, out height);
        }
        else if (state == State.FlockFormation)
        {
            // show fps
            GUI.Box(new Rect(Screen.width - 80f, 20f, 60f, 25f), "fps: " + (int)fpsCalculator.fps);

            GUI.Box(new Rect(12.5f, 10f, 100f, 100f), "Load Model");
            modelToLoad = GUI.TextField(new Rect(25f, 30f, 75f, 25f), modelToLoad);

            if (GUI.Button(new Rect(25f, 60f, 40f, 25f), "Load"))
            {
                //var vertices = Util.LoadModel("Assets/Models/" + modelToLoad, 150f);
                //Util.SimplifyPoints(vertices, 3f);
                var vertices = Util.LoadModel("Assets/Models/" + modelToLoad);
                state = State.FlockFormation;

                // if there are more boids active than needed, disable some
                if (boidsInUse > vertices.Count)
                {
                    Debug.Log("disabling " + (boidsInUse - vertices.Count));
                    var starlings = GameObject.FindGameObjectsWithTag("Starling");
                    for (int i = 0; i < boidsInUse - vertices.Count; ++i)
                    {
                        if( starlings[i].name == "Player" )
                            continue;
                        starlings[i].SetActive(false);
                    }
                }
                // if we need more boids
                else if (boidsInUse < vertices.Count)
                {
                   // if we already have enough, enable some
                    if (boidsGenerated >= vertices.Count)
                    {
                        Debug.Log("enabling " + (vertices.Count - boidsInUse));
                        int count = vertices.Count - boidsInUse;
                        var starlings = GameObject.FindGameObjectsWithTag("Starling");
                        for (int i = 0; i < starlings.Length && count != 0; ++i)
                        {
                            if(starlings[i].name == "Player")
                                continue;

                            if (!starlings[i].activeSelf)
                            {
                                starlings[i].SetActive(true);
                                --count;
                            }
                        }
                    }
                    // if we don't, we need to generate new boids
                    else
                    {
                        int count = vertices.Count - boidsGenerated; // how many to generate
                        Debug.Log("generating " + count);

                        int j = 0;
                        var starlings = GameObject.FindGameObjectsWithTag("Starling");
                        GameObject starling = null;
                        while (!starlings[j].activeSelf || starlings[j].name == "Player")
                            ++j;
                        starling = starlings[j];

                        for (int i = 0; i < count; ++i)
                        {
                            var g = GameObject.Instantiate(starling) as GameObject;
                            g.name = "s-" + (boidsGenerated + i);
                            g.transform.position = player.transform.position;
                        }

                        boidsGenerated += count;
                    }
                }

                boidsInUse = vertices.Count;
                BoidsFormation(vertices);
            }
        }
        else if (state == State.InGame)
        {
            var myStyle = new GUIStyle();
            myStyle.normal.textColor = new Color(1f, 1f, 1f);
            myStyle.fontSize = 25;
            myStyle.alignment = TextAnchor.UpperCenter;

            GUI.DrawTexture(new Rect(5.0f, 12.5f, 158 * 0.5f, 108 * 0.5f), birdsIcon);
            GUI.DrawTexture(new Rect(25.0f, 75f, 48, 48), clockIcon);

            var g = flockState as FlockGame;
            GUI.Label(new Rect(85f, 17.5f, 50, 50), g.Score + "", myStyle);
            GUI.Label(new Rect(90f, 90f, 50, 50), g.TimePassed.ToString("0.0"), myStyle);
        }
        else if (state == State.NotStarted)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 150f, 400f, 375f), "Flocking");

            var style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f);

            GUI.Label(new Rect(Screen.width * 0.5f - 20f, Screen.height * 0.5f - 110f, 40f, 25f), "Mode");
            int modeIndex = GUI.SelectionGrid(new Rect(Screen.width * 0.5f - 200f * 0.5f, Screen.height * 0.5f - 90f, 200f, 70f), (mode == Mode.Flocking ? 0 : mode == Mode.Formation ? 1 : 2), new GUIContent[]{new GUIContent("Wander Behavior"), new GUIContent("Shape Formation"), new GUIContent("\"Gather the Flock mini-game\"")}, 1);
            mode = (modeIndex == 0 ? Mode.Flocking : modeIndex == 1 ? Mode.Formation : Mode.Game);

            if (mode == Mode.Flocking)
            {
                GUI.Label(new Rect(Screen.width * 0.5f - 75, Screen.height * 0.5f - 10f, 150, 20f), "Number of Boids", style);
                numBirds = GUI.TextField(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.5f - 20f + 30f, 100f, 20f), numBirds);
            }
            else if (mode == Mode.Game)
                GUI.Label(new Rect(Screen.width * 0.5f - 75f, Screen.height * 0.5f + 25f, 150, 20f), "Get close to birds to make them join your flock\nYou have 60 seconds\nThe best scores are saved\n\nUse the A/S/W/D keys to move around", style);

            // button to start the simulation
            if (GUI.Button(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.5f + 100f, 100f, 40f), "Start") )
            {
                if (mode == Mode.Flocking)
                {
                    GenerateBoidsAround(int.Parse(numBirds), new Vector3(700f, 100f, 350f));
                    state = State.FlockWander;
                    BoidsFlocking();
                }
                else if (mode == Mode.Formation)
                {
                    var vertices = Util.LoadModel("Assets/Models/" + modelToLoad);
                    
                    GenerateBoidsAround(vertices.Count, new Vector3(700f, 100f, 350f));
                    boidsGenerated = boidsInUse = vertices.Count;
                    
                    state = State.FlockFormation;
                    player.transform.position = new Vector3(700f, 90f, 350f);
                    player.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                    BoidsFormation(vertices);

                }
                else if (mode == Mode.Game)
                {
                    var bbox = new Bounds(new Vector3(1162, 113, 770), new Vector3(500, 150, 500));
                    GenerateBoidsBetween(NUM_BIRDS_GAME, bbox);
                    BoidsHuntMiniGame();
                    state = State.InGame;
                    cameraAngle = 0f;
                    cameraDistanceFromFlock = 50f;
                }
            }
            else if (GUI.Button(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.5f + 160f, 100f, 40f), "Exit"))
                Application.Quit();
        }
        else if (state == State.Paused)
        {
            ShowPauseMenu();
        }
        else if (state == State.GameEnd)
        {
            var myStyle = new GUIStyle();
            myStyle.normal.textColor = new Color(1f, 1f, 1f);
            myStyle.fontSize = 25;
            myStyle.alignment = TextAnchor.UpperCenter;

            GUI.DrawTexture(new Rect(5.0f, 12.5f, 158 * 0.5f, 108 * 0.5f), birdsIcon);
            GUI.DrawTexture(new Rect(25.0f, 75f, 48, 48), clockIcon);
            
            var g = flockState as FlockGame;
            GUI.Label(new Rect(85f, 17.5f, 50, 50), g.Score + "", myStyle);
            GUI.Label(new Rect(90f, 90f, 50, 50), g.TimePassed.ToString("0.0"), myStyle);

            ShowGameEndScreen((flockState as FlockGame).Score);
        }
    }

    /// <summary>
    /// Generates the boids and puts them in the scene.
    /// They are generated with respect to 'pos'
    /// </summary>
    void GenerateBoidsAround(int n, Vector3 pos)
    {
        if (n <= 0)
            return;

        var star = GameObject.Find("s-0");
        for (int i = 1; i < n; ++i)
        {
            var go = GameObject.Instantiate(star) as GameObject;
            go.name = "s-" + i;
        }
        
        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        Debug.Log("# of birds: " + (starlings.Length - 1));

        int x = 0, y = 0, z = 0;
        
        for (int i = 0; i < starlings.Length; ++i)
        {
            if( starlings[i].name == "Player" )
                continue;
            
            // line up the birds in a cube
            starlings[i].transform.position = pos + new Vector3(5f * x, 3f * y, 5f * z);

            ++z;
            if( z == 10 )
            {
                ++x;
                z = 0;
            }
            if( x == 10 )
            {
                ++y;
                x = 0;
            }
        }
    }

    void GenerateBoidsBetween(int n, Bounds bbox)
    {
        if (n <= 0)
            return;
        
        var star = GameObject.Find("s-0");
        for (int i = 1; i < n; ++i)
        {
            var go = GameObject.Instantiate(star) as GameObject;
            go.name = "s-" + i;
        }
        //star.name = "s-" + (n - 1);
        
        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        Debug.Log("# of birds: " + (starlings.Length - 1));

        for (int i = 0; i < starlings.Length; ++i)
        {
            if( starlings[i].name == "Player" )
                continue;
            
            // line up the birds in a cube
            starlings[i].transform.position = new Vector3(UnityEngine.Random.Range(bbox.min.x, bbox.max.x), UnityEngine.Random.Range(bbox.min.y, bbox.max.y), UnityEngine.Random.Range(bbox.min.z, bbox.max.z));
        }
    }

    /// <summary>
    /// Sets the boids behavior to flocking (FlockWander)
    /// </summary>
    void BoidsFlocking()
    {
        var wander = new FlockWander();
        flockState = wander;
        wander.anchor = player.GetComponent<Starling>();
        player.GetComponent<Starling>().maxSpeed = 10f;
        player.transform.Find("Sphere").gameObject.SetActive(true);

        player.transform.position = new Vector3(700f, 90f, 350f);
        player.transform.eulerAngles = new Vector3(0f, 0f, 0f);

        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        for (int i = 0; i < starlings.Length; ++i)
        {
            if( starlings[i].name == "Player" )
                continue;
            
            var starling = starlings[i].GetComponent<Starling>();
            starlings[i].transform.Find("Sphere").gameObject.SetActive(false);

            starling.state = wander;
            wander.Add(starling);
        }

        wander.Init();

        cameraDistanceFromFlock = 150f;
        cameraAngle = 90f;
    }

    void BoidsFormation(List<Vector3> vertices)
    {
        var formation = new FlockFormation(vertices);
        flockState = formation;
        formation.anchor = player.GetComponent<Starling>();
        player.transform.Find("Sphere").gameObject.SetActive(false);
        player.GetComponent<Starling>().maxSpeed = 15f;

        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        for (int i = 0; i < starlings.Length; ++i)
        {
            if (starlings[i].name == "Player")
                continue;
            
            var starling = starlings[i].GetComponent<Starling>();
            var sphere = starlings[i].transform.Find("Sphere").gameObject;
            sphere.SetActive(true);
            sphere.transform.localScale = new Vector3(2f, 2f, 2f);
            sphere.GetComponent<Renderer>().material.name = "Water Splash2";
            sphere.GetComponent<Renderer>().material.color = new Color(1, 1, 1,1);

            starling.state = formation;
            formation.Add(starling);
        }
        
        formation.anchor.maxSpeed *= 0.75f;
        formation.Init();
    }

    void BoidsHuntMiniGame()
    {
        var wander = new FlockGame();
        flockState = wander;

        wander.anchor = player.GetComponent<Starling>();

        player.transform.Find("Sphere").gameObject.SetActive(false);

        player.GetComponent<Starling>().maxSpeed = 30f;
        player.transform.position = new Vector3(1302f, 60f, 562f);
        player.transform.eulerAngles = new Vector3(0f, 0f, 0f);

        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        for (int i = 0; i < starlings.Length; ++i)
        {
            if (starlings[i].name == "Player" )
                continue;

            var starling = starlings[i].GetComponent<Starling>();
            var sphere = starlings[i].transform.Find("Sphere").gameObject;
            sphere.SetActive(true);
            sphere.transform.localScale = new Vector3(4f, 4f, 4f);
            sphere.GetComponent<Renderer>().material.color = new Color(10.0f, 10.0f, 10.0f, 0.99f);

            starling.state = wander;
            wander.Add(starling);
        }
        
        wander.anchor.maxSpeed *= 0.75f;
        wander.Init();
    }

    /// <summary>
    /// Draws the options for the Separation of the flock
    /// </summary>
    void DrawSeparationOptions(float x, float y, FlockWander flockWander, out float width, out float height)
    {
        if (separationMenuShown)
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "-"))
                separationMenuShown = !separationMenuShown;

            float temp;
            int tempInt;

            GUI.Box(new Rect(x, y, 175f, 255f), "Separation");

            width = x + 175f;
            height = y + 255;

            // slider - flock separation weight
            GUI.Label(new Rect(x + 12.5f, y + 30f, 150, 25), "Weight");
            flockWander.separationWeight = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 50f, 100, 30), flockWander.separationWeight, 0.0f, 2.0f);
            GUI.Label(new Rect(x + 117.5f, y + 45f, 100, 25), flockWander.separationWeight.ToString("0.000"));

            // slider - flock cohesion angle
            GUI.Label(new Rect(x + 12.5f, y + 65f, 150, 25), "Angle");
            flockWander.SeparationAngle = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 85f, 100, 30), flockWander.SeparationAngle, 0.0f, 360.0f);
            GUI.Label(new Rect(x + 117.5f, y + 80f, 100, 25), flockWander.SeparationAngle.ToString("0.000"));

            // SEPARATION a-knn approximation value
            GUI.Label(new Rect(x + 12.5f, y + 95, 150, 25), "a-knn epsilon");
            if (float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 115f, 150, 25), flockWander.separationAknnVal.ToString(".000")), out temp))
                flockWander.separationAknnVal = temp;
            else
                flockWander.separationAknnVal = 1.0f;
            
            // SEPARATION distance
            GUI.Label(new Rect(x + 12.5f, y + 145, 150, 25), "neighbors within (m)");
            if(float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 165, 150, 25), flockWander.separationDistance.ToString(".000")), out temp))
                flockWander.separationDistance = temp;
            else
                flockWander.separationDistance = 5.0f;

            // SEPARATION k (number of neighbors)
            GUI.Label(new Rect(x + 12.5f, y + 195, 150, 25), "# of neighbors");
            if( int.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 215, 150, 25), flockWander.separationK.ToString()), out tempInt) )
                flockWander.separationK = tempInt;
            else
                flockWander.separationK = 5;
        }
        else
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "+"))
                separationMenuShown = !separationMenuShown;

            GUI.Box(new Rect(x, y, 175f, 30f), "Separation");
            
            width = x + 175f;
            height = y + 30f;

        }
    }

    /// <summary>
    /// Draws the options for the Cohesion of the flock
    /// </summary>
    void DrawCohesionOptions(float x, float y, FlockWander flockWander, out float width, out float height)
    {
        if (cohesionMenuShown)
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "-"))
                cohesionMenuShown = !cohesionMenuShown;
            
            float temp;
            int tempInt;
            
            GUI.Box(new Rect(x, y, 175f, 255f), "Cohesion");
            
            width = x + 175f;
            height = y + 255f;
            
            // slider - flock cohesion weight
            GUI.Label(new Rect(x + 12.5f, y + 30f, 150, 25), "Weight");
            flockWander.cohesionWeight = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 50f, 100, 30), flockWander.cohesionWeight, 0.0f, 2.0f);
            GUI.Label(new Rect(x + 117.5f, y + 45f, 100, 25), flockWander.cohesionWeight.ToString("0.000"));

            // slider - flock cohesion angle
            GUI.Label(new Rect(x + 12.5f, y + 65f, 150, 25), "Angle");
            flockWander.CohesionAngle = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 85f, 100, 30), flockWander.CohesionAngle, 0.0f, 360.0f);
            GUI.Label(new Rect(x + 117.5f, y + 80f, 100, 25), flockWander.CohesionAngle.ToString("0.000"));

            // cohesion a-knn approximation value
            GUI.Label(new Rect(x + 12.5f, y + 95f, 150, 25), "a-knn epsilon");
            if (float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 115f, 150, 25), flockWander.cohesionAknnVal.ToString(".000")), out temp))
                flockWander.cohesionAknnVal = temp;
            else
                flockWander.cohesionAknnVal = 50.0f;
            
            // cohesion distance
            GUI.Label(new Rect(x + 12.5f, y + 145, 150, 25), "neighbors within (m)");
            if (float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 165f, 150, 25), flockWander.cohesionDistance.ToString(".000")), out temp))
                flockWander.cohesionDistance = temp;
            else
                flockWander.cohesionDistance = 100.0f;
            
            // cohesion k (number of neighbors)
            GUI.Label(new Rect(x + 12.5f, y + 195f, 150, 25), "# of neighbors");
            if (int.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 215f, 150, 25), flockWander.cohesionK.ToString()), out tempInt))
                flockWander.cohesionK = tempInt;
            else
                flockWander.cohesionK = 30;
        }
        else
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "+"))
                cohesionMenuShown = !cohesionMenuShown;

            GUI.Box(new Rect(x, y, 175f, 30f), "Cohesion");
            
            width = x + 175f;
            height = y + 30f;
        }
    }

    /// <summary>
    /// Draws the options for the Velocity Match of the flock
    /// </summary>
    void DrawVelocityMatchOptions(float x, float y, FlockWander flockWander, out float width, out float height)
    {
        if (velocityMatchMenuShown)
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "-"))
                velocityMatchMenuShown = !velocityMatchMenuShown;
            
            float temp;
            int tempInt;
            
            GUI.Box(new Rect(x, y, 175f, 255f), "Velocity Match");
            
            width = x + 175f;
            height = y + 255f;
            
            // slider - flock velocity match weight
            GUI.Label(new Rect(x + 12.5f, y + 30f, 150, 25), "Weight");
            flockWander.velocityMatchWeight = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 50f, 100, 30), flockWander.velocityMatchWeight, 0.0f, 2.0f);
            GUI.Label(new Rect(x + 117.5f, y + 45f, 100, 25), flockWander.velocityMatchWeight.ToString("0.000"));

            // slider - flock angle
            GUI.Label(new Rect(x + 12.5f, y + 65f, 150, 25), "Angle");
            flockWander.VelocityMatchAngle = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 85f, 100, 30), flockWander.VelocityMatchAngle, 0.0f, 360.0f);
            GUI.Label(new Rect(x + 117.5f, y + 80f, 100, 25), flockWander.VelocityMatchAngle.ToString("0.000"));

            // velocityMatch a-knn approximation value
            GUI.Label(new Rect(x + 12.5f, y + 95f, 150, 25), "a-knn epsilon");
            if (float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 115f, 150, 25), flockWander.velocityMatchAknnVal.ToString(".000")), out temp))
                flockWander.velocityMatchAknnVal = temp;
            else
                flockWander.velocityMatchAknnVal = 50.0f;
            
            // velocityMatch distance
            GUI.Label(new Rect(x + 12.5f, y + 145f, 150, 25), "neighbors within (m)");
            if (float.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 165f, 150, 25), flockWander.velocityMatchDistance.ToString(".000")), out temp))
                flockWander.velocityMatchDistance = temp;
            else
                flockWander.velocityMatchDistance = 100.0f;
            
            // velocityMatch k (number of neighbors)
            GUI.Label(new Rect(x + 12.5f, y + 195f, 150, 25), "# of neighbors");
            if (int.TryParse(GUI.TextField(new Rect(x + 12.5f, y + 215f, 150, 25), flockWander.velocityMatchK.ToString()), out tempInt))
                flockWander.velocityMatchK = tempInt;
            else
                flockWander.velocityMatchK = 30;
        }
        else
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "+"))
                velocityMatchMenuShown = !velocityMatchMenuShown;
            
            GUI.Box(new Rect(x, y, 175f, 30f), "Velocity Match");
            
            width = x + 175f;
            height = y + 30f;
        }
    }

    void DrawOtherOptions(float x, float y, FlockWander flockWander, out float width, out float height)
    {
        if (otherOptionsShown)
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "-"))
                otherOptionsShown = !otherOptionsShown;

            GUI.Box(new Rect(x, y, 175f, 195f), "Other Options");
            width = x + 175f;
            height = y + 195f;

            // bird trails
            bool oldShowTrails = showTrails;
            showTrails = GUI.Toggle(new Rect(x + 12.5f, y + 30f, 150f, 20f), showTrails, " Show trails");

            if (oldShowTrails != showTrails)
            {
                var starlings = GameObject.FindGameObjectsWithTag("Starling");
                foreach(var starling in starlings)
                    if( starling.name != "Player" ) // skip the Player
                        starling.GetComponent<TrailRenderer>().enabled = showTrails;
            }

            var cameraModes = new GUIContent[]{new GUIContent("Flock leader"), new GUIContent("Random boid")};
            GUI.Label(new Rect(x + 12.5f, y + 55f, 100f, 25f), "Camera Mode");
            int oldCameraModeIndex = cameraModeIndex;
            cameraModeIndex = GUI.SelectionGrid(new Rect(x + 25f, y + 75f, 100f, 65f), cameraModeIndex, cameraModes, 1);

            // if camera mode has changed and it was set to follow a random boid
            if (oldCameraModeIndex != cameraModeIndex)
            {
                if (cameraModeIndex == 0)
                    cameraDistanceFromFlock = 135f;
                else if( cameraModeIndex == 1 )
                {
                    cameraDistanceFromFlock = 10f;

                    var boid = GameObject.Find("s-" + UnityEngine.Random.Range(0, int.Parse(numBirds)));
                    boidTranformToFollow = boid.transform;
                }
            }

            // slider - percentage of birds to update every frame
            GUI.Label(new Rect(x + 12.5f, y + 150f, 150, 25), "% boids to update");
            flockWander.percentageOfBoidsToUpdate = GUI.HorizontalSlider(new Rect(x + 12.5f, y + 170f, 100, 30), flockWander.percentageOfBoidsToUpdate, 0.01f, 1.0f);
            GUI.Label(new Rect(x + 117.5f, y + 165f, 100, 25), (flockWander.percentageOfBoidsToUpdate * 100f).ToString("0.0") + "%");

        }
        else
        {
            if (GUI.Button(new Rect(x + 150f, y + 5f, 20, 20), "+"))
                otherOptionsShown = !otherOptionsShown;
            
            GUI.Box(new Rect(x, y, 175f, 30f), "Other Options");
            
            width = x + 175f;
            height = y + 30f;
        }
    }

    /// <summary>
    /// Focuses the camera on the flock.
    /// </summary>
    void FocusCameraOnFlock()
    {
        Vector3 center, avgVel;
        flockState.GetFlockInfo(out center, out avgVel);

        var temp = avgVel;
        temp.y = 0f;

        mainCamera.transform.position = center - Quaternion.AngleAxis(cameraAngle, Vector3.up) * temp.normalized * cameraDistanceFromFlock;
        mainCamera.transform.LookAt(center);
    }

    /// <summary>
    /// Focuses the camera on the flock's anchor.
    /// </summary>
    void FocusCameraOnAnchor()
    {
        Vector3 center, avgVel;

        center = player.transform.position;
        avgVel = player.GetComponent<Rigidbody>().velocity;

        if (avgVel.sqrMagnitude == 0f)
            avgVel.Set(0f, 0f, 20f);

        var temp = avgVel;
        temp.y = 0f;

        mainCamera.transform.position = center - Quaternion.AngleAxis(cameraAngle, Vector3.up) * temp.normalized * cameraDistanceFromFlock;
        mainCamera.transform.LookAt(center);

    }

    /// <summary>
    /// Focuses the camera a random boid
    /// </summary>
    void FocusCameraOnRandomBoid()
    {
        mainCamera.transform.position = boidTranformToFollow.position + boidTranformToFollow.up * 2f - boidTranformToFollow.forward * cameraDistanceFromFlock;
        mainCamera.transform.LookAt(boidTranformToFollow.position);
    }


    void ShowPauseMenu()
    {
        var style = new GUIStyle();
        style.normal.textColor = new Color(1f, 1f, 1f);
        style.alignment = TextAnchor.UpperCenter;

        float w = float.NaN, h = float.NaN;

        if (prevState == State.InGame)
        {
            w = 50f; h = 50f;
            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.25f - h * 0.5f, w, h), "Game Paused", style);

            style.fontSize = 20;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.35f - h * 0.5f, w, h), "Controls", style);

            style.fontSize = 15;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.40f - h * 0.5f, w, h), "A / S / W / D to move around", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.45f - h * 0.5f, w, h), "Left / Right arrows to rotate camera", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.50f - h * 0.5f, w, h), "Mouse scroll to zoom in / out", style);

            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.60f - h * 0.5f, w, h), "Press ESC to resume", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.65f - h * 0.5f, w, h), "Press Q to exit", style);
        }
        else if (prevState == State.FlockWander)
        {
            w = 50f; h = 50f;
            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.25f - h * 0.5f, w, h), "Game Paused", style);
            
            style.fontSize = 20;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.35f - h * 0.5f, w, h), "Controls", style);
            
            style.fontSize = 15;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.40f - h * 0.5f, w, h), "A / S / W / D to move around", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.45f - h * 0.5f, w, h), "Left / Right arrows to rotate camera", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.50f - h * 0.5f, w, h), "Mouse scroll to zoom in / out", style);
            
            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.60f - h * 0.5f, w, h), "Press ESC to resume", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.65f - h * 0.5f, w, h), "Press Q to exit", style);
        }
        else if (prevState == State.FlockFormation)
        {
            w = 50f; h = 50f;
            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.25f - h * 0.5f, w, h), "Game Paused", style);
            
            style.fontSize = 20;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.35f - h * 0.5f, w, h), "Controls", style);
            
            style.fontSize = 15;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.40f - h * 0.5f, w, h), "A / S / W / D to move around", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.45f - h * 0.5f, w, h), "Left / Right arrows to rotate camera", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.50f - h * 0.5f, w, h), "Mouse scroll to zoom in / out", style);
            
            style.fontSize = 25;
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.60f - h * 0.5f, w, h), "Press ESC to resume", style);
            GUI.Label(new Rect(Screen.width * 0.5f - w * 0.5f, Screen.height * 0.65f - h * 0.5f, w, h), "Press Q to exit", style);
        }
    }

    void LoadHighScores(string filename)
    {
        // create the file if it doesn't exist
        if (!File.Exists(Application.persistentDataPath + "/" + filename))
        {
            var writer = File.CreateText(Application.persistentDataPath + "/" + filename);
            writer.Close();
        }
        else
        {
            var reader = File.OpenText(Application.persistentDataPath + "/" + filename);

            string name = "";
            int score = -1;
            int count = 0;

            string line;
            while( (line = reader.ReadLine()) != null )
            {
                if( count % 2 == 0 )
                    name = line;
                else
                {
                    score = int.Parse(line);
                    highscoreNames.Add(name);
                    highscores.Add(score);
                }

                ++count;
            }
            reader.Close();
        }
    }

    void SaveHighScores(string filename, List<string> names, List<int> scores)
    {
        var writer = File.CreateText(Application.persistentDataPath + "/" + filename);

        if (names.Count > 3)
        {
            names.RemoveRange(3, names.Count - 3);
            scores.RemoveRange(3, scores.Count - 3);
        }

        for (int i = 0; i < names.Count; ++i)
        {
            writer.WriteLine(names[i]);
            writer.WriteLine(scores[i]);
        }
        writer.Close();
    }

    void ShowGameEndScreen(int playerScore)
    {
        var style = new GUIStyle();
        style.normal.textColor = new Color(1f, 1f, 1f);
        style.fontSize = 25;
        style.alignment = TextAnchor.UpperCenter;

        GUI.Label(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.20f, 100f, 100f), "Game Completed!", style);

        var names = new List<string>(highscoreNames);
        var scores = new List<int>(highscores);

        if (scores.Count < 3 || playerScore > scores[2])
        {
            names.Add(playerName);
            scores.Add(playerScore);
        }

        // sort high scores
        for (int i = 0; i < scores.Count - 1; ++i)
        {
            for (int j = i + 1; j < scores.Count; ++j)
            {
                if( scores[j] > scores[i] )
                {
                    int temp = scores[i];
                    scores[i] = scores[j];
                    scores[j] = temp;

                    string strTemp = names[i];
                    names[i] = names[j];
                    names[j] = strTemp;
                }
            }
        }

        style.fontSize = 20;
        // show high scores
        for(int i = 0; i < (scores.Count < 3 ? scores.Count : 3); ++i)
            GUI.Label(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.30f + i * 40f, 100f, 100f), names[i] + "   " + scores[i], style);

        // if the player is in the highscores
        if ((scores.Count >= 3 && playerScore >= scores[2]) || scores.Count < 3)
        {
            GUI.Label(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.50f, 100f, 100f), "Congratulations! You have made a highscore!", style);
        
            playerName = GUI.TextField(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.60f, 100f, 35f), playerName);

            if (GUI.Button(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.675f, 100f, 35f), "Save"))
            {
                state = State.NotStarted;
                SaveHighScores(SAVEFILE, names, scores);
                player.GetComponent<Starling>().maxSpeed = 0f;
                DestroyBirds();
            }
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width * 0.5f - 50f, Screen.height * 0.675f, 100f, 35f), "Back"))
            {
                state = State.NotStarted;
                player.GetComponent<Starling>().maxSpeed = 0f;
                DestroyBirds();
            }
        }
    }


    /// <summary>
    /// Destroys all birds
    /// </summary>
    void DestroyBirds()
    {
        var starlings = GameObject.FindGameObjectsWithTag("Starling");
        for (int i = 0; i < starlings.Length; ++i)
        {
            starlings[i].GetComponent<Starling>().state = null;
            
            if (starlings[i].name == "Player" || starlings[i].name == "s-0")
                continue;
            
            GameObject.Destroy(starlings[i]);
        }
    }
}

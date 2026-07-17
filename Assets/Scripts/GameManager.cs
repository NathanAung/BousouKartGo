using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] bool debugMode = false;
    [SerializeField] int lapsToWin = 3;

    [SerializeField] Transform[] carsPos;
    [SerializeField] private CameraFollow[] playerCameras;
    private List<Car> cars;
    private bool raceEnded = false;

    [Header("LapUI")]
    [SerializeField] private RectTransform lapP1;
    [SerializeField] private RectTransform lapP2;

    [Header("End Sequence")]
    [SerializeField] private GameObject fireworkPrefab;
    [SerializeField] private Transform[] fireworkSpawnPoints;

    [SerializeField] private WinScreenUI winScreen;
    [SerializeField] private Transform victoryRespawnPoint;

    [SerializeField] private Transform victoryCameraPoint;
    [SerializeField] private AudioSource fireworksAudio;

    [Header("Return To Title")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private float returnInputDelay = 1.5f;

    private float raceEndTime;

    void Start()
    {
        int i = 0;
        cars = new List<Car>();

        foreach(Transform t in carsPos)
        {
            PlayerLap playerLap = t.GetComponent<PlayerLap>();

            if (playerLap == null) {
                Debug.LogError("GameManager: CarObject missing PlayerLap!");
                continue;
            }

            cars.Add(new Car($"Car[{i}]",i,t,playerLap));
            i++;
        }
    }

    void Update()
    {
        foreach (Car car in cars)
        {
            if (car.lap.lapState == 2)
            {
                car.lap.lapState = 0;
                car.lap.lap += 1;
                (car.name == "Car[0]" ? lapP1 : lapP2).position += Vector3.right * 40;
            }

            if (car.lap.lap >= lapsToWin && !raceEnded)
            {
                // GAME WIN
                //Debug.Break();
                (car.name == "Car[0]" ? lapP1 : lapP2).gameObject.SetActive(false);

                raceEnded = true;
                winScreen.ShowWinner(car.id);
                LaunchFireworks();
                if (fireworksAudio != null) 
                { 
                    fireworksAudio.Play();
                }

                playerController winnerController =
                    car.pos.GetComponent<playerController>();

                if (winnerController != null)
                {
                    winnerController.SetVictoryRespawnPoint(
                        victoryRespawnPoint
                    );

                    winnerController.LockControls();
                    winnerController.StartVictoryDonut(0.5f);
                }

                if (
                    car.id >= 0 &&
                    car.id < playerCameras.Length &&
                    playerCameras[car.id] != null
                )
                {
                    playerCameras[car.id].StartVictoryCamera(
                        victoryCameraPoint
                    );
                }

                foreach (Car otherCar in cars)
                {
                    if (otherCar.id == car.id)
                        continue;

                    playerController loserController =
                        otherCar.pos.GetComponent<playerController>();

                    if (loserController != null)
                    {
                        loserController.StartLoserSequence();
                    }
                }

                raceEndTime = Time.time;
            }
        }

        if (raceEnded && Time.time >= raceEndTime + returnInputDelay && AnyInputPressed())
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    private void OnGUI()
    {
        if (!debugMode)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 350, Screen.height));

        GUILayout.Label("<b>Car Positions</b>");

        foreach (Car car in cars)
        {
            Vector3 p = car.pos.position;
            GUILayout.Label($"{car.name}: Lap={car.lap.lap}, X={p.x:F2}, Y={p.y:F2}, Z={p.z:F2}");
        }

        GUILayout.EndArea();
    }

    private void LaunchFireworks()
    {
        foreach (Transform spawnPoint in fireworkSpawnPoints)
        {
            StartCoroutine(LaunchFireworkDelayed(spawnPoint));
        }
    }

    private IEnumerator LaunchFireworkDelayed(Transform spawnPoint)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));

        Instantiate(
            fireworkPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }

    private bool AnyInputPressed()
    {
        if (
            Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame
        )
        {
            return true;
        }

        foreach (Gamepad gamepad in Gamepad.all)
        {
            foreach (InputControl control in gamepad.allControls)
            {
                if (
                    control is UnityEngine.InputSystem.Controls.ButtonControl button &&
                    button.wasPressedThisFrame
                )
                {
                    return true;
                }
            }
        }

        return false;
    }
}


class Car
{
    // Identifiers
    public string name {  get; private set; }
    public int id { get; private set; }
    public Transform pos { get; private set; }


    // Utils
    public PlayerLap lap { get; private set; }

    public Car(string n, int i, Transform t, PlayerLap l) { 
        name = n; id = i; pos = t; lap = l;
    }
}
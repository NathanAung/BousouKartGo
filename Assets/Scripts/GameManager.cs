using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [SerializeField] bool debugMode = false; 

    [SerializeField] Transform[] carsPos;


    private List<Car> cars;

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
        foreach (Car car in cars) {
            if(car.lap.lapState == 2)
            {
                car.lap.lapState = 0;
                car.lap.lap += 1;
            }

            if (car.lap.lap == 3)
            {
                // GAME WIN
                Debug.Break();
            }
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
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    [SerializeField] byte toCheckFlag = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" || other.tag == "NPC")
        {
            PlayerLap car = other.GetComponent<PlayerLap>();
            
            if(car == null)
            {
                Debug.LogError("LapCounter: CarObject missing PlayerLap!");
                return;
            }

            if(car.lapState == toCheckFlag)
            {
                car.lapState += 1;
                return;
            }

        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DiceGameManager : MonoBehaviour
{
    public static DiceGameManager Instance { get; private set; }

    public GameObject dicePrefab;
    public Transform diceSpawnPoint;

    private GameObject dice;
    private Rigidbody diceRb;
    private bool isRolling;

    public Text upperSideTxt;

    public Vector3Int directionValues;

    private readonly string[] faceRepresent = {"", "1", "2", "3", "4", "5", "6"};

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InstantiateDice();
    }

    public void OnMatchStateReceived(byte[] data)
    {
        Debug.Log("Match state received. Data length: " + data.Length);
        // Implement the logic to handle match state received
    }

    private void InstantiateDice()
    {
        dice = Instantiate(dicePrefab, diceSpawnPoint.position, UnityEngine.Random.rotation);
        diceRb = dice.GetComponent<Rigidbody>();

        if (diceRb == null)
        {
            Debug.LogError("Rigidbody component missing on dicePrefab.");
        }
    }

    public void RollDice()
    {
        if (isRolling) return;

        if (diceRb == null)
        {
            Debug.LogError("diceRb is not assigned. Make sure dicePrefab has a Rigidbody component.");
            return;
        }

        isRolling = true;
        diceRb.AddForce(Vector3.up * 20f, ForceMode.Impulse);
        diceRb.AddTorque(UnityEngine.Random.insideUnitSphere * 30f, ForceMode.Impulse);
        StartCoroutine(WaitForDiceToStop());
    }

    private IEnumerator WaitForDiceToStop()
    {
        yield return new WaitForSeconds(3);

        if (dice == null)
        {
            Debug.LogError("Dice GameObject is null. Ensure it is instantiated correctly.");
            isRolling = false;
            yield break;
        }

        int diceValue = CalculateDiceValue();

        if (NakamaManager.Instance != null)
        {
            byte[] message = BitConverter.GetBytes(diceValue);
            NakamaManager.Instance.SendMatchState(1, message).Wait();
        }
        else
        {
            Debug.LogError("NakamaManager.Instance is null.");
        }

        isRolling = false;
    }

    private int CalculateDiceValue()
    {
        if (dice == null)
        {
            Debug.LogError("Dice GameObject is null. Cannot calculate value.");
            return -1;
        }

        Vector3 localUp = dice.transform.InverseTransformDirection(Vector3.up);
        Vector3 localForward = dice.transform.InverseTransformDirection(Vector3.forward);
        Vector3 localRight = dice.transform.InverseTransformDirection(Vector3.right);

        float dotUp = Vector3.Dot(localUp, Vector3.up);
        float dotForward = Vector3.Dot(localForward, Vector3.up);
        float dotRight = Vector3.Dot(localRight, Vector3.up);

        if (Mathf.Abs(dotUp) > Mathf.Abs(dotForward) && Mathf.Abs(dotUp) > Mathf.Abs(dotRight))
        {
            return 1; // Y-axis
        }
        else if (Mathf.Abs(dotForward) > Mathf.Abs(dotRight))
        {
            if (dotForward > 0)
            {
                return directionValues.z;
            }
            else
            {
                return directionValues.z == 6 ? 1 : directionValues.z + 1;
            }
        }
        else
        {
            if (dotRight > 0)
            {
                return directionValues.x;
            }
            else
            {
                return directionValues.x == 6 ? 1 : directionValues.x + 1;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rigidBody;
    bool hasLanded;
    bool isDiceThrown;
    
    Vector3 initialPosition;
    int diceValue;

    [SerializeField] DiceSide[] diceSides;
    [SerializeField] int force;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
    }

    void Update()
    {
        if(rigidBody.IsSleeping() && !hasLanded && isDiceThrown)
        {
            //Debug.Log("1");
            hasLanded = true;
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            SideValueCheck();
        }
        else if(rigidBody.IsSleeping() && hasLanded && diceValue == 0)//≈—À» «¿—“–ﬂÀ
        {
            //Debug.Log("2");
            RerollDice();
        }
    }

    public void RollPhysicalDice()
    {
        Reset();
        if (!isDiceThrown && !hasLanded)
        {
            //Debug.Log("3");
            isDiceThrown = true;
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            rigidBody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
            //rigidBody.AddForce(Vector3.up* 50000);//0, Random.Range(400, 500), 0
        }
        //else if (isDiceThrown && hasLanded)
        //{
        //    Debug.Log("4");
        //    Reset();
        //}
    }
    private void Reset()
    {
        transform.position = initialPosition;
        isDiceThrown = false;
        hasLanded = false;
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        //Debug.Log("5");
    }
    void RerollDice()
    {
        Reset();
        isDiceThrown = true;
        rigidBody.useGravity = true;
        rigidBody.isKinematic = false;
        rigidBody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
        //rigidBody.AddForce(0, Random.Range(40000, 50000), 0);
        //Debug.Log("6");
    }

    void SideValueCheck()
    {
        diceValue = 0;
        foreach (var side in diceSides)
        {
            if (side.OnGround)
            {
                diceValue = side.SideValue();
                //Debug.Log("3D  Œ—“» ¬€¡–Œ—»À»: " + diceValue);
                break;
            }
        }
        GameManager.instance.ReportDiceRolled(diceValue);
    }
}

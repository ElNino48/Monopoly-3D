using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceSide : MonoBehaviour
{
    bool onGround;
    public bool OnGround => onGround;

    private void OnTriggerStay(Collider collider) 
    {
        if (collider.CompareTag("Ground"))
        {
            onGround = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    public int SideValue()
    {
        //��� ����� ����� ������� ������� ������ �������, ���������������� ���������   
        int value = Int32.Parse(name);
        return value;
    }
}
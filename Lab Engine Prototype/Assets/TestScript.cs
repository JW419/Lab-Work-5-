using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AVoider;

public class TestScript : MonoBehaviour
{
    public 
    void Start()
    {
        string greeting = Class1.GetGreeting();
        Debug.Log(greeting);
    }
}

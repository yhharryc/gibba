using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 movementInput;
    public Vector2 MovementInput{
        get{return movementInput;}
        set{movementInput=value;}
    }
}

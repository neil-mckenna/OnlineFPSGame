using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TMP_Text overheatedMessage  = null;
    public static UIController instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        
    }

}

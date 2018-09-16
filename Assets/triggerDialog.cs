using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class triggerDialog : MonoBehaviour {

    public Text uiText;
    public string texto= "";
    public string texto2 = "";
    private bool activo = false;
    private bool itsBeenActivated = false;

    
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player" && uiText.text.Equals(""))
        {
            if (itsBeenActivated && !activo)
            {
                uiText.text = texto2;
                activo = true;
            }
            else if(uiText.text.Equals(""))
            {
                uiText.text = texto;
                activo = true;
                itsBeenActivated = true;
            }
           
        }

    }
    // Use this for initialization
    void Start () {
		
	}

    IEnumerator Fade()
    {
        
       yield return new WaitForSeconds(2f);
        
        uiText.text = "";
        activo = false;
    }
    // Update is called once per frame
    void Update () {
        if (activo)
        {       
            StartCoroutine(Fade());
            activo = false;
        }
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{

    public Slider maxXVelocityMagnitude;
    public Slider maxJumpMagnitude;
    public Slider maxGravityMagnitude;
    public Slider maxAscendingSlope;
    public Slider groundAcceleration;
    public Slider airAcceleration;
    public InputField gravityX;
    public InputField gravityY;

    void Start()
    {
        LoadDefaultValues();
        LoadPrefs();
    }

    public void LoadDefaultValues()
    {
        maxXVelocityMagnitude.value = 10f;
        maxJumpMagnitude.value = 12f;
        maxGravityMagnitude.value = 30f;
        maxAscendingSlope.value = 30f;
        groundAcceleration.value = 20f;
        airAcceleration.value = 25f;
        gravityX.text = "0";
        gravityY.text = "-25";
    }

    public void LoadPrefs()
    {
        if (PlayerPrefs.HasKey("maxHorizontalVelocity"))
        {
            maxXVelocityMagnitude.value = PlayerPrefs.GetFloat("maxHorizontalVelocity");
        }
        if (PlayerPrefs.HasKey("maxJumpMagnitude"))
        {
            maxJumpMagnitude.value = PlayerPrefs.GetFloat("maxJumpMagnitude");
        }
        if (PlayerPrefs.HasKey("maxGravityMagnitude"))
        {
            maxGravityMagnitude.value = PlayerPrefs.GetFloat("maxGravityMagnitude");
        }
        if (PlayerPrefs.HasKey("maxAscendingSlope"))
        {
            maxAscendingSlope.value = PlayerPrefs.GetFloat("maxAscendingSlope");
        }
        if (PlayerPrefs.HasKey("speedAccelerationOnGround"))
        {
            groundAcceleration.value = PlayerPrefs.GetFloat("speedAccelerationOnGround");
        }
        if (PlayerPrefs.HasKey("speedAccelerationOnAir"))
        {
            airAcceleration.value = PlayerPrefs.GetFloat("speedAccelerationOnAir");
        }
        if (PlayerPrefs.HasKey("gravityX"))
        {
            gravityX.text = PlayerPrefs.GetFloat("gravityX").ToString();
        }
        if (PlayerPrefs.HasKey("gravityY"))
        {
            gravityY.text = PlayerPrefs.GetFloat("gravityY").ToString();
        }
    }

    public void SaveAndExit()
    {
        PlayerPrefs.SetFloat("maxHorizontalVelocity", maxXVelocityMagnitude.value);
        PlayerPrefs.SetFloat("maxJumpMagnitude", maxJumpMagnitude.value);
        PlayerPrefs.SetFloat("maxGravityMagnitude", maxGravityMagnitude.value);
        PlayerPrefs.SetFloat("maxAscendingSlope", maxAscendingSlope.value);
        PlayerPrefs.SetFloat("speedAccelerationOnGround", groundAcceleration.value);
        PlayerPrefs.SetFloat("speedAccelerationOnAir", airAcceleration.value);
        PlayerPrefs.SetFloat("gravityX", float.Parse(gravityX.text));
        PlayerPrefs.SetFloat("gravityY", float.Parse(gravityY.text));
        Exit();
    }

    public void Exit()
    {
        SceneManager.LoadScene("Game");
    }
}

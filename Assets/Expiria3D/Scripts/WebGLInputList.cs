using UnityEngine;
using TMPro;

public class WebGLInputList : MonoBehaviour
{
    public TMP_InputField[] inputFields = new TMP_InputField[0]; // Array of TMP InputFields
    private TouchScreenKeyboard[] keyboards = new TouchScreenKeyboard[0]; // Array of TouchScreenKeyboards

    void Start()
    {
        // Initialize the keyboards array to the same length as inputFields
        keyboards = new TouchScreenKeyboard[inputFields.Length];
    }

    void Update()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            // Check if the current input field is focused and the keyboard is not already open
            if (inputFields[i].isFocused && (keyboards[i] == null || !keyboards[i].active))
            {
                // Open the soft keyboard for this input field
                keyboards[i] = TouchScreenKeyboard.Open(inputFields[i].text, TouchScreenKeyboardType.Default);
            }

            // Sync the input field with the keyboard input (in case keyboard is used)
            if (keyboards[i] != null && keyboards[i].active)
            {
                inputFields[i].text = keyboards[i].text;
            }
        }
    }
}
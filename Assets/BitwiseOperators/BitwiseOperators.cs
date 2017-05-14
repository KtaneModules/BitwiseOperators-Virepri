using System;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

public class BitwiseOperators : MonoBehaviour
{
    /*
     * 0 - AND
     * 1 - OR
     * 2 - XOR
     * 3 - NOT
     */
    static string[] Operations = { "AND", "OR", "XOR", "NOT" };

    byte solution;
    byte currentDisplay = 0x00;
    bool isActivated;

    public TextMesh OperatorScreen;
    public TextMesh[] AnswerScreen;
    public KMSelectable Submit;
    public KMSelectable[] Inputs;
    public KMBombInfo BombInfo;

    int moduleId;
    static int moduleIdCounter = 1;

    void Start()
    {
        moduleId = moduleIdCounter++;

        Submit.OnInteract = delegate () { OnSubmit(); return false; };
        for (int i = 0; i < Inputs.Length; i++)
        {
            int j = i;
            Inputs[i].OnInteract = delegate () { OnScreenManip(j); return false; };
        }

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        isActivated = true;

        var b1 = getByte1();
        var b2 = getByte2();
        var operation = Random.Range(0, 4);

        OperatorScreen.text = Operations[operation];
        SetAnswerScreen();

        switch (operation)
        {
            case 0:
                solution = (byte) (b1 & b2);
                break;
            case 1:
                solution = (byte) (b1 | b2);
                break;
            case 2:
                solution = (byte) (b1 ^ b2);
                break;
            case 3:
                solution = (byte) (~b1);
                break;
        }

        Debug.LogFormat("[Bitwise Operators #{0}] Solution is: {1}", moduleId, Convert.ToString(solution, 2).PadLeft(8, '0'));
    }

    private void SetAnswerScreen()
    {
        for (int i = 0; i < 8; i++)
            AnswerScreen[i].text = (currentDisplay & (1 << (7 - i))) == 0 ? "0" : "1";
    }

    void OnScreenManip(int button)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.LogFormat("[Bitwise Operators #{0}] Pressed button before module has been activated.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            currentDisplay ^= (byte) (1 << (7 - button));
            SetAnswerScreen();
        }
    }

    void OnSubmit()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.LogFormat("[Bitwise Operators #{0}] Pressed button before module has been activated.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            Debug.LogFormat("[Bitwise Operators #{0}] Answer given is {1}.", moduleId, Convert.ToString(currentDisplay, 2).PadLeft(8, '0'));
            if (currentDisplay == solution)
            {
                Debug.LogFormat("[Bitwise Operators #{0}] Module solved.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
            }
            else
            {
                Debug.LogFormat("[Bitwise Operators #{0}] Incorrect answer.", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }

    byte getByte1()
    {
        string o = "";
        o += BombInfo.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.AA) == 0 ? '1' : '0';
        o += BombInfo.GetPortCount(KMBombInfoExtensions.KnownPortType.Parallel) != 0 ? '1' : '0';
        o += BombInfo.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.NSA) ? '1' : '0';
        o += BombInfo.GetModuleNames().Count > (BombInfo.GetTime() / 60) ? '1' : '0';
        o += BombInfo.GetOnIndicators().Count() > 1 ? '1' : '0';
        o += BombInfo.GetModuleNames().Count % 3 == 0 ? '1' : '0';
        o += BombInfo.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.D) < 2 ? '1' : '0';
        o += BombInfo.GetPortCount() < 4 ? '1' : '0';

        Debug.LogFormat("[Bitwise Operators #{0}] First byte: {1}", moduleId, o);
        return Convert.ToByte(o, 2);
    }

    byte getByte2()
    {
        string o = "";
        o += BombInfo.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.D) >= 1 ? '1' : '0';
        o += BombInfo.GetPortCount() >= 3 ? '1' : '0';
        o += BombInfo.GetBatteryHolderCount() >= 2 ? '1' : '0';
        o += BombInfo.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.BOB) ? '1' : '0';
        o += BombInfo.GetOffIndicators().Count() > 1 ? '1' : '0';
        o += BombInfo.GetSerialNumberNumbers().Last() % 2 != 0 ? '1' : '0';
        o += BombInfo.GetModuleNames().Count % 2 == 0 ? '1' : '0';
        o += BombInfo.GetBatteryCount() >= 2 ? '1' : '0';

        Debug.LogFormat("[Bitwise Operators #{0}] Second byte: {1}", moduleId, o);
        return Convert.ToByte(o, 2);
    }
}

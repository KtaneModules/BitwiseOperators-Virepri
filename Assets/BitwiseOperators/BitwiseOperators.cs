using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

public class BitwiseOperators : MonoBehaviour {
    byte b1;
    byte b2;
    int Operation;
    /*
     * 0 - AND
     * 1 - OR
     * 2 - XOR
     * 3 - NOT
     */
    string[] Operations = {"AND","OR","XOR","NOT"};
    byte solution;
    byte osdat = 0x00;
    bool isActivated;
    
    public TextMesh bos;
    public TextMesh bss;
    public KMSelectable Submit;
    public KMSelectable[] Inputs;
    public KMBombInfo kmbi;

	void Start ()
    {
        
        Submit.OnInteract = delegate () { OnSubmit(); return false; };
        for (int i = 0; i < Inputs.Length; i++)
        {
            int ReallyLocal = i;
            Inputs[i].OnInteract = delegate () { OnScreenManip(ReallyLocal); return false; };
        }

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
	}

    string GetScreen(byte dat, bool os)
    {
        //dat - what we're outputting
        //os - whether we're getting the data input screen or the output screen(s)
        string o = Convert.ToString(dat, 2).PadLeft(8, '0');
        if (os)
        {
            string to = "";
            to = o.Substring(0, 4) + " " + o.Substring(4,4);
            return to;
        }
        else
        {
            string to = "";
            foreach(char c in o)
            {
                to += c.ToString() + " ";
            }
            return to;
        }
    }

    void ActivateModule()
    {
        isActivated = true;
        
        b1 = db1();
        b2 = db2();
        Operation = UnityEngine.Random.Range(0, 4);
        
        bos.text = Operations[Operation];
        bss.text = GetScreen(osdat, false);

        switch (Operation)
        {
            case 0:
                solution = (byte)(b1 & b2);
                break;
            case 1:
                solution = (byte)(b1 | b2);
                break;
            case 2:
                solution = (byte)(b1 ^ b2);
                break;
            case 3:
                solution = (byte)(~b1);
                break;
        }
    }
	
    void OnScreenManip(int button)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.Log("Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            StringBuilder tmp = new StringBuilder(Convert.ToString(osdat, 2).PadLeft(8, '0'));
            tmp[button] = (tmp[button].Equals('0')) ? '1' : '0';
            osdat = Convert.ToByte(tmp.ToString(), 2);
            bss.text = GetScreen(osdat, false);
        }
    }

    void OnSubmit()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.Log("Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            Debug.Log("Solution is " + Convert.ToString(solution, 2).PadLeft(8,'0') + " answer is " + Convert.ToString(osdat,2).PadLeft(8,'0'));
            if(osdat.Equals(solution))
            {
                Debug.Log("Correctly performed bitwise op");
                GetComponent<KMBombModule>().HandlePass();
            } else
            {
                Debug.Log("Incorrectly performed bitwise op");
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }

    byte db1()
    {
        string o = "";

        o += kmbi.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.AA) == 0 ? '1' : '0';
        o += kmbi.GetPortCount(KMBombInfoExtensions.KnownPortType.Parallel) != 0 ? '1' : '0';
        o += kmbi.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.NSA) ? '1' : '0';
        o += kmbi.GetModuleNames().Count > (kmbi.GetTime()/60) ? '1' : '0';
        o += (new List<string>(kmbi.GetOnIndicators())).Count > 1 ? '1' : '0';
        o += kmbi.GetModuleNames().Count % 3 == 0 ? '1' : '0';
        o += kmbi.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.D) < 2 ? '1' : '0';
        o += kmbi.GetPortCount() < 4 ? '1' : '0';
        

        return Convert.ToByte(o,2);
    }

    byte db2()
    {
        string o = "";

        o += kmbi.GetBatteryCount(KMBombInfoExtensions.KnownBatteryType.D) >= 1 ? '1' : '0';
        o += kmbi.GetPortCount() >= 3 ? '1' : '0';
        o += kmbi.GetBatteryHolderCount() >= 2 ? '1' : '0';
        o += kmbi.IsIndicatorOn(KMBombInfoExtensions.KnownIndicatorLabel.BOB) ? '1' : '0';
        o += (new List<string>(kmbi.GetOffIndicators())).Count > 1 ? '1' : '0';
        List<int> SNums = new List<int>(kmbi.GetSerialNumberNumbers());
        o += SNums[SNums.Count - 1] % 2 != 0 ? '1' : '0';
        o += kmbi.GetModuleNames().Count % 2 == 0 ? '1' : '0';
        o += kmbi.GetBatteryCount() >= 2 ? '1' : '0';

        return Convert.ToByte(o, 2);
    }
}

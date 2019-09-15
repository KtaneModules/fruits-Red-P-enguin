using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class fruitsScript : MonoBehaviour {
    public KMBombInfo bomb;
    public KMAudio audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;
    private bool incorrect = false;

    public KMSelectable[] buttons;
    public Renderer[] buttonRends;
    public Sprite[] fruits;
    public SpriteRenderer[] fruitRends;
    public Material[] mats;

    private int index = 0;
    private List<int> indexTracker = new List<int>();

    private KMSelectable submit;
    private KMSelectable press;
    private int pressPos;
    private int timesPressed;

    int[,] chartNums = new int[4, 4] {{ 1,2,3,4 },
                                        { 4,3,2,1},
                                        { 3,1,4,2},
                                        { 2,4,1,3}};

    // Use this for initialization
    void Start () {
        init();
    }
	
    void init()
    {
        for (int i = 0; i < 4; i++)
        {
            index = UnityEngine.Random.Range(0, 4);
            indexTracker.Add(index);
            buttonRends[i].material = mats[indexTracker[i]];
            fruitRends[i].sprite = fruits[indexTracker[i]];
            DebugMsg("Button " + (i + 1) + "'s label is " + fruitRends[i].sprite.name + ".");
        }
        submit = buttons[(chartNums[indexTracker[0], indexTracker[2]] + chartNums[indexTracker[1], indexTracker[3]]) % 4];
        DebugMsg("The submit button is " + submit.name + ".");
        press = buttons[bomb.GetSerialNumberNumbers().Sum() % 4];
        if (press == submit)
        {
            for (int i = 0; i < 4; i++)
            {
                if (buttons[i] == submit)
                {
                    press = buttons[(i + 1) % 4];
                    i = 4;
                }
            }
        }
        DebugMsg("The button you need to press is " + press.name + ".");
        for (int i = 0; i < 4; i++)
        {
            if (buttons[i] == press)
            {
                pressPos = i + 1;
                i = 4;
            }
        }
        DebugMsg("You need to press the button " + (bomb.GetOffIndicators().Count() + pressPos) + " times.");
    }

	// Update is called once per frame
	void Awake () {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { buttonPressed(pressedButton); return false; };
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);

        incorrect = false;

        if (moduleSolved)
        {
            return;
        }
        else
        {
            if(pressedButton == buttons[4])
            {
                timesPressed = 0;
            }
            if(pressedButton == press)
            {
                timesPressed++;
            }
            if (pressedButton == submit)
            {
                if(timesPressed != bomb.GetOffIndicators().Count() + pressPos)
                {
                    incorrect = true;
                }
                if (!incorrect)
                {
                    DebugMsg("Module solved!");
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                }
                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    DebugMsg("Strike! The button you need to press was pressed " + timesPressed + " times, while the number of presses you needed was " + (bomb.GetOffIndicators().Count() + pressPos) + " times.");
                    incorrect = false;
                    timesPressed = 0;
                    indexTracker.Clear();
                    init();
                }
            }
        }
    }

    private bool isCommandValid(string cmd)
    {
        string[] validbtns = { "1","2","3","4" };

        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        foreach (var btn in parts)
        {
            if (!validbtns.Contains(btn.ToLower()))
            {
                return false;
            }
        }
        return true;
    }

    public string TwitchHelpMessage = "Use !{0} press 1 13 to press the first button 13 times.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (isCommandValid(cmd))
        {
            yield return null;
            for (int i = 0; i < parts.Count(); i++)
            {
                if (parts[i] == "1")
                {
                    yield return new KMSelectable[] { buttons[0] };
                }
                else if (parts[i] == "2")
                {
                    yield return new KMSelectable[] { buttons[1] };
                }
                else if (parts[i] == "3")
                {
                    yield return new KMSelectable[] { buttons[2] };
                }
                else if (parts[i] == "4")
                {
                    yield return new KMSelectable[] { buttons[3] };
                }
            }
        }
        else if (parts.Length == 1 && parts[0].ToLower() == "reset")
        {
            yield return null;
            yield return new KMSelectable[] { buttons[4] };
        }
        else
        {
            yield break;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Module Movements #{0}] {1}", ModuleId, msg);
    }
}

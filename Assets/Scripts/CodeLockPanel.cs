using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Put this on an object that is ALWAYS active (e.g. the Canvas), not on the popup itself.
/// Handles the keypad popup: digit entry, checking the code, and feedback.
public class CodeLockPanel : MonoBehaviour
{
    [Header("Popup")]
    [Tooltip("The popup object that gets shown/hidden (the full-screen dimmer with the keypad inside).")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text displayText;

    [Header("Buttons")]
    [Tooltip("Element 0 = the '0' button, element 1 = '1', ... element 9 = '9'.")]
    [SerializeField] private Button[] digitButtons = new Button[10];
    [SerializeField] private Button backspaceButton;
    [SerializeField] private Button closeButton;
    [Tooltip("Optional. If set, the code is only checked when this is pressed. If empty, it is checked automatically after the last digit.")]
    [SerializeField] private Button enterButton;

    [Header("Feedback colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color correctColor = new Color(0.3f, 0.9f, 0.4f);
    [SerializeField] private Color wrongColor = new Color(1f, 0.3f, 0.3f);

    /// True while the keypad is on screen. Controllers use this to freeze movement.
    public static bool IsOpen { get; private set; }

    private CombinationLock currentLock;
    private string entered = "";
    private bool busy; // true while showing the right/wrong feedback

    private void Awake()
    {
        for (int i = 0; i < digitButtons.Length; i++)
        {
            int digit = i; // copy, so each button remembers its own number
            if (digitButtons[i] != null)
                digitButtons[i].onClick.AddListener(() => AddDigit(digit));
        }

        if (backspaceButton != null) backspaceButton.onClick.AddListener(Backspace);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (enterButton != null) enterButton.onClick.AddListener(Submit);

        panelRoot.SetActive(false);
    }

    private void OnDisable()
    {
        IsOpen = false;
    }

    private void Update()
    {
        if (!IsOpen || busy) return;

        // PC testing: type the digits on the keyboard
        foreach (char c in Input.inputString)
        {
            if (c >= '0' && c <= '9') AddDigit(c - '0');
        }

        if (Input.GetKeyDown(KeyCode.Backspace)) Backspace();
        if (Input.GetKeyDown(KeyCode.Escape)) Close();

        if (enterButton != null && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            Submit();
    }

    public void Show(CombinationLock lockToOpen)
    {
        currentLock = lockToOpen;
        entered = "";
        busy = false;
        displayText.color = normalColor;
        Refresh();

        panelRoot.SetActive(true);
        IsOpen = true;
    }

    private void Close()
    {
        if (busy) return;

        panelRoot.SetActive(false);
        IsOpen = false;
        currentLock = null;
    }

    private void AddDigit(int digit)
    {
        if (!IsOpen || busy) return;
        if (entered.Length >= currentLock.CodeLength) return;

        entered += digit;
        Refresh();

        // With no Enter button, check automatically after the last digit
        if (enterButton == null && entered.Length == currentLock.CodeLength)
            StartCoroutine(CheckCode());
    }

    // Enter pressed: an incomplete code simply counts as wrong
    private void Submit()
    {
        if (!IsOpen || busy || entered.Length == 0) return;
        StartCoroutine(CheckCode());
    }

    private void Backspace()
    {
        if (!IsOpen || busy || entered.Length == 0) return;

        entered = entered.Substring(0, entered.Length - 1);
        Refresh();
    }

    private IEnumerator CheckCode()
    {
        busy = true;
        CombinationLock target = currentLock;

        yield return new WaitForSeconds(0.2f); // let the player see the last digit

        bool correct = target.CheckCode(entered);
        displayText.color = correct ? correctColor : wrongColor;
        yield return new WaitForSeconds(0.6f);

        displayText.color = normalColor;
        entered = "";
        busy = false;

        if (correct)
        {
            Close();
            target.OpenCabinet();
        }
        else
        {
            Refresh();
        }
    }

    // Shows e.g. "4 2 _ _"
    private void Refresh()
    {
        int length = currentLock.CodeLength;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(i < entered.Length ? entered[i] : '_');
        }

        displayText.text = sb.ToString();
    }
}

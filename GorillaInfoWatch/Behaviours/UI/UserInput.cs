using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.UserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI;

internal class UserInput : MonoBehaviour
{
    public static UserInput Instance { get; private set; }

    public Panel Panel;

    private UserInputBoard _board;

    private GameObject _standardBoardRoot, _advancedBoardRoot;

    private TMP_Text _textBox;

    private string _input;

    private int _limit;

    private EventHandler<UserInputArgs> _submit;

    private readonly List<KeyboardButton> _keys = [];

    private bool _useSpecialCharacters;

    private bool _useTypingTimer;

    private float _typingTimer;

    private string _typedText = string.Empty;

    public void Awake()
    {
        if (Instance.Exists() && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        _standardBoardRoot = transform.Find("Canvas/StandardKeyboard").gameObject;
        Transform standardRoot = _standardBoardRoot.transform.Find("Keys");
        _advancedBoardRoot = transform.Find("Canvas/AdvancedKeyboard").gameObject;
        Transform advancedRoot = _advancedBoardRoot.transform.Find("Keys");

        foreach (UserInputBinding binding in Enum.GetValues(typeof(UserInputBinding)).Cast<UserInputBinding>())
        {
            Transform child = standardRoot?.Find(binding.ToString());
            if (child.Exists())
            {
                KeyboardButton key = child.GetComponentInChildren<Collider>().gameObject.AddComponent<KeyboardButton>();
                key.Binding = binding;
                key.IsLargeKey = binding.IsFunctionKey();
                _keys.Add(key);
            }

            child = advancedRoot?.Find(binding.ToString());
            if (child.Exists())
            {
                if (binding == UserInputBinding.Shift)
                {
                    child.GetComponentInChildren<Collider>().gameObject.AddComponent<ShiftButton>();
                    continue;
                }

                KeyboardButton key = child.GetComponentInChildren<Collider>().gameObject.AddComponent<KeyboardButton>();
                key.Binding = binding;
                key.IsLargeKey = binding.IsFunctionKey();
                _keys.Add(key);
            }
        }

        KeyboardButton.OnKeyClicked += ProcessBinding;
        ShiftButton.ShiftToggled += ProcessShift;

        _textBox = transform.Find("Canvas/TextBox/Text").GetComponent<TMP_Text>();
        _textBox.text = string.Empty;
    }

    public void Update()
    {
        if (_useTypingTimer)
        {
            _typingTimer += Time.unscaledDeltaTime;
            if (_typingTimer > 1f)
            {
                if (_typedText != _input)
                {
                    _typedText = _input;
                    _submit?.Invoke(this, new()
                    {
                        Input = _input,
                        IsTyping = true
                    });
                }
                _typingTimer = 0f;
            }
            return;
        }

        _typingTimer = 0f;
        _typedText = string.Empty;
    }

    public static void Activate(string input, UserInputBoard board, int limit = int.MaxValue, EventHandler<UserInputArgs> submit = null, UserInput userInput = null)
    {
        UserInput instance = userInput ?? Instance;
        instance?.Activate(input, board, limit, submit);
    }

    public void Activate(string input, UserInputBoard board, int limit = int.MaxValue, EventHandler<UserInputArgs> submit = null)
    {
        _board = board;
        _standardBoardRoot.SetActive(board == UserInputBoard.Standard);
        _advancedBoardRoot.SetActive(board == UserInputBoard.Advanced);

        _input = (input ?? string.Empty).LimitLength(limit);
        _textBox.text = input;
        _limit = limit;
        _submit = submit;
        _useTypingTimer = true;

        gameObject.SetActive(true);
        Panel.SetActive(false);
        ProcessShift(false);

        transform.position = Panel.transform.position;
        transform.eulerAngles = Panel.transform.eulerAngles;
        transform.localScale = Panel.transform.localScale * 1.2f;
    }

    public void ProcessBinding(UserInputBinding binding)
    {
        if (binding == UserInputBinding.Return)
        {
            _useTypingTimer = false;
            Panel.SetActive(true);
            gameObject.SetActive(false);

            return;
        }

        if (binding == UserInputBinding.Enter)
        {
            _useTypingTimer = false;
            _submit?.Invoke(this, new()
            {
                Input = _input,
                IsTyping = false
            });
            Panel.SetActive(true);
            gameObject.SetActive(false);

            return;
        }

        if (binding == UserInputBinding.Delete)
        {
            if (_input.Length > 0)
            {
                _input = _input[..^1];
                _textBox.text = _input;
            }
            return;
        }

        _input = (_input + (_useSpecialCharacters ? binding.ToSpecialChar() : ((binding.IsLetterKey() && (_board == UserInputBoard.Standard || _useSpecialCharacters)) ? char.ToUpper(binding.ToChar()) : char.ToLower(binding.ToChar())))).LimitLength(_limit);
        _textBox.text = _input;
    }

    public void ProcessShift(bool useSpecialCharacters)
    {
        _useSpecialCharacters = useSpecialCharacters;

        foreach (var key in _keys)
        {
            if (key.Binding.IsFunctionKey()) return;

            if (key.Binding.IsLetterKey())
            {
                key.Text.text = ((_board == UserInputBoard.Standard || useSpecialCharacters) ? char.ToUpper(key.Binding.ToChar()) : char.ToLower(key.Binding.ToChar())).ToString();
                continue;
            }

            key.Text.text = (useSpecialCharacters ? key.Binding.ToSpecialChar() : key.Binding.ToChar()).ToString();
        }
    }
}

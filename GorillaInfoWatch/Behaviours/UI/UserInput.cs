using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.UserInput;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI;

internal class UserInput : MonoBehaviour
{
    public static UserInput Instance { get; private set; }

    public Panel Panel;

    private TMP_Text _textBox;

    private string _input;

    private Action _submit;

    public void Awake()
    {
        if (Instance.Exists() && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        Transform keyRoot = transform.Find("Canvas/Keyboard/Keys");

        foreach (UserInputBinding binding in Enum.GetValues(typeof(UserInputBinding)).Cast<UserInputBinding>())
        {
            Transform child = keyRoot?.Find(binding.ToString());
            if (child.Null()) continue;

            Key key = child.GetComponentInChildren<Collider>().gameObject.AddComponent<Key>();
            key.Binding = binding;
            key.IsLargeKey = binding.IsFunctionKey();
        }

        Key.OnKeyClicked += ProcessBinding;

        _textBox = transform.Find("Canvas/TextBox/Text").GetComponent<TMP_Text>();
        _textBox.text = string.Empty;
    }

    public static void Activate(string input, Action submit, UserInput instance = null)
    {
        instance ??= Instance;
        instance.Activate(input, submit);
    }

    public void Activate(string input, Action submit)
    {
        _input = input;
        _textBox.text = input;
        _submit = submit;
        Panel.SetActive(false);

        gameObject.SetActive(true);
        transform.position = Panel.transform.position;
        transform.eulerAngles = Panel.transform.eulerAngles;
        transform.localScale = Panel.transform.localScale * 1.5f;
    }

    public void ProcessBinding(UserInputBinding binding)
    {
        if (binding == UserInputBinding.Return)
        {
            Panel.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        if (binding == UserInputBinding.Enter)
        {
            _submit?.SafeInvoke();
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

        _input += binding.RootBeer();
        _textBox.text = _input;
    }
}

using GorillaInfoWatch.Models.UserInput;

namespace GorillaInfoWatch.Extensions;

public static class UserInputExtensions
{
    public static bool IsNumericKey(this UserInputBinding binding) => binding >= UserInputBinding.Zero && binding <= UserInputBinding.Nine;

    public static bool IsFunctionKey(this UserInputBinding binding) => binding >= UserInputBinding.Return;

    public static bool TryParseNumber(this UserInputBinding binding, out int number)
    {
        if (IsNumericKey(binding))
        {
            number = (int)binding;
            return true;
        }

        number = -1;
        return false;
    }

    public static string RootBeer(this UserInputBinding binding) => binding.TryParseNumber(out int number) ? number.ToString() : (binding == UserInputBinding.Space ? " " : binding.ToString());

}

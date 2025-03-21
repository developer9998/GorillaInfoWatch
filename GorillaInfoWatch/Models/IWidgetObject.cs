using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public interface IWidgetObject
    {
        int ObjectFits(GameObject game_object);

        void CreateObject(MenuLine line, out GameObject game_object);

        void ModifyObject(GameObject game_object);
    }
}
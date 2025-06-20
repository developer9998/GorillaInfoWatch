using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Models.Commands
{
    public abstract class Command_Base : MonoBehaviour
    {
        public abstract string Title { get; }
        public virtual string Description { get; set; }
    }
}

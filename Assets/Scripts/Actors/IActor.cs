using UnityEngine;

namespace Actors
{
    public interface IActor
    {
        public GameObject thisObject { get; set; }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hydra
{
    [CreateAssetMenu]
    public class PoolSet : ScriptableObject {

        public List<Pool> pools = new List<Pool>();

    }
}

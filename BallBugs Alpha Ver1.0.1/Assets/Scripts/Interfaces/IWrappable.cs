using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public interface IWrappable
{
    void Wrap(RuntimeAnimatorController wrappedBug, float seconds);
}

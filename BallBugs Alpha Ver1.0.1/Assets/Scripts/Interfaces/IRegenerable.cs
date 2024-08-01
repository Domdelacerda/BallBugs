using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegenerable
{
    void Regenerate(int healPerInterval, float interval, int numIntervals);
}

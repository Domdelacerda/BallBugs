using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoisonable
{
    void Poison(int damagePerInterval, float interval, int numIntervals);
}

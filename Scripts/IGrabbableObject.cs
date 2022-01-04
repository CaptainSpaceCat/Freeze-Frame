using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbableObject
{
    void OnGrab();
    void OnRelease();
}

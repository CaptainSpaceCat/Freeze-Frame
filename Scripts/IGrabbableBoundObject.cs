using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbableBoundObject
{
    void setGrabbed(bool state, Vector3 playerOffset);
}

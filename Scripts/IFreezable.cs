using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFreezable
{
    Frame getFrame();
    void setFrame(Frame f);
    void setFrozen(bool state);
    void setEnabled(bool state);
}

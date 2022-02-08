using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCockpit : BlockBehavior
{

    public override void Start()
    {
        base.Start();

        //On assigne le gameobject cockpit au vaisseau
        selfShip.blockCockpit = this.gameObject;
    }
}

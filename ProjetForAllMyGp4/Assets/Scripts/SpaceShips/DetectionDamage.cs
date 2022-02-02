using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionDamage : MonoBehaviour
{

    #region references
    private CompositeCollider2D selfCollider;
    private SpaceShipBehavior selfShip;
    private Transform shipTransform;
    #endregion

    private void Awake()
    {
        selfCollider = gameObject.GetComponent<CompositeCollider2D>();
        selfShip = gameObject.GetComponent<SpaceShipBehavior>();
        shipTransform = gameObject.transform;
    }

    void Update()
    {
       
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (selfShip != null)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                Vector3 closestPoint = collision.ClosestPoint(transform.position);
                //On cherche quels block sont touché
                GameObject blockTuch = FindNearestGameObject(closestPoint);

                Debug.Log(collision.gameObject.name);
                
                if(blockTuch.GetComponent(typeof(BlockBehavior)) != null)
                {
                    (blockTuch.GetComponent(typeof(BlockBehavior)) as BlockBehavior).HitSignal(0f);
                }
            }

        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        if (selfShip != null)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                //On désacitve le collider
                collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;

                //On cherche quels block sont touché
                GameObject blockTuch = FindNearestGameObject(collision.contacts[0].point);

                Debug.Log(collision.gameObject.name);

                if (blockTuch.GetComponent(typeof(BlockBehavior)) != null)
                {
                    (blockTuch.GetComponent(typeof(BlockBehavior)) as BlockBehavior).HitSignal(0f);
                }
            }

        }
    }*/

    //Donne le block le plus proche touché ( parcours chaque block et fais Position du block - position du tir), parcours classique peut-être optimiser
    private GameObject FindNearestGameObject(Vector3 pos)
    {
        Vector3 posVector3 = pos;
        float length;
        float smallestLength = float.MaxValue;

        GameObject blockTuch = null;
        foreach (GameObject key in selfShip.allBlocks.Values)
        {
            length = (key.transform.position - posVector3).magnitude;
            if(length < smallestLength)
            {
                smallestLength = length;
                blockTuch = key;
            }
        }

        return blockTuch;
    }

}

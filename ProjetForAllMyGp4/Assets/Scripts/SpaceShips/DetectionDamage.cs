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

    private Transform over;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (selfShip != null)
        {
            if (collision.gameObject.tag == "Bullet")
            {
                over = collision.transform;

                //Il faudrait addapter la size, au tir
                Collider2D[] overlaps = Physics2D.OverlapBoxAll(collision.transform.position,new Vector2(1,0.5f),0);
                Collider2D tuchObject = null;

               for(int i =0; i< overlaps.Length; i++)
               {
                    if(overlaps[i].GetComponent(typeof(BlockBehavior)) != null)
                    {
                        tuchObject = overlaps[i];
                        break;
                    }
               }
                
                if(tuchObject != null)
                {
                    if (tuchObject.GetComponent(typeof(BlockBehavior)) != null)
                    {
                        (tuchObject.GetComponent(typeof(BlockBehavior)) as BlockBehavior).HitSignal(0f);
                    }
                }
            }

        }
    }

    public void OnDrawGizmos()
    {
        if(over != null)
        {
            Gizmos.DrawCube(over.transform.position, new Vector3(1,0.5f,1));
        }
    }


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

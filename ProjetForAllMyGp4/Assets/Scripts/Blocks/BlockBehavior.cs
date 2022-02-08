using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum direction
{
    up,
    down,
    left,
    right
}

public enum NeighboorDirection
{
    South,
    North,
    West,
    East
}
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(BoxCollider2D))]
public class BlockBehavior : MonoBehaviour //Parent of all blocks
{
    #region param�tres modifiable
    [Header("Param�tre du block de base")]
    [Range(1,float.MaxValue)]
    public float block_LifePoint;
    public direction block_direction;
    #endregion

    #region param�tres interne
    [HideInInspector]
    public int id_block;
    protected static int id_toAdd;
    protected bool isAttachToMainShip = false;

    //Les clefs sont : nord, sud, est, ouest, en valeurs on a les scripts parents de tous les voisins
    public Dictionary<NeighboorDirection, BlockBehavior> neighbourBlocks;
    public (int _x, int _y) positionInArray_block;

    //Permet de v�rifier quand un block est connect� au cockpit
    public bool isConnected = true;
    #endregion

    #region r�f�rences

    [HideInInspector]
    public BoxCollider2D selfCollider;

    [HideInInspector]
    public SpaceShipBehavior selfShip;

    [HideInInspector]
    public SpriteRenderer selfRenderer;

    #endregion

    public void Awake()
    {
        neighbourBlocks = new Dictionary<NeighboorDirection, BlockBehavior>();

        //Ajoute les r�f�rences � la main
        selfCollider = gameObject.GetComponent<BoxCollider2D>();
        selfShip = GetComponentInParent<SpaceShipBehavior>();
        selfRenderer = gameObject.GetComponent<SpriteRenderer>();

    }

    public virtual void Start()
    {
        //Cherche som emplacement dans le tableau
        //Pour faire �a, on divise la local position du block, avec la taille de son sprite, pour nous donner le pas entre chaque bloque
        positionInArray_block = (Mathf.CeilToInt(transform.localPosition.x / (selfRenderer.bounds.size.x)), Mathf.CeilToInt(transform.localPosition.y / (selfRenderer.bounds.size.y)));

        //Cr�er un id unique pour le block
        id_block = id_toAdd;
        gameObject.name = id_block.ToString();

        //on l'ajoute au dictionnaire du comportement de spaceship
        if (selfShip != null)
            selfShip.allBlocks.Add(id_block, gameObject);

        id_toAdd++;

        //Trouve les voisins du block
        neighbourBlocks = FindBlockNeighbour();

    }

    //Ne pas mettre grand chose dans l'update, �a sera appel� dans chaque block
    private void Update()
    {

    }

    protected void CalculOwnVelocity()
    {

    }

    //Est appel� quand un tir est touch� par le block
    public void HitSignal(float damage)
    {
        TakeDamage(damage);
    }

    //Enl�ve la vie du block
    public void TakeDamage(float damage)
    {
        block_LifePoint -= 100;
        if(block_LifePoint <= 0)
        {
            DestroyBlock();
        }
    }

    private void DestroyBlock()
    {
        if (isConnected)
        {
            //Met � jour la liste de tous les blocks du vaisseau
            selfShip.allBlocks.Remove(id_block);

            //Met a jour le tableau contenant tous les blocksBehaviors
            selfShip.allBlocksBehavior[positionInArray_block._x, positionInArray_block._y] = null;

            //D�sactive la collision, pour permettre aux autres blocks de ne plus le reconnaitre
            gameObject.GetComponent<BoxCollider2D>().enabled = false;

            //regarde Si un block est s�par� du cockpit
            if (selfShip != null)
            {
                if (selfShip.blockCockpit)
                {
                    selfShip.connectedNeighbour = new List<BlockBehavior>();
                    selfShip.DeepFindNeighbour(selfShip.blockCockpit.GetComponent<BlockBehavior>(), ref selfShip.connectedNeighbour, this);
                }
            }

            //Liste de tous les bloques d�connect�s
            List<BlockBehavior> blocksToDisconnected = new List<BlockBehavior>();

            //Compare la liste de tous les bloques, avec la liste de tous les voisins connect�
            foreach (BlockBehavior block in selfShip.allBlocksBehaviorUnorganized)
            {
                if (!selfShip.connectedNeighbour.Contains(block))
                {
                    blocksToDisconnected.Add(block);
                }
            }

            if (blocksToDisconnected.Count > 1)
            {
                //On cr�er un objet parent qui va relier les blocks d�rivant entre eux,
                // On lui ajoute tous les composents d'un vaisseau
                GameObject craps = Instantiate(new GameObject(), null);
                Rigidbody2D rbd = craps.AddComponent<Rigidbody2D>();
                rbd.isKinematic = true;
                CompositeCollider2D coll = craps.AddComponent<CompositeCollider2D>();
                coll.isTrigger = true;
                SpaceShipBehavior spc = craps.AddComponent<SpaceShipBehavior>();
                DetectionDamage dtD = craps.AddComponent<DetectionDamage>();

                //On d�connecte les blocks
                foreach (BlockBehavior blockDisconnected in blocksToDisconnected)
                {
                    if (blockDisconnected != this)
                    {
                        blockDisconnected.isConnected = false;
                        blockDisconnected.transform.parent = craps.transform;
                        selfShip.allBlocks.Remove(blockDisconnected.id_block);
                        selfShip.allBlocksBehaviorUnorganized.Remove(blockDisconnected);
                        selfShip.allBlocksBehavior[blockDisconnected.positionInArray_block._x, positionInArray_block._y] = null;
                        spc.allBlocks.Add(blockDisconnected.id_block, blockDisconnected.gameObject);
                        blockDisconnected.selfShip = spc;
                        blockDisconnected.selfShip.allBlocksBehaviorUnorganized.Add(this);
                    }
                }

            }
            
            //Si aucun block n'est � s�parer, on enl�ve le block d�truit de la liste d�sorganis� de tous les blocks
            selfShip.allBlocksBehaviorUnorganized.Remove(this);
            

            //D�truit le block, si plus d'un bloc, sinon d�truit le vaisseau
            if (selfShip.allBlocks.Values.Count > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                if (transform.parent.gameObject != null)
                    Destroy(transform.parent.gameObject);
            }
        }
        else
        {
            selfShip.allBlocks.Remove(id_block);
            selfShip.allBlocksBehaviorUnorganized.Remove(this);
            //Met � jour les voisins
            foreach (BlockBehavior blockNeighbour in neighbourBlocks.Values)
            {
                if (blockNeighbour != null)
                {
                    blockNeighbour.neighbourBlocks = blockNeighbour.FindBlockNeighbour();
                }
            }

            if (selfShip.allBlocks.Values.Count > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject.transform.parent.gameObject);
            }

        }

    }

    //On va utiliser des raycast pour gagner en performance, et ne pas chercher dans une liste
    public Dictionary<NeighboorDirection, BlockBehavior> FindBlockNeighbour()
    {
        //Peut �tre sortie en param�tre
        float raycastDist = 0.9f;
        Dictionary<NeighboorDirection, BlockBehavior> findNeighbour = new Dictionary<NeighboorDirection, BlockBehavior>();

        //Renvoie un dictionnaire remplie de tous les voisins, avec en clef, leur position par rapport au block actuel
        for(int i =0; i < 4; i++)
        {
            BlockBehavior blockTuch = null;
            RaycastHit2D hit;
            switch (i)
            {
                case 0:
                    hit = Physics2D.Raycast(transform.position + new Vector3(0, (selfRenderer.bounds.size.y / 2) + 0.1f, 0), Vector2.up,raycastDist );
                    if(hit.collider != null)
                    {
                        if (hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();
                        }

                        findNeighbour.Add(NeighboorDirection.North, blockTuch);
                    }
                    
                    break;
                case 1:
                    hit = Physics2D.Raycast(transform.position + new Vector3(0, (-selfRenderer.bounds.size.y / 2) - 0.1f, 0), Vector2.down, raycastDist);
                    if (hit.collider != null )
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();
                        }

                        findNeighbour.Add(NeighboorDirection.South, blockTuch);
                    }
                    
                    break;
                case 2:
                    hit = Physics2D.Raycast(transform.position + new Vector3((selfRenderer.bounds.size.x / 2) + 0.1f, 0, 0), Vector2.right, raycastDist);
                    if (hit.collider != null)
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();
                        }
                        findNeighbour.Add(NeighboorDirection.East, blockTuch);
                    }
                    
                    break;
                case 3:
                    hit = Physics2D.Raycast(transform.position + new Vector3((-selfRenderer.bounds.size.x / 2) - 0.1f, 0, 0), Vector2.left, raycastDist);
                    if (hit.collider != null )
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();

                        }
                        findNeighbour.Add(NeighboorDirection.West, blockTuch);
                    }
                    break;
            }

        }

        return findNeighbour;
    }
    
    //Check si un block poss�de des voisins � l'exception d'un block
    private bool IsAlone(GameObject blockToIgnore)
    {
        
        foreach(BlockBehavior blockNeighbour in neighbourBlocks.Values)
        {
            if (blockNeighbour != null)
            {
                if (blockToIgnore.name != blockNeighbour.gameObject.name)
                {
                
                    return false;
                }
            }
        }
        return true;
    }

}

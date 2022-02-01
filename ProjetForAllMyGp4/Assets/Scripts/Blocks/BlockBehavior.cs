using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum direction
{
    up,
    down,
    left,
    right
}
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(BoxCollider2D))]
public class BlockBehavior : MonoBehaviour //Parent of all blocks
{
    #region paramètres modifiable
    [Header("Paramètre du block de base")]
    [Range(1,float.MaxValue)]
    public float block_LifePoint;
    public direction block_direction;
    #endregion

    #region paramètres interne
    [HideInInspector]
    public int id_block;
    protected static int id_toAdd;
    protected bool isAttachToMainShip = false;

    public Dictionary<string,BlockBehavior> neighbourBlocks;
    public (int _x, int _y) positionInArray_block;
    #endregion

    #region références

    [HideInInspector]
    public BoxCollider2D selfCollider;

    [HideInInspector]
    public SpaceShipBehavior selfShip;

    [HideInInspector]
    public SpriteRenderer selfRenderer;

    #endregion

    public void Awake()
    {
        //Ajoute les références à la main
        selfCollider = gameObject.GetComponent<BoxCollider2D>();
        selfShip = GetComponentInParent<SpaceShipBehavior>();
        selfRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public virtual void Start()
    {
        //Cherche som emplacement dans le tableau
        //Pour faire ça, on divise la local position du block, avec la taille de son sprite, pour nous donner le pas entre chaque bloque
        positionInArray_block = (Mathf.CeilToInt(transform.localPosition.x / (selfRenderer.bounds.size.x)), Mathf.CeilToInt(transform.localPosition.y / (selfRenderer.bounds.size.y)));

        //Créer un id unique pour le block
        id_block = id_toAdd;
        gameObject.name = id_block.ToString();

        //on l'ajoute au dictionnaire du comportement de spaceship
        if (selfShip != null)
            selfShip.allBlocks.Add(id_block, gameObject);

        id_toAdd++;
        //Trouve les voisins du block
        neighbourBlocks = FindBlockNeighbour();

    }

    //Ne pas mettre grand chose dans l'update, ça sera appelé dans chaque block
    private void Update()
    {

    }

    protected void CalculOwnVelocity()
    {

    }

    //Est appelé quand un tir est touché par le block
    public void HitSignal(float damage)
    {
        TakeDamage(damage);
    }

    //Enlève la vie du block
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
        //Met à jour la liste de tous les blocks du vaisseau
        selfShip.allBlocks.Remove(id_block);

        //Met a jour le tableau contenant tous les blocksBehaviors
        selfShip.allBlocksBehavior[positionInArray_block._x, positionInArray_block._y] = null;

        //Désactive la collision, pour permettre aux autres blocks de ne plus le reconnaitre
        gameObject.GetComponent<BoxCollider2D>().enabled = false;

        //Dictionary<string,BlockBehavior> blockNeighbours = new Dictionary<string, BlockBehavior>(neighbourBlocks);

        //Met à jour les voisins
        foreach (BlockBehavior blockNeighbour in neighbourBlocks.Values)
        {
            if (blockNeighbour != null)
            {
                blockNeighbour.neighbourBlocks = blockNeighbour.FindBlockNeighbour();
                //Si il n'a pas de voisin, le block n'appartient plus au vaisseau
                if (blockNeighbour.IsAlone(this.gameObject))
                {
                    blockNeighbour.transform.parent = null;
                }
            }
        }
        //Détruit le block;

        //Détruit le bloc, si plus d'un bloc, sinon détruit le vaisseau
        if (selfShip.allBlocks.Values.Count > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            if(transform.parent.gameObject != null)
                Destroy(transform.parent.gameObject);
        }

    }

    //On va utiliser des raycast pour gagner en performance, et ne pas chercher dans une liste
    public Dictionary<string, BlockBehavior> FindBlockNeighbour()
    {
        //Peut être sortie en paramètre
        float raycastDist = 0.9f;
        Dictionary<string, BlockBehavior> findNeighbour = new Dictionary<string, BlockBehavior>();

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
                    }
                    findNeighbour.Add("North", blockTuch);
                    break;
                case 1:
                    hit = Physics2D.Raycast(transform.position + new Vector3(0, (-selfRenderer.bounds.size.y / 2) - 0.1f, 0), Vector2.down, raycastDist);
                    if (hit.collider != null )
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();
                        }
                    }
                    findNeighbour.Add("South", blockTuch);
                    break;
                case 2:
                    hit = Physics2D.Raycast(transform.position + new Vector3((selfRenderer.bounds.size.x / 2) + 0.1f, 0, 0), Vector2.right, raycastDist);
                    if (hit.collider != null)
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();
                        }
                    }
                    findNeighbour.Add("East", blockTuch);
                    break;
                case 3:
                    hit = Physics2D.Raycast(transform.position + new Vector3((-selfRenderer.bounds.size.x / 2) - 0.1f, 0, 0), Vector2.left, raycastDist);
                    if (hit.collider != null )
                    {
                        if(hit.collider.gameObject.GetComponent<BlockBehavior>() != null)
                        {
                            blockTuch = hit.collider.gameObject.GetComponent<BlockBehavior>();

                        }
                    }
                    findNeighbour.Add("West", blockTuch);
                    break;
            }

        }

        return findNeighbour;
    }
    
    //Check si un block possède des voisins à l'exception d'un block
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

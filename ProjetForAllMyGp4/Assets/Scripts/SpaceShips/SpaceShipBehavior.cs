using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipBehavior : MonoBehaviour
{

    [HideInInspector]
    public Dictionary<int, GameObject> allBlocks = new Dictionary<int, GameObject>();

    //Tableau représeantant en 2d dimension, lesblocks behavior
    //les blocks étant rangé de cette manière (0,0), étant en bas à gauche, (max,max) en haut à droite
    [HideInInspector]
    public BlockBehavior[,] allBlocksBehavior;
    public int allBlocksBehaviorLength;

    void Start()
    {
        allBlocksBehaviorLength = 4;//Mathf.CeilToInt(Mathf.Sqrt(allBlocks.Values.Count));
        allBlocksBehavior = new BlockBehavior[allBlocksBehaviorLength, allBlocksBehaviorLength];

        FilledAllBlockBehavior();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Créer le tableau 2d de tous les blocks behavior, remplissage flood it (ajoute chaque voisin)
    private void FilledAllBlockBehavior()
    {
        Vector3 upLeftBlockPosition = Vector3.zero;

        (int _x, int _y) offsetResult = FindOffsetToAllBlockBehavior();

        foreach (GameObject blocks in allBlocks.Values)
        {
            //Pour ranger dans l'ordre d'affichage, on enlève l'offset, et on met tout en valeur absolue 
            int positionInArrayX = Mathf.Abs(blocks.GetComponent<BlockBehavior>().positionInArray_block._x - offsetResult._x);
            int positionInArrayY = Mathf.Abs(blocks.GetComponent<BlockBehavior>().positionInArray_block._y - offsetResult._y);

            //Lui donne son véritable emplacement dans le tableau
            blocks.GetComponent<BlockBehavior>().positionInArray_block = (positionInArrayX, positionInArrayY);
            allBlocksBehavior[positionInArrayX, positionInArrayY] = blocks.GetComponent<BlockBehavior>();
        
        }

    }

    //Cherche l'offset en fonction du nombre de valeur négative
    public (int _x, int _y) FindOffsetToAllBlockBehavior()
    {
        int maxOffsetY = 0;
        int maxOffsetX = 0;
        foreach (GameObject blocks in allBlocks.Values)
        {
            int positionInArrayX = blocks.GetComponent<BlockBehavior>().positionInArray_block._x;
            int positionInArrayY = blocks.GetComponent<BlockBehavior>().positionInArray_block._y;

            //On ne cherche que les blocks étant en coordonnée négative
            if(positionInArrayY < 0)
            {
                maxOffsetY = Mathf.Min(maxOffsetY, positionInArrayY);
            }
            if (positionInArrayX < 0)
            {
                maxOffsetX = Mathf.Min(maxOffsetX, positionInArrayX);
            }
            
        }
        return (Mathf.Abs(maxOffsetX), Mathf.Abs(maxOffsetY));
    }

}

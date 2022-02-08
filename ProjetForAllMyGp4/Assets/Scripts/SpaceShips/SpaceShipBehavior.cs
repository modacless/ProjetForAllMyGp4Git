using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpaceShipBehavior : MonoBehaviour
{

    [HideInInspector]
    public Dictionary<int, GameObject> allBlocks = new Dictionary<int, GameObject>();
    public List<BlockBehavior> allBlocksBehaviorUnorganized = new List<BlockBehavior>();

    //Tableau repr�seantant en 2d dimension, lesblocks behavior
    //les blocks �tant rang� de cette mani�re (0,0), �tant en bas � gauche, (max,max) en haut � droite
    [HideInInspector]
    public BlockBehavior[,] allBlocksBehavior;
    public int allBlocksBehaviorLength;

    //Permet de connaitre le block de controle plus simplement
    [HideInInspector]
    public GameObject blockCockpit;

    //Liste contenant tous les blockbehavior attach� au cockpit, se raffraichit juste quand le baisseau est touch�,
    //A chaque fois qu'on utlise DeepFindNeighbour, on doit clear la list
    public List<BlockBehavior> connectedNeighbour = new List<BlockBehavior>();

    void Start()
    {
        allBlocksBehaviorLength = 8;//Mathf.CeilToInt(Mathf.Sqrt(allBlocks.Values.Count));
        allBlocksBehavior = new BlockBehavior[allBlocksBehaviorLength, allBlocksBehaviorLength];

        FilledAllBlockBehavior();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Cr�er le tableau 2d de tous les blocks behavior, remplissage flood it (ajoute chaque voisin)
    private void FilledAllBlockBehavior()
    {
        Vector3 upLeftBlockPosition = Vector3.zero;

        (int _x, int _y) offsetResult = FindOffsetToAllBlockBehavior();

        foreach (GameObject blocks in allBlocks.Values)
        {
            //Pour ranger dans l'ordre d'affichage, on enl�ve l'offset, et on met tout en valeur absolue 
            int positionInArrayX = Mathf.Abs(blocks.GetComponent<BlockBehavior>().positionInArray_block._x - offsetResult._x);
            int positionInArrayY = Mathf.Abs(blocks.GetComponent<BlockBehavior>().positionInArray_block._y - offsetResult._y);

            //Lui donne son v�ritable emplacement dans le tableau
            blocks.GetComponent<BlockBehavior>().positionInArray_block = (positionInArrayX, positionInArrayY);
            allBlocksBehavior[positionInArrayX, positionInArrayY] = blocks.GetComponent<BlockBehavior>();

            //Ajoute dans une liste non organis�
            allBlocksBehaviorUnorganized.Add(blocks.GetComponent<BlockBehavior>());

        }

    }

    //Cherche l'offset en fonction du nombre de valeur n�gative
    public (int _x, int _y) FindOffsetToAllBlockBehavior()
    {
        int maxOffsetY = 0;
        int maxOffsetX = 0;
        foreach (GameObject blocks in allBlocks.Values)
        {
            int positionInArrayX = blocks.GetComponent<BlockBehavior>().positionInArray_block._x;
            int positionInArrayY = blocks.GetComponent<BlockBehavior>().positionInArray_block._y;

            //On ne cherche que les blocks �tant en coordonn�e n�gative
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

    //Cherche tous les bloques connect�s au cockpit, retourne toujours une liste, peut-�tre vide
    public List<BlockBehavior> FindUnConnectedBlock()
    {
        List<BlockBehavior> unConnectedBlock = new List<BlockBehavior>();
        List<BlockBehavior> connectedBlock = new List<BlockBehavior>();

        return unConnectedBlock;
    }

    //Permet de connaitre tous les blocks connect�es � un block
    public void DeepFindNeighbour(BlockBehavior objCockpit, ref List<BlockBehavior> seeNeighb, BlockBehavior exception)
    {
        if (objCockpit != null)
        {
            if (!seeNeighb.Contains(objCockpit) && exception != objCockpit && objCockpit.neighbourBlocks.Count > 0)
            {
                connectedNeighbour.Add(objCockpit);
                foreach (BlockBehavior val in objCockpit.neighbourBlocks.Values)
                {
                    DeepFindNeighbour(val,ref seeNeighb, exception);
                }
            }

        }

    }
}

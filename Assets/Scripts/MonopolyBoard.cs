using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet 
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();

    }

    public List<NodeSet> nodeSetList = new List<NodeSet>();

    private void Awake()
    {
        instance = this;
    }

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }
    }
    private void OnDrawGizmos()
    {
        if(route.Count>1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i + 1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    public void MovePlayerToken(int steps, Player player)
    {
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    public void MovePlayerToken(MonopolyNodeType type, Player player)
    {
        int indexOfNextNodeType = -1;//������ ������� ���� �����
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);//����� ��� �����
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0;//���������� ������ ��������� �����

        while (indexOfNextNodeType == -1 && nodeSearches < route.Count) //��������� ������ 
        {
            if(route[startSearchIndex].type == type)//������ ������� ���
            {
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;//�������� OutOfRange ������ (startSearchIndex + 1) �� ����� ������ route.Count
            nodeSearches++;
        }
        if(indexOfNextNodeType==-1)//���������� �����
        {
            //Debug.Log("������ �� �������!");
            return;
        }

        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        yield return new WaitForSeconds(0.5f);
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;//��� �������� � ���� ������������(������ � ��������)
        if (isMovingForward)
        {
            while (stepsLeft > 0)
            {
                indexOnBoard++;
                //�������� �� ����� ���� GO?
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true;
                }
                //��������� ��������� � �������� ����� ����������� + ����������� � ����� � ������(����� � ��������)
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;
                //���������:
                while (isMovedToNextNode(tokenToMove, endPos, 10))
                {
                    yield return null;
                }
                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft < 0)
            {
                indexOnBoard--;
                //�������� �� ����� ���� GO?
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count-1;
                }
                //��������� ��������� � �������� ����� ����������� + ����������� � ����� � ������(����� � ��������)
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;
                //���������:
                while (isMovedToNextNode(tokenToMove, endPos, 20))
                {
                    yield return null;
                }
                stepsLeft++;
            }
        }

        //������ �������� � ���� GO
        if(moveOverGo)
        {
            //���������� ����� �� �ר� 
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }
        //��������� ����� ����� CURRENT ������
        player.SetNewNode(route[indexOnBoard]);
    }

    bool isMovedToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }

    public (List<MonopolyNode> list, bool areAllSame) PlayerHasAllNodesOfSet(MonopolyNode node)
    {
        bool areAllSame = false;
        foreach  ( var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSetList.Contains(node))
            {
                //LINQ:
                areAllSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodesInSetList, areAllSame);
            }
        }
        return (null, areAllSame);
    }

    public MonopolyNode GetRandomPurchasableNode()
    {
        MonopolyNode selectedNode = null;
        bool isPurchasableNode = false;

        do
        {
            int randomIndex = Random.Range(0, route.Count);
            selectedNode = route[randomIndex];

            if(selectedNode.type == MonopolyNodeType.Property || selectedNode.type == MonopolyNodeType.Utility)
            {
                isPurchasableNode = true;
            }
        }
        while (!isPurchasableNode);

        return selectedNode;
    }
}

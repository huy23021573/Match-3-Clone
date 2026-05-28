using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Dot : MonoBehaviour
{

    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private HintManager hintManager;
    private FindMatches findMatches;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMaker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;


    // Use this for initialization
    void Start()
    {

        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        hintManager = FindAnyObjectByType<HintManager>();
        board = FindAnyObjectByType<Board>();
        findMatches = FindAnyObjectByType<FindMatches>();

    }


    //This is for testing and Debug only.

    // Update is called once per frame
    void Update()
    {

        targetX = column;
        targetY = row;
        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;

        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //Move Towards the target
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();

        }
        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;

        }
    }

    public IEnumerator CheckMoveCo()
    {
        Dot otherDot1 = otherDot.GetComponent<Dot>();

        if ((isColumnBomb || isRowBomb) && (otherDot1.isColumnBomb || otherDot1.isRowBomb))
        {
            isColumnBomb = false;
            isRowBomb = false;
            otherDot1.isColumnBomb = false;
            otherDot1.isRowBomb = false;

            List<GameObject> dots = new List<GameObject>();
            dots.Union(findMatches.GetColumnPieces(column));
            dots.Union(findMatches.GetRowPieces(row));
        }

        if ((isColumnBomb && otherDot1.isAdjacentBomb) || (otherDot1.isColumnBomb && isAdjacentBomb))
        {
            isColumnBomb = false;
            isAdjacentBomb = false;
            otherDot1.isColumnBomb = false;
            otherDot1.isAdjacentBomb = false;

            List<GameObject> dots = new List<GameObject>();

            if (column == 0)
            {
                dots.Union(findMatches.GetColumnPieces(column));
                dots.Union(findMatches.GetColumnPieces(column + 1));
            }
            else if (column > 0 && column < board.width - 1)
            {
                dots.Union(findMatches.GetColumnPieces(column));
                dots.Union(findMatches.GetColumnPieces(column - 1));
                dots.Union(findMatches.GetColumnPieces(column + 1));
            }
            else if (column == board.width - 1)
            {
                dots.Union(findMatches.GetColumnPieces(column));
                dots.Union(findMatches.GetColumnPieces(column - 1));
            }
        }
        if ((isRowBomb && otherDot1.isAdjacentBomb) || (otherDot1.isRowBomb && isAdjacentBomb))
        {
            isRowBomb = false;
            isAdjacentBomb = false;
            otherDot1.isRowBomb = false;
            otherDot1.isAdjacentBomb = false;

            List<GameObject> dots = new List<GameObject>();

             if (row == 0)
            {
                dots.Union(findMatches.GetRowPieces(row));
                dots.Union(findMatches.GetRowPieces(row + 1));
            }
            else if (row > 0 && row < board.height - 1)
            {
                dots.Union(findMatches.GetRowPieces(row));
                dots.Union(findMatches.GetRowPieces(row - 1));
                dots.Union(findMatches.GetRowPieces(row + 1));
            } else if (row == board.height - 1)
            {
                dots.Union(findMatches.GetRowPieces(row));
                dots.Union(findMatches.GetRowPieces(row - 1));
            }
        }

        if (isColorBomb)
        {
                //This piece is a color bomb, and the other piece is the color to destroy
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }
        else if (otherDot1.isColorBomb)
        {
            //The other piece is a color bomb, and this piece has the color to destroy
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot1.isMatched = true;
        }
        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot1.isMatched)
            {
                otherDot1.row = row;
                otherDot1.column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.4f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            //otherDot = null;
        }

    }

    private void OnMouseDown()
    {
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {   board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;

        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePiecesReal(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else board.currentState = GameState.move;
    }
    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //Right Swipe
            MovePiecesReal(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            //Up Swipe
            MovePiecesReal(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            //Left Swipe
            MovePiecesReal(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            //Down Swipe
            MovePiecesReal(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
        this.gameObject.tag = "Color";
    }
    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject maker = Instantiate(adjacentMaker, transform.position, Quaternion.identity);
        maker.transform.parent = this.transform;

    }
}
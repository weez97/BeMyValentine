using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// public enum Direction { Up, Down, Left, Right }
public enum Axis { Horizontal, Vertical }

public class PuzzlePiece : MonoBehaviour, IPointerClickHandler
{
    public int PieceNumber { get; private set; }
    private Image image;
    private bool valid;

    public void SetUpPiece(Sprite sprite, int number, bool valid)
    {
        image = GetComponent<Image>();
        if (sprite != null)
            image.sprite = sprite;
        else
            image.color = Color.clear;
        PieceNumber = number;
        name = $"Piece {PieceNumber}";
        this.valid = valid;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!valid) return;
        MovePiece();
    }

    private void MovePiece()
    {
        PuzzleController.Instance.TryToMovePiece(PieceNumber);
    }
}